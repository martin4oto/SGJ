using UnityEngine;

public interface ICardHolder
{
    //true if there are cards
    public bool hasCards();

    //returns null if there arent any cards
    public Card takeTopCard();
}
