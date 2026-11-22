using UnityEngine;

public class AnimationShake : MonoBehaviour
{
    public float duration;
    float timer;
    public float strenght;
    [HideInInspector]
    public float delay;
    public bool localSpace;
    bool animActive;
    Vector3 startSpot;
    // Update is called once per frame
    void Update()
    {
        if (delay > 0)
        {
            delay -= Time.deltaTime;
            return;
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                animActive = false;
                if (localSpace) transform.localPosition = startSpot;
                else            transform.position = startSpot;
            }
            else
            {
                if (localSpace) transform.localPosition = (Vector2)startSpot + new Vector2(Random.Range(-strenght, strenght), Random.Range(-strenght, strenght));
                else            transform.position = (Vector2)startSpot + new Vector2(Random.Range(-strenght, strenght), Random.Range(-strenght, strenght));
            }
        }
    }
    public void Play(float d)
    {
        if (!animActive) delay = d;
        Play();
    }
    public void Play()
    {
        if (!animActive)
        {
            if (localSpace) startSpot = transform.localPosition;
            else            startSpot = transform.position;
        }
        timer += duration;
        if (timer > 3 * duration) timer = 3 * duration;
        animActive = true;
    }
}
