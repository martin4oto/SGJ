using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SmartLoadingScreen : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string sceneToLoad = "StartScene";

    [Header("Show Only If Slow")]
    [Tooltip("If loading finishes before this time, the loading UI is never shown.")]
    [SerializeField] private float showDelay = 0.5f;

    [Header("Loading UI Root (parent object to hide/show)")]
    [SerializeField] private GameObject loadingRoot;

    [Header("Spinner (PNG frames)")]
    [SerializeField] private Image spinnerImage;
    [SerializeField] private Sprite[] spinnerFrames;
    [SerializeField] private float spinnerFPS = 50f;

    [Header("Loading Text")]
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private float dotsFPS = 3f;

    private int _spinnerIndex = 0;
    private float _spinnerTimer = 0f;

    private int _dots = 1;
    private float _dotsTimer = 0f;

    private void Start()
    {
        if (loadingRoot != null)
            loadingRoot.SetActive(false); // hidden by default

        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        // Let this scene render at least one frame (avoid black frame issues)
        yield return null;

        // Begin loading immediately (fast machines benefit)
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false;

        float t = 0f;
        bool shown = false;

        // Wait until either:
        // - load finishes quickly (progress reaches 0.9), OR
        // - showDelay passes (then we show the loading UI)
        while (op.progress < 0.9f && t < showDelay)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // If still not loaded after delay, show and animate
        if (op.progress < 0.9f)
        {
            shown = true;
            if (loadingRoot != null)
                loadingRoot.SetActive(true);

            // Animate until loaded
            while (op.progress < 0.9f)
            {
                Animate(Time.unscaledDeltaTime);
                yield return null;
            }
        }

        // If we showed the loading UI, you can optionally force it to be visible
        // for a minimum time to avoid "show then instantly disappear".
        // For now, you asked only for show-after-delay behavior.

        op.allowSceneActivation = true;
    }

    private void Animate(float dt)
    {
        // Spinner frames
        if (spinnerImage != null && spinnerFrames != null && spinnerFrames.Length > 0)
        {
            _spinnerTimer += dt;
            float frameTime = 1f / Mathf.Max(1f, spinnerFPS);

            while (_spinnerTimer >= frameTime) // while handles very high FPS cleanly
            {
                _spinnerTimer -= frameTime;
                spinnerImage.sprite = spinnerFrames[_spinnerIndex];
                _spinnerIndex = (_spinnerIndex + 1) % spinnerFrames.Length;
            }
        }

        // Loading dots
        if (loadingText != null)
        {
            _dotsTimer += dt;
            float dotTime = 1f / Mathf.Max(1f, dotsFPS);

            while (_dotsTimer >= dotTime)
            {
                _dotsTimer -= dotTime;
                _dots = (_dots % 3) + 1;
                loadingText.text = "Loading" + new string('.', _dots);
            }
        }
    }
}
