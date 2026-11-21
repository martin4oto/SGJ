using UnityEngine;

public class AnimationShake : MonoBehaviour
{
    public float duration;
    float timer;
    public float strenght;
    bool localSpace;
    bool animActive;
    Vector3 startSpot;
    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;

            if (timer <= 0)
            {
                if (localSpace) transform.localPosition = startSpot;
                else            transform.position = startSpot;
            }
            else
            {
                if (localSpace) transform.localPosition = new Vector3(Random.Range(-strenght, strenght), Random.Range(-strenght, strenght), transform.localPosition.z);
                else            transform.position = new Vector3(Random.Range(-strenght, strenght), Random.Range(-strenght, strenght), transform.position.z);
            }
        }
    }
    public void Play()
    {
        if (!animActive)
        {
            if (localSpace) startSpot = transform.localPosition;
            else            startSpot = transform.position;
        }
        timer = duration;
        animActive = true;
    }
}
