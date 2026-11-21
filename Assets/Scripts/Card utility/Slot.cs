using UnityEngine;
using TMPro;

[System.Serializable]
public class Slot : MonoBehaviour, ICardHolder
{
    public Card currentCard;
    Table table;

    public TMP_Text cardNumber1;
    public TMP_Text cardNumber2;

    public GameObject highighter;

    //true if there are cards
    public bool hasCards() 
    {
        if (currentCard != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //returns null if there arent any cards
    public Card takeTopCard() 
    {
        Card temporary = currentCard;
        currentCard = null;

        return temporary;
    }

    public Card peekTop()
    {
        return currentCard;
    }

    void EmptyVisuals() 
    {
    }

    void UpdateVisual() 
    {
        cardNumber1.text = currentCard.value + "";
        cardNumber2.text = currentCard.value + "";
    }

    public void PutCard(Card card)
    {
        currentCard = card;
        Debug.Log("put: " + card);

        UpdateVisual();
    }

    void Start()
    {
    }

    void Update()
    {
        
    }

    public void PutOnTable(Table _table)
    {
        table = _table;
    }
}
