using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

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
    public ParticleSystem lowSmoke;
    public ParticleSystem highSmoke;
    public ParticleSystem firePart;

    public SlamHand slamHand;
    float holdThreshold = 0.2f;
    bool isHoldingA;
    bool pressedA;
    float pressedATime;
    bool isHoldingB;
    bool pressedB;
    float pressedBTime;
    bool isSmoking = false;
    float timeOfLastStickMove;
    float stickDelay = 0.2f;

    public Player otherPlayer;
    float burnTimer = 0;
    public float maxBurnTimer;
    public GameObject cardText;
    bool firstFrameTest = true;
    void Update()
    {
        float modif = 1; 
        if (Dealer.LookingAtId() == playerIndex) modif = 1.5f;

        obsession += Time.deltaTime * modif;
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
            ObsessionBarFix();

            firstFrameTest = false;
        }

        if (isHoldingA && isHoldingB && hand.hasCards())
        {
            //fire
            if (!firePart.isPlaying) firePart.Play();
            burnTimer += Time.deltaTime;
            Dealer.playerSuspiciousActivity(playerIndex, " burning a card...");

            if (burnTimer > maxBurnTimer) 
            {
                burnTimer = 0;
                Burn();
            }
        }
        else
        {
            burnTimer = 0;
            if (firePart.isPlaying) firePart.Stop();
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
        //Debug.Log(context.performed.ToString() + timeOfLastStickMove + " " + (Time.time - stickDelay) + "r?: " + (timeOfLastStickMove < Time.time - stickDelay));
        if (!context.performed || timeOfLastStickMove > Time.time - stickDelay) return;

        Vector2 value = context.ReadValue<Vector2>();

        timeOfLastStickMove = Time.time;

        int rawValueX = (int)(Mathf.Sign(value.x) * Mathf.Ceil(Mathf.Abs(value.x)));

        if (playerIndex == 1) rawValueX *= -1;
        Debug.Log("throw test: " + rawValueX);

        if (isHoldingA && rawValueX != 0) 
        {
            Debug.Log("throw test yes");
            if (ThrowCardToOther())
            {
                return;
            }
        }

        int rawValueY = (int)(Mathf.Sign(value.y) * Mathf.Ceil(Mathf.Abs(value.y)));

        if (table.TryToSelectThrown(rawValueY))
        {
            table.Select(rawValueX);
        }
    }

    int blackJackCounter = 0;
    public TMP_Text blackjackCounterObject;
    static int numberOfBlackjacks = 0;
    //starts exactly when the player reaches 21
    void SumReached()
    {
        GetComponent<AudioSource>().Play();
        blackJackCounter++;
        numberOfBlackjacks++;

        switch (numberOfBlackjacks)
        {
            case 4:
                maxObsession = 30;
                ObsessionBarFix();
                break;
            case 6:
                calmingEffect = 1.25f;
                break;
            case 8:
                maxObsession = 20;
                ObsessionBarFix();
                break;
            case 10:
                calmingEffect = 0.75f;
                break;
            case 12:
                maxObsession = 15;
                ObsessionBarFix();
                break;
            case 18:
                maxObsession = 7;
                calmingEffect = 0.5f;
                ObsessionBarFix();
                break;
        }



        blackjackCounterObject.text = "HANDS WON:" + blackJackCounter;
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
        Dealer.playerSuspiciousActivity(playerIndex, " moving cards around...");
        Card tableCard = table.SwapCard(hand.takeTopCard());

        if (tableCard != null)
        {
            hand.PutCard(tableCard);
            hand.SwapSprite(rightHandHold, false);
            cardText.SetActive(true);
        }
        else
        {
            hand.SwapSprite(rightHandDefault, false);
            cardText.SetActive(false);
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

    bool isTextVisible;
    public void fixText() 
    {
        cardText.SetActive(isTextVisible);
    }

    public void Hit()
    {
        if (table.canPutAnother())
        {
            Dealer.instance.ChangeId(playerIndex);
            if (hand.hasCards())
            {
                hand.lastSprite = rightHandHold;
            }
            else
            {
                hand.lastSprite = rightHandDefault;
            }
            isTextVisible = cardText.activeSelf;
            hand.SwapSprite(rightHandPoint, isTextVisible);
            cardText.SetActive(false);
            hand.Travel(hand.defaultPoint, hand.slamPoint,  450f, 4);
            
            hand.slamEnd = true;
            Card card = Deck.instance.takeTopCard();
            table.PutNewCard(card);
            obsession = 0;

            
            int sum = table.GetSum();

            if (sum == 21)
            {
                SumReached();
            }
        }
        else
        {
            int pId = playerIndex == 0 ? 1 : 2;
            EndScreen.TriggerEndScreen("Player " + pId + " couldn't hit as their table was full...");
        }
    }

    public void Burn()
    {
        if (hand.peekTop() != null)
        {
            hand.takeTopCard();
            Dealer.instance.score += 10;
            hand.SwapSprite(rightHandDefault, false);
            cardText.SetActive(false);
        }
    }

    public void StartSmoking()
    {
        if (isSmoking) return;
        isSmoking = true;
        slamHand.StartSmoking();
        slamHand.SwapSprite(leftHandSmoke);
        lowSmoke.Stop();
        highSmoke.Play();
    }

    public void StopSmoking()
    {
        if (!isSmoking) return;
        isSmoking = false;
        slamHand.StopSmoking();
        slamHand.SwapSprite(leftHandDefault);
        lowSmoke.Play();
        highSmoke.Stop();
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
        Dealer.playerSuspiciousActivity(playerIndex, " had a card thrown to him...");
    }

    bool ThrowCardToOther() 
    {
        if (hand.peekTop() != null && otherPlayer.canCatch()) 
        {
            otherPlayer.CatchCard(hand.takeTopCard());
            hand.SwapSprite(rightHandDefault, false);
            cardText.SetActive(false);

            return true;
        }
        return false;
    }

    void ObsessionBarFix() 
    {
        slider.maxValue = maxObsession;
    }
}
