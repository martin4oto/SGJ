using UnityEngine;
using TMPro;

namespace ArcadesTemplate.UI
{
    /// <summary>
    /// Small fading on-screen message (used for hints such as "press Exit again to quit").
    /// </summary>
    public class MessagePopupUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text messageText;

        [Header("Timing")]
        [SerializeField] private float showDuration = 2f;
        [SerializeField] private float fadeOutDuration = 0.4f;

        private float _timer = 0f;
        private bool _isShowing = false;

        private void Reset()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            if (messageText == null)
                messageText = GetComponentInChildren<TMP_Text>();
        }

        private void Awake()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        private void Update()
        {
            if (!_isShowing || canvasGroup == null)
                return;

            _timer += Time.unscaledDeltaTime;

            if (_timer > showDuration)
            {
                float t = (_timer - showDuration) / Mathf.Max(0.01f, fadeOutDuration);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

                if (t >= 1f)
                {
                    _isShowing = false;
                }
            }
        }

        public void ShowMessage(string text)
        {
            if (messageText != null)
                messageText.text = text;

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

            _timer = 0f;
            _isShowing = true;
        }
    }
}
