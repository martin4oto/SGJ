using UnityEngine;
using UnityEngine.UI;

public class Hand : MonoBehaviour, ICardHolder
{
    public Slot holdingCard;
    bool holdsCard = false;
    Vector3 defaultPoint;
    Vector3 pointA;
    Vector3 pointB;
    float totalTravelTime;
    float timer;
    bool traveling;
    public Vector3 holdPoint;
    float curveComponent;

    void Awake()
    {
        defaultPoint = transform.localPosition;
        holdPoint = defaultPoint + new Vector3(-200, 200, 0);
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
        //holdingCard.gameObject.SetActive(false);

        return holdingCard.takeTopCard();
    }

    public Card peekTop()
    {
        if (!holdsCard) return null;
        return holdingCard.peekTop();
    }
    public void StartHoldHandUp()
    {
        Travel(defaultPoint, holdPoint, 400, 4);
    }

    public void StopHoldHandUp()
    {
        Travel(holdPoint, defaultPoint, 400, 4);
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
            //holdingCard.gameObject.SetActive(true);
        }
        holdingCard.PutCard(card);
    }
    public void SwapSprite(Sprite newSprite)
    {
        transform.GetChild(0).GetComponent<Image>().sprite = newSprite;
    }

    void Update()
    {
        if (traveling)
        {
            transform.localPosition = Vector3.Lerp(pointA, pointB, 1 - Mathf.Pow(timer/totalTravelTime, curveComponent));
            timer -= Time.deltaTime;
            if (timer <= 0) traveling = false;
        }
    }

    public void Travel(Vector3 a, Vector3 b, float pixelsPerSecond, float cc)
    {
        pointA = a;
        pointB = b;
        totalTravelTime = Vector3.Distance(pointA, pointB)/pixelsPerSecond;
        timer = totalTravelTime;
        curveComponent = cc;
        traveling = true;
    }
}
