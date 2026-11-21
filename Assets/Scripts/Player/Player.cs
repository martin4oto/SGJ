using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Table table;
    public Hand hand;
    public int playerIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    bool firstFrameTest = true;
    void Update()
    {
        if (firstFrameTest) 
        {
            for (int i = 0; i < 2; i++)
            {
                Card card = Deck.instance.takeTopCard();
                table.PutNewCard(card);
            }

            Card card1 = Deck.instance.takeTopCard();
            table.ThrowCard(card1);

            firstFrameTest = false;
        }
    }

    public void ChangeSelection(InputAction.CallbackContext context) 
    {
        if (!context.performed) return;

        Vector2 value = context.ReadValue<Vector2>();

        int rawValueX = Mathf.CeilToInt(value.x);

        if (rawValueX == 0) rawValueX = -1;

        if (playerIndex == 1) rawValueX *= -1;

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

    public void CardInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Card tableCard = table.SwapCard(hand.takeTopCard());

        if (tableCard != null) 
        {
            hand.PutCard(tableCard);
        }

        int sum = table.GetSum();

        if(sum == 21) 
        {
            SumReached();
        }
    }
    void OnDisable()
    {
        InputManager.instance.players[playerIndex].Unsubscribe("ButtonB", Burn);
        InputManager.instance.players[playerIndex].Unsubscribe("ButtonA", CardInteract);
        InputManager.instance.players[playerIndex].Unsubscribe("Stick", ChangeSelection);
    }
    void OnEnable()
    {
        InputManager.instance.players[playerIndex].Subscribe("ButtonB", Burn);
        InputManager.instance.players[playerIndex].Subscribe("ButtonA", CardInteract);
        InputManager.instance.players[playerIndex].Subscribe("Stick", ChangeSelection);
    }

    public void Hit() 
    {
        Card card = Deck.instance.takeTopCard();
        table.PutNewCard(card);
    }

    public void Burn(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (hand.peekTop() != null) 
        {
            hand.takeTopCard();
        }
    }
}
