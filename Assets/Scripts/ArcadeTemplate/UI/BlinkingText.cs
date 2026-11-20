using UnityEngine;
using TMPro;

public class BlinkingText : MonoBehaviour
{
    [SerializeField] private float blinkSpeed = 1f; // times per second
    [SerializeField] private bool enabled = true;
    private TMP_Text _text;
    private float _timer;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (!enabled || _text == null)
            return;
        
        _timer += Time.unscaledDeltaTime * blinkSpeed;
        float alpha = (Mathf.Sin(_timer * Mathf.PI) + 1f) * 0.5f; // 0..1
        if (_text != null)
        {
            var color = _text.color;
            color.a = alpha;
            _text.color = color;
        }
    }
}
