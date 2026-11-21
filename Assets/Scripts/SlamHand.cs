using UnityEngine;
using UnityEngine.UI;

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

    public void SwapSprite(Sprite newSprite)
    {
        GetComponent<Image>().sprite = newSprite;
    }
}
