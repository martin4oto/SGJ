using UnityEngine;
using UnityEngine.UI;
using ArcadesTemplate.Core;
using TMPro;

public class Dealer : MonoBehaviour
{
    public static Dealer instance;
    public Player[] players;
    public int currentPlayerId;
    public TextMeshProUGUI scoreText;
    public Image warningL;
    public Image warningR;
    public GameObject visionCone;
    public Transform eyes;
    int lookDir;
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
        if (!EndScreen.instance.gameEnd) scoreText.text = Mathf.RoundToInt(score) + "$";
        attention += 1/maxTimerToStop * Time.deltaTime;
        scoreText.color = Color.HSVToRGB(Time.time/5%1, 1, 1);

        score -= Time.deltaTime;
        if (score < 0) score = 0;

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
        visionCone.transform.localScale = new Vector3(lookDir * Mathf.Abs(visionCone.transform.localScale.x), visionCone.transform.localScale.y, visionCone.transform.localScale.z);
    
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

    public static void playerSuspiciousActivity(int playerId) 
    {
        if (playerId == instance.currentPlayerId)
        {
            Lose();
        }
    }

    static void Lose() 
    {
        int pId = instance.lookDir == -1 ? 2 : 1;
        EndScreen.TriggerEndScreen("The Dealer saw Player " + pId + " cheat...");
    }

    void CheckForThrownCard() 
    {
        if (!players[currentPlayerId].canCatch())
        {
            Lose();
        }
    }
}
