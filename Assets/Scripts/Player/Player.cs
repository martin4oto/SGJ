using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Table table;
    public Hand hand;
    public int playerIndex;
    public Slider slider;
    float obsession = 0;
    public float maxObsession;
    public float calmingEffect;
    public Sprite leftHandDefault;
    public Sprite leftHandSmoke;
    public Sprite rightHandDefault;
    public Sprite rightHandPoint;
    public Sprite rightHandHold;

    public SlamHand slamHand;
    float holdThreshold = 0.2f;
    bool isHoldingA;
    bool pressedA;
    float pressedATime;
    bool isHoldingB;
    bool pressedB;
    float pressedBTime;
    bool isSmoking = false;

    public Player otherPlayer;


    float burnTimer = 0;
    public float maxBurnTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    bool firstFrameTest = true;
    void Update()
    {
        obsession += Time.deltaTime;
        slider.value = obsession;

        if (isSmoking && !isHoldingA)
        {
            obsession -= Time.deltaTime * calmingEffect;
            if (obsession < 0) obsession = 0;
        }

        if (obsession > maxObsession)
        {
            Hit();
            obsession = 0;
        }

        if (firstFrameTest)
        {
            for (int i = 0; i < 2; i++)
            {
                Card card = Deck.instance.takeTopCard();
                table.PutNewCard(card);
            }

            //Card card1 = Deck.instance.takeTopCard();
            //table.ThrowCard(card1);

            firstFrameTest = false;
        }

        if (isHoldingA && isHoldingB)
        {
            //fire
            burnTimer += Time.deltaTime;
            Dealer.playerSuspiciousActivity(playerIndex);

            if (burnTimer > maxBurnTimer) 
            {
                burnTimer = 0;
                Burn();
            }
        }
        else
        {
            burnTimer = 0;
        }

        if (pressedA && pressedATime <= Time.time - holdThreshold)
        {
            if (!isHoldingA)
            {
                hand.StartHoldHandUp();
            }
            isHoldingA = true;
        }
        if (pressedB && pressedBTime <= Time.time - holdThreshold)
        {
            if (!isHoldingB)
            {
                StartSmoking();
            }
            isHoldingB = true;
        }
    }

    public void ChangeSelection(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector2 value = context.ReadValue<Vector2>();

        int rawValueX = Mathf.CeilToInt(value.x);

        if (rawValueX == 0) rawValueX = -1;

        if (playerIndex == 1) rawValueX *= -1;

        if (isHoldingA && rawValueX == 1) 
        {
            if (ThrowCardToOther())
            {
                return;
            }
        }

        int rawValueY = Mathf.CeilToInt(value.y);

        if (rawValueY == 0) rawValueY = -1;

        if (table.TryToSelectThrown(rawValueY))
        {
            table.Select(rawValueX);
        }
    }

    //starts exactly when the player reaches 21
    void SumReached()
    {
        table.clearTable();
    }
    void InputA(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            pressedA = true;
            pressedATime = Time.time;
        }

        if (context.canceled)
        {
            if (!isHoldingA)
            {
                CardInteract();
            }
            else
            {
                hand.StopHoldHandUp();
            }
            pressedA = false;
            isHoldingA = false;
        }
    }
    void InputB(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            pressedB = true;
            pressedBTime = Time.time;
        }

        if (context.canceled)
        {
            if (!isHoldingB)
            {
                Slam();
            }else{
                StopSmoking();
            }
            pressedB = false;
            isHoldingB = false;
        }
    }
    public void CardInteract()
    {
        Card tableCard = table.SwapCard(hand.takeTopCard());

        if (tableCard != null)
        {
            hand.PutCard(tableCard);
            hand.SwapSprite(rightHandHold);
        }
        else
        {
            hand.SwapSprite(rightHandDefault);
        }

        int sum = table.GetSum();

        if (sum == 21)
        {
            SumReached();
        }
    }
    void OnDisable()
    {
        InputManager.instance.players[playerIndex].Unsubscribe("ButtonC", Hit);
        InputManager.instance.players[playerIndex].Unsubscribe("ButtonB", InputB);
        InputManager.instance.players[playerIndex].Unsubscribe("ButtonA", InputA);
        InputManager.instance.players[playerIndex].Unsubscribe("Stick", ChangeSelection);
    }
    void OnEnable()
    {
        InputManager.instance.players[playerIndex].Subscribe("ButtonC", Hit);
        InputManager.instance.players[playerIndex].Subscribe("ButtonB", InputB);
        InputManager.instance.players[playerIndex].Subscribe("ButtonA", InputA);
        InputManager.instance.players[playerIndex].Subscribe("Stick", ChangeSelection);
    }

    public void Hit(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Hit();
    }

    public void Hit()
    {
        if (table.canPutAnother())
        {
            Card card = Deck.instance.takeTopCard();
            table.PutNewCard(card);
            obsession = 0;
        }
    }

    public void Burn()
    {
        if (hand.peekTop() != null)
        {
            hand.takeTopCard();
            hand.SwapSprite(rightHandDefault);
        }
    }

    public void StartSmoking()
    {
        if (isSmoking) return;
        isSmoking = true;
        slamHand.StartSmoking();
        slamHand.SwapSprite(leftHandSmoke);
    }

    public void StopSmoking()
    {
        if (!isSmoking) return;
        isSmoking = false;
        slamHand.StopSmoking();
        slamHand.SwapSprite(leftHandDefault);
    }

    public void Slam()
    {
        slamHand.Slam(playerIndex);
        slamHand.SwapSprite(leftHandDefault);
    }

    public bool canCatch() 
    {
        return !table.hasThrownCard();
    }

    void CatchCard(Card card) 
    {
        table.ThrowCard(card);
        Dealer.playerSuspiciousActivity(playerIndex);
    }

    bool ThrowCardToOther() 
    {
        if (hand.peekTop() != null && otherPlayer.canCatch()) 
        {
            otherPlayer.CatchCard(hand.takeTopCard());
            hand.SwapSprite(rightHandDefault);
            return true;
        }
        return false;
    }
}
