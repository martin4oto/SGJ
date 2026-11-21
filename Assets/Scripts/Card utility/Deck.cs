using UnityEngine;
using System.Collections.Generic;

public class Deck : MonoBehaviour, ICardHolder
{
    List<Card> allCards;
    public static Deck instance;

    void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allCards = new List<Card>();
        GenerateDeck();
        Shuffle();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateDeck()
    {
        allCards.Clear();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 1; j <= 11; j++)
            {
                allCards.Add(new Card(j));
            }
        }
    }

    public void Shuffle() 
    {
        for (int i = 0; i < allCards.Count; i++)
        {
            for (int j = 0; j < allCards.Count; j++)
            {
                int index = (int)Random.Range(0, allCards.Count);

                Card temp = allCards[index];

                allCards[index] = allCards[j];
                allCards[j] = temp;
            }
        }
    }

    //true if there are cards
    public bool hasCards() 
    {
        return allCards.Count != 0;
    }

    //returns null if there arent any cards
    public Card takeTopCard() 
    {
        Card card = allCards[allCards.Count - 1];
        allCards.RemoveAt(allCards.Count - 1);

        if (allCards.Count == 0) 
        {
            GenerateDeck();
            Shuffle();
        }

        return card;
    }

    public Card peekTop() 
    {
        return allCards[allCards.Count - 1];
    }

    public void PutCard(Card card) 
    {
        if (card == null) 
        {
            Debug.Log("trying to add a null card to the deck");
        }
        allCards.Add(card);
    }
}
