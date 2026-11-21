using UnityEngine;
using TMPro;

public class Table : MonoBehaviour
{
    public GameObject slotPrefab;
    public GameObject highlighterPrefab;
    public int maximumTableSlots;

    public Transform tableRow;
    public Transform highlighterRow;

    public Slot[] allSlots;
    GameObject[] highlights;

    int numberOfCardsPresnt;

    int currentSelected;
    public int playerIndex;

    public TMP_Text scoreText;

    public Slot ThrownCardSlot;
    public GameObject thrownHighlight;

    bool thrownSelected = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSelected = 0;
        numberOfCardsPresnt = 0;

        allSlots = new Slot[maximumTableSlots];
        highlights = new GameObject[maximumTableSlots + 1];

        for (int i = 0; i < maximumTableSlots; i++)
        {
            Transform currentSlot = Instantiate(slotPrefab).transform;

            currentSlot.SetParent(tableRow);
            currentSlot.localScale = Vector3.one;

            allSlots[i] = currentSlot.GetComponent<Slot>();
            allSlots[i].currentCard = null;

            allSlots[i].gameObject.SetActive(false);

            highlights[i] = Instantiate(highlighterPrefab, highlighterRow).transform.GetChild(0).gameObject;
        }

        Debug.Log(Deck.instance);
        highlights[highlights.Length - 1] = Instantiate(highlighterPrefab, Deck.instance.transform).transform.GetChild(0).gameObject;

        ThrownCardSlot.currentCard = null;

        highlights[0].SetActive(true);
    }

    void PutAtTheEnd() 
    {
        Slot slotToGoBack = allSlots[currentSelected];
        slotToGoBack.gameObject.SetActive(false);

        for (int i = currentSelected; i < allSlots.Length-1; i++)
        {
            allSlots[i] = allSlots[i + 1];
        }
        allSlots[allSlots.Length - 1] = slotToGoBack;

        slotToGoBack.transform.SetAsLastSibling();
    }

    void UpdateSelectorObject(int move) 
    {
        highlights[currentSelected].SetActive(false);
        highlights[currentSelected + move].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Select(int move)
    {
        currentSelected += move;

        if (currentSelected< 0 || currentSelected> maximumTableSlots)
        {
            currentSelected -= move;
        }
        else
        {
            currentSelected -= move;
            UpdateSelectorObject(move);
            currentSelected += move;
        }
    }

    Card DrawCard(Card _card)
    {
        if (_card != null) return _card;

        return Deck.instance.takeTopCard();
    }

    Card GetThrownCard(Card _card)
    {
        if (_card != null) return _card;

        Card card = ThrownCardSlot.takeTopCard();
        ThrownCardSlot.gameObject.SetActive(false);

        currentSelected = 0;
        highlights[currentSelected].SetActive(true);

        thrownSelected = false;

        return card;
    }

    //if _card == null it just takes it
    //if there is no card in the slot, it only puts the card in the current slot
    //returns a null if there is no card to take
    public Card SwapCard(Card _card) 
    {
        if (thrownSelected) 
        {
            return GetThrownCard(_card);
        }

        if (currentSelected >= maximumTableSlots) 
        {
            return DrawCard(_card);
        }

        Card returnCard = allSlots[currentSelected].takeTopCard();

        Debug.Log(_card + " " + returnCard);

        if (_card != null && returnCard != null) 
        {
            allSlots[currentSelected].PutCard(_card);
        }
        else if (returnCard != null && _card == null)
        {
            numberOfCardsPresnt--;
            PutAtTheEnd();
        }
        else if(_card != null && returnCard == null) 
        {
            PutNewCard(_card);
            return returnCard;
        }
        CalculatePoints();

        return returnCard;
    }

    public void PutNewCard(Card card) 
    {
        if (card == null) 
        {
            Debug.Log("tried to push a null card");
            return;
        }

        allSlots[numberOfCardsPresnt].gameObject.SetActive(true);

        allSlots[numberOfCardsPresnt].PutCard(card);
        numberOfCardsPresnt++;
        CalculatePoints();
    }

    public int size() 
    {
        return numberOfCardsPresnt;
    }

    void CalculatePoints() 
    {
        int points = GetSum();
        scoreText.text = "Score:" + points;
    }

    public int GetSum() 
    {
        int sum = 0;
        for (int i = 0; i < numberOfCardsPresnt; i++)
        {
            if (allSlots[i].peekTop() == null) continue;
            sum += allSlots[i].peekTop().value;
        }

        return sum;
    }

    public void clearTable()
    {
        for (int i = 0; i < numberOfCardsPresnt; i++)
        {
            allSlots[i].takeTopCard();
            allSlots[i].gameObject.SetActive(false);
        }
        Dealer.instance.score += 100;
        CalculatePoints();
        numberOfCardsPresnt = 0;
        Dealer.instance.ChangeId(playerIndex);
    }

    public void ThrowCard(Card card) 
    {
        if (hasThrownCard()) return;

        ThrownCardSlot.gameObject.SetActive(true);
        ThrownCardSlot.PutCard(card);
    }

    public bool hasThrownCard() 
    {
        return ThrownCardSlot.hasCards();
    }

    public bool TryToSelectThrown(int y)
    {
        if (y == -1)
        {
            if (!thrownSelected)
            {
                thrownHighlight.SetActive(true);
                thrownSelected = true;

                highlights[currentSelected].SetActive(false);
                return false;
            }
            return false;
        }
        else if (y == 1)
        {
            if (thrownSelected)
            {
                thrownHighlight.SetActive(false);
                thrownSelected = false;

                highlights[currentSelected].SetActive(true);
                return false;
            }
            return true;
        }
        return true;
    }

    public bool canPutAnother() 
    {
        return size() < maximumTableSlots;
    }
}
