using UnityEngine;
using UnityEngine.UI;

public class SlamHand : MonoBehaviour
{
    Vector3 defaultPoint;
    Vector3 pointA;
    Vector3 pointB;
    float totalTravelTime;
    float timer;
    bool traveling;
    public Vector3 slamPoint;
    public Vector3 smokePoint;
    float curveComponent;
    bool slamEnd;
    void Awake()
    {
        defaultPoint = transform.localPosition;
        slamPoint = defaultPoint + new Vector3(0, 150, 0);
        smokePoint = defaultPoint + new Vector3(200, 200, 0);
    }
    public void StartSmoking()
    {
        Travel(defaultPoint, smokePoint, 400, 4);
    }

    public void StopSmoking()
    {
        Travel(smokePoint, defaultPoint, 400, 4);
    }

    public void Slam(int PlayerID) 
    {
        Dealer.SlamTheTable(PlayerID);
        Travel(defaultPoint, slamPoint, 450, 4);
        slamEnd = true;
    }

    public void SwapSprite(Sprite newSprite)
    {
        GetComponent<Image>().sprite = newSprite;
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
                slamEnd = false;
                Travel(slamPoint, defaultPoint, 5000f, 1);
            }
        }
    }

    public void Travel(Vector3 a, Vector3 b, float pixelsPerSecond, float cc)
    {
        Debug.Log(pixelsPerSecond);
        pointA = a;
        pointB = b;
        totalTravelTime = Vector3.Distance(pointA, pointB)/pixelsPerSecond;
        timer = totalTravelTime;
        curveComponent = cc;
        traveling = true;
    }
}
