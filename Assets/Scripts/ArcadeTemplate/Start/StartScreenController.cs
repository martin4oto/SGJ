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
    bool p1;
    bool p2;
    bool starting;
    private float _timeSinceLoad;

    void Awake() {
        QualitySettings.vSyncCount = 1;
        //Application.targetFrameRate = 20; ne
    }
    void P1Ready(InputAction.CallbackContext context)
    {
        p1 = true;
    }
    void P2Ready(InputAction.CallbackContext context)
    {
        p2 = true;
    }
    void OnEnable()
    {
        InputManager.instance.players[0].Subscribe("ButtonA", P1Ready);
        InputManager.instance.players[1].Subscribe("ButtonA", P2Ready);
    }
    void OnDisable()
    {
        InputManager.instance.players[0].Unsubscribe("ButtonA", P1Ready);
        InputManager.instance.players[1].Unsubscribe("ButtonA", P2Ready);
    }
    private void Update()
    {
        _timeSinceLoad += Time.unscaledDeltaTime;
        
        if (p1 && p2 && !starting) StartGame();
        // DebugAllButtons(); // uncomment to debug all button presses
    }

    private void StartGame()
    {
        starting = true;

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