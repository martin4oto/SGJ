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
    public Image warningL;
    public Image warningR;

    public Transform eyes;
    int lookDir;

    float frustration = 0f;
    public float score;
    float attention;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        fixEyes();
    }

    public float maxTimerToStop;

    // Update is called once per frame
    void Update()
    {   
        scoreText.text = score + "$";
        attention += 1/maxTimerToStop * Time.deltaTime;

        if (frustration > 0) 
        {
            Frustrate(-Time.deltaTime * frustrationDecay);
        }
        scoreText.color = Color.HSVToRGB(Time.time/5%1, 1, 1);

        if (attention >= 1)
        {
            attention = 0;
            ChangeId();
        }
        else
        {
            if (lookDir == 1)
            {
                warningR.fillAmount = attention;
            }
            else
            {

                warningL.fillAmount = attention;
            }
        }
    }

    void fixEyes() 
    {
        lookDir = Mathf.RoundToInt((currentPlayerId - 0.5f) * -2);

        eyes.transform.localScale = new Vector3(lookDir * Mathf.Abs(eyes.transform.localScale.x), eyes.transform.localScale.y, eyes.transform.localScale.z);
    
        if (lookDir == 1)
        {
            warningL.transform.parent.gameObject.SetActive(false);
            warningR.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            warningL.transform.parent.gameObject.SetActive(true);
            warningR.transform.parent.gameObject.SetActive(false);
        }
    
    }
    public static void SlamTheTable(int playerID) 
    {
        instance.Frustrate(frustrationBySlamming);
        if (playerID == 0 && instance.lookDir == 1){
            instance.attention -= 0.1f;
        }else if (playerID == 0 && instance.lookDir == -1){
            instance.attention += 0.2f;
        }else if (playerID == 1 && instance.lookDir == 1){
            instance.attention += 0.2f;
        }else if (playerID == 1 && instance.lookDir == -1){
            instance.attention -= 0.1f;
        }

        if (instance.attention < 0) instance.attention = 0;
    }

    public void ChangeId() 
    {
        currentPlayerId = currentPlayerId == 0 ? 1 : 0;
        Debug.Log(currentPlayerId);
        fixEyes();
        CheckForThrownCard();
        attention = 0;
    }

    public void ChangeId(int id) 
    {
        currentPlayerId = id;
        Debug.Log(currentPlayerId);
        fixEyes();
        CheckForThrownCard();
        attention = 0;
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
