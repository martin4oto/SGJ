using ArcadesTemplate.Core;
using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    public static EndScreen instance;
    float timeToLeave = 6f;
    public bool gameEnd;
    bool leaving;
    float moveTimer = 0.5f;
    Vector3 startPos;
    public TextMeshProUGUI reasonBox;
    public ParticleSystem chipDrain;
    public int finalScore;
    int displayScore;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        instance = this;
        startPos = transform.localPosition;
    }
    void Update()
    {
        if (!gameEnd) return;

        Dealer.instance.scoreText.text = displayScore + "$";
        displayScore -= Mathf.RoundToInt(Time.deltaTime * finalScore/5);


        if (moveTimer > 0)
        {
            moveTimer -= Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, 1 - moveTimer*2);
        }

        timeToLeave -= Time.deltaTime;
        if (timeToLeave <= 0 && !leaving)
        {
            leaving = true;
            ArcadeSceneManager.Instance.LoadLoseScreen(finalScore);
        }
    }

    public static void TriggerEndScreen(string reason)
    {
        if (instance.gameEnd) return;

        instance.finalScore = (int)Dealer.instance.score;
        instance.displayScore = instance.finalScore;
        instance.reasonBox.text = reason;
        instance.gameEnd = true;

        instance.chipDrain.Play();
    }
}
