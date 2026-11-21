using UnityEngine;

public class Hand : MonoBehaviour, ICardHolder
{
    public Slot holdingCard;
    bool holdsCard = false;

    void Start() 
    {
        holdingCard.gameObject.SetActive(false);
    }

    //true if there are cards
    public bool hasCards() 
    {
        return holdsCard;
    }

    //returns null if there arent any cards
    public Card takeTopCard()
    {
        if (!holdsCard) return null;

        holdsCard = false;
        holdingCard.gameObject.SetActive(false);

        return holdingCard.takeTopCard();
    }

    public Card peekTop()
    {
        if (!holdsCard) return null;
        return holdingCard.peekTop();
    }

    public void PutCard(Card card)
    {
        if (card == null)
        {
            Debug.Log("tried to put a null card in hand");
        }
        else 
        {
            holdsCard = true;
            holdingCard.gameObject.SetActive(true);
        }
        holdingCard.PutCard(card);
    }
}
