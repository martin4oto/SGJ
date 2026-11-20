using UnityEngine;

public class Table : MonoBehaviour
{
    public GameObject slotPrefab;
    public GameObject highlighterPrefab;
    public int maximumTableSlots;

    public Transform tableRow;
    public Transform highlighterRow;

    Slot[] allSlots;
    GameObject[] highlights;

    int numberOfCardsPresnt;

    int currentSelected;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentSelected = 0;
        numberOfCardsPresnt = 0;

        allSlots = new Slot[maximumTableSlots];
        highlights = new GameObject[maximumTableSlots];

        for (int i = 0; i < maximumTableSlots; i++)
        {
            Transform currentSlot = Instantiate(slotPrefab).transform;

            currentSlot.SetParent(tableRow);

            allSlots[i] = currentSlot.GetComponent<Slot>();

            allSlots[i].gameObject.SetActive(false);

            highlights[i] = Instantiate(highlighterPrefab, highlighterRow);
        }

        highlights[0].SetActive(true);
    }

    void PutAtTheEnd() 
    {
        Slot slotToGoBack = allSlots[currentSelected];

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
        UpdateSelectorObject(move);
        currentSelected += move;
    }

    //if _card == null it just takes it
    //if there is no card in the slot, it only puts the card in the current slot
    //returns a null if there is no card to take
    public Card SwapCard(Card _card) 
    {
        Card returnCard = allSlots[currentSelected].takeTopCard();

        if (_card != null) 
        {
            allSlots[currentSelected].PutCard(_card);
        }
        else if (returnCard != null && _card == null)
        {
            numberOfCardsPresnt--;
            PutAtTheEnd();
        }

        if(_card != null && returnCard == null) 
        {
            PutNewCard(_card);
        }

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
    }
}
