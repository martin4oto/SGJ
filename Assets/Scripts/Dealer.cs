using UnityEngine;
using UnityEngine.UI;
using ArcadesTemplate.Core;
using TMPro;

public class Dealer : MonoBehaviour
{
    public static int frustrationBySlamming = 10;
    public static Dealer instance;

    public Player[] players;
    public int currentPlayerId;
    public float frustrationDecay;

    public Slider frustrationMeter;
    public TextMeshProUGUI scoreText;

    public Transform eyes;

    float frustration = 0f;
    public float score;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
    }

    float timerToSwap = 0;
    public float maxTimerToStop;

    // Update is called once per frame
    void Update()
    {   
        scoreText.text = score + "$";
        timerToSwap += Time.deltaTime;
        if (timerToSwap> maxTimerToStop)
        {
            ChangeId();
        }

        if (frustration > 0) 
        {
            Frustrate(-Time.deltaTime * frustrationDecay);
        }
        scoreText.color = Color.HSVToRGB(Time.time/5%1, 1, 1);
    }

    void fixEyes() 
    {
        float eyeDirecton = (currentPlayerId - 0.5f) * -2;

        eyes.transform.localScale = new Vector3(eyeDirecton * Mathf.Abs(eyes.transform.localScale.x), eyes.transform.localScale.y, eyes.transform.localScale.z);
    }
    public static void SlamTheTable(int playerID) 
    {
        instance.ChangeId(playerID);
        instance.Frustrate(frustrationBySlamming);
    }

    void ChangeId() 
    {
        currentPlayerId = currentPlayerId == 0 ? 1 : 0;
        Debug.Log(currentPlayerId);
        fixEyes();
        CheckForThrownCard();
        timerToSwap = 0;
    }

    void ChangeId(int id) 
    {
        currentPlayerId = id;
        Debug.Log(currentPlayerId);
        fixEyes();
        CheckForThrownCard();
        timerToSwap = 0;
    }

    void Frustrate(float amount) 
    {
        frustration += amount;

        if (frustration < 0) frustration = 0;

        frustrationMeter.value = frustration;
    }

    public static void playerSuspiciousActivity(int playerId) 
    {
        if (playerId == instance.currentPlayerId)
        {
            Lose();
        }
    }

    static void Lose() 
    {
        //gotina animaciq na zaguba
        ArcadeSceneManager.Instance.LoadLoseScreen(Mathf.RoundToInt(instance.score));
    }

    void CheckForThrownCard() 
    {
        if (!players[currentPlayerId].canCatch())
        {
            Lose();
        }
    }
}
