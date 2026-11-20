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
            Draw();
        }
    }
    public void Draw()
    {
        if (!hand.hasCards())
        {
            Card card = Deck.instance.takeTopCard();
            hand.PutCard(card);
        }
    }
    public void DrawCard(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // Trqbva da se sloji oshte edin condition za tova dali e selectnat draw pile-a sega

        Draw();
    }

    public void ChangeSelection(InputAction.CallbackContext context) 
    {
        Vector2 value = context.ReadValue<Vector2>();
        table.Select(Mathf.CeilToInt(value.x));
    }

    public void CardInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

    }
    void OnDisable()
    {
        InputManager.instance.players[playerIndex].Subscribe("ButtonA", CardInteract);
        InputManager.instance.players[playerIndex].Subscribe("ButtonA", DrawCard);
        InputManager.instance.players[playerIndex].Subscribe("Stick", ChangeSelection);
    }
    void OnEnable()
    {
        InputManager.instance.players[playerIndex].Unsubscribe("ButtonA", CardInteract);
        InputManager.instance.players[playerIndex].Unsubscribe("ButtonA", DrawCard);
        InputManager.instance.players[playerIndex].Unsubscribe("Stick", ChangeSelection);
    }
}
