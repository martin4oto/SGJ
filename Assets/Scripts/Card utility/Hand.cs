using UnityEngine;

public class Hand : MonoBehaviour, ICardHolder
{
    Card holdingCard;

    //true if there are cards
    public bool hasCards() 
    {
        return holdingCard != null;
    }

    //returns null if there arent any cards
    public Card takeTopCard()
    {
        Card temp = holdingCard;
        holdingCard = null;
        return temp;
    }

    public Card peekTop() 
    {
        return holdingCard;
    }

    public void PutCard(Card card) 
    {
        if (card == null) 
        {
            Debug.Log("tried to put a null card in hand");
        }

        holdingCard = card;
    }
}
