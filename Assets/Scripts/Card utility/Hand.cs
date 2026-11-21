using UnityEngine;
using UnityEngine.UI;

public class Hand : MonoBehaviour, ICardHolder
{
    public Slot holdingCard;
    bool holdsCard = false;
    public Vector3 defaultPoint;
    public Vector3 slamPoint;
    public Vector3 pointPoint;
    Vector3 pointA;
    Vector3 pointB;
    float totalTravelTime;
    float timer;
    bool traveling;
    public Vector3 holdPoint;
    float curveComponent;
    public bool slamEnd;
    public Sprite lastSprite;
    float switchTimer;
    public bool isTextEnabled;
    public GameObject CardText;

    void Awake()
    {
        defaultPoint = transform.localPosition;
        slamPoint = defaultPoint + new Vector3(-50, 200, 0);
        pointPoint = defaultPoint + new Vector3(-50, 50, 0); 
        holdPoint = defaultPoint + new Vector3(-200, 225, 0);
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

    public void Hit() 
    {
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

    /*
        Аз не помня, аз не съм видял
        минаха ли моите години?
        Ти не ме оставяй да загина,
        господи, преди да съм живял!

        Изведи ме вън от всяка сложност,
        научи ме пак на простота:
        да отдавам сетния петак
        от сърце на срещнатия просек.

        Да усещам своя радостта
        на невинното дете, което
        първите снежинки от небето
        сбира със отворена уста.

        И без свян да мога да говоря
        с простите на прост неук език...
        Научи ме, господи велик,
        да живея като всички хора.
     */
    public void SwapSprite(Sprite newSprite, bool txt)
    {
        isTextEnabled = txt;
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
        else
        {
            if (slamEnd)
            {
                Travel(slamPoint, pointPoint, 5000f, 1);
                slamEnd = false;
                switchTimer = 0.2f;
            }
            if (switchTimer > 0)
            {
                switchTimer -= Time.deltaTime;
                if (switchTimer < 0)
                {
                    CardText.SetActive(isTextEnabled);
                    SwapSprite(lastSprite, false);
                    Travel(pointPoint, defaultPoint, 150f, 1);
                }
            }
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
