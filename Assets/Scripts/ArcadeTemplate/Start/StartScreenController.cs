using ArcadesTemplate.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using ArcadesTemplate.Leaderboard;

public class StartScreenController : MonoBehaviour
{
    [Header("Optional")]
    [Tooltip("Minimum time before input is accepted (seconds).")]
    public float inputDelay = 0.5f;

    private float _timeSinceLoad;

    void Awake() {
        QualitySettings.vSyncCount = 1;
        //Application.targetFrameRate = 20; ne
    }

    void OnEnable()
    {
        InputManager.instance.players[0].Subscribe("ButtonA", StartGame);
    }
    void OnDisable()
    {
        InputManager.instance.players[0].Unsubscribe("ButtonA", StartGame);
    }
    private void Update()
    {
        _timeSinceLoad += Time.unscaledDeltaTime;
        
        // DebugAllButtons(); // uncomment to debug all button presses
    }

    private void StartGame(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (_timeSinceLoad < inputDelay) return;
        if (ArcadeSceneManager.Instance != null)
        {
            // Free to play: no credit check.
            ArcadeSceneManager.Instance.LoadMainGame();
        }
        else
        {
            Debug.LogWarning("[StartScreenController] ArcadeSceneManager missing - loading MainScene directly.");
            SceneManager.LoadScene("MainScene");
        }
    }
    
    // in update debug every button pressed from ArcadeInput
}