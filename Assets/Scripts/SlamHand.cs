using UnityEngine;

public class SlamHand : MonoBehaviour
{
    public void StartSmoking() 
    {
    }

    public void StopSmoking() 
    {
    }

    public void Slam(int PlayerID) 
    {
        Dealer.SlamTheTable(PlayerID);
    }
}
