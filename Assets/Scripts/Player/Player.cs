using UnityEngine;

public class Player : MonoBehaviour
{
    public Table table;
    public Hand hand;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    bool firstFrameTest = true;
    void Update()
    {
        if (firstFrameTest) 
        {
            DrawCard();
        }
    }

    public void DrawCard()
    {
        if (!hand.hasCards())
        {
            Card card = Deck.instance.takeTopCard();
            hand.PutCard(card);
        }
    }

    public void ChangeSelection(int move) 
    {
    }

    public void CardInteract() 
    {
    }

    public void Hit() 
    {
        Card card = Deck.instance.takeTopCard();
        table.PutNewCard(card);
    }
}
