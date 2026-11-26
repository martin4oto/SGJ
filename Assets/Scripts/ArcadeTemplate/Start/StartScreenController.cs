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
    public GameObject p1R;
    public GameObject p2R;
    bool p1;
    bool p2;
    bool foundP1Input;
    bool foundP2Input;
    bool starting;
    private float _timeSinceLoad;

    void Awake() {
        QualitySettings.vSyncCount = 1;
        //Application.targetFrameRate = 20; ne
    }
    void P1Ready(InputAction.CallbackContext context)
    {
        p1 = true;
        p1R.SetActive(true);
    }
    void P2Ready(InputAction.CallbackContext context)
    {
        p2 = true;
        p2R.SetActive(true);
    }
    void OnDisable()
    {
        InputManager.instance.players[0].Unsubscribe("ButtonA", P1Ready);
        InputManager.instance.players[0].Unsubscribe("ButtonB", P1Ready);
        InputManager.instance.players[0].Unsubscribe("ButtonC", P1Ready);
        InputManager.instance.players[1].Unsubscribe("ButtonA", P2Ready);
        InputManager.instance.players[1].Unsubscribe("ButtonB", P2Ready);
        InputManager.instance.players[1].Unsubscribe("ButtonC", P2Ready);
    }
    private void Update()
    {
        _timeSinceLoad += Time.unscaledDeltaTime;
        
        if (!foundP1Input && InputManager.instance.players.Count > 0)
        {
            Debug.Log("P1 bind");
            foundP1Input = true;
            InputManager.instance.players[0].Subscribe("ButtonA", P1Ready);
            InputManager.instance.players[0].Subscribe("ButtonB", P1Ready);
            InputManager.instance.players[0].Subscribe("ButtonC", P1Ready);
            
        }
        if (!foundP2Input && InputManager.instance.players.Count > 1){
            foundP2Input = true;
            InputManager.instance.players[1].Subscribe("ButtonA", P2Ready);
            InputManager.instance.players[1].Subscribe("ButtonB", P2Ready);
            InputManager.instance.players[1].Subscribe("ButtonC", P2Ready);
        }

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