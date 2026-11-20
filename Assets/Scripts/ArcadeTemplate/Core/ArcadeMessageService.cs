using UnityEngine;
using ArcadesTemplate.UI;

namespace ArcadesTemplate.Core
{
    /// <summary>
    /// Persistent service that shows short on-screen messages on top of every scene.
    /// Lives on the MessagePopup prefab placed in the StartScene.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ArcadeMessageService : MonoBehaviour
    {
        public static ArcadeMessageService Instance { get; private set; }

        [Header("UI (optional, auto-locates child if null)")]
        [SerializeField] private MessagePopupUI popupUI;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (popupUI == null)
                popupUI = GetComponentInChildren<MessagePopupUI>(true);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// Shows a short message using the popup UI (falls back to the Console).
        /// </summary>
        public void ShowMessage(string message)
        {
            if (popupUI != null)
                popupUI.ShowMessage(message);
            else
                Debug.Log($"[ArcadeMessageService] {message}");
        }
    }
}
