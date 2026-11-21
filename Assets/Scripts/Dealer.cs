using UnityEngine;
using UnityEngine.UI;

public class Dealer : MonoBehaviour
{
    public static int frustrationBySlamming = 10;
    public static Dealer instance;

    public Player[] players;
    public int currentPlayerId;
    public float frustrationDecay;

    public Slider frustrationMeter;

    float frustration = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (frustration > 0) 
        {
            Frustrate(-Time.deltaTime * frustrationDecay);
        }
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
        CheckForThrownCard();
    }

    void ChangeId(int id) 
    {
        currentPlayerId = id;
        Debug.Log(currentPlayerId);
        CheckForThrownCard();
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
    }

    void CheckForThrownCard() 
    {
        if (!players[currentPlayerId].canCatch())
        {
            Lose();
        }
    }
}
