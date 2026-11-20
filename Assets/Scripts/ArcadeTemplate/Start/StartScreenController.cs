using ArcadesTemplate.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using ArcadesTemplate.Input;
using ArcadesTemplate.Leaderboard;

public class StartScreenController : MonoBehaviour
{
    [Header("Optional")]
    [Tooltip("Minimum time before input is accepted (seconds).")]
    public float inputDelay = 0.5f;

    private float _timeSinceLoad;

    void Awake() {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 20;
    }

    
    private void Update()
    {
        _timeSinceLoad += Time.unscaledDeltaTime;
        if (_timeSinceLoad < inputDelay)
            return;

        if (StartPressed())
        {
            StartGame();
        }
        
        // DebugAllButtons(); // uncomment to debug all button presses
    }

    private bool StartPressed()
    {
        // 1) Preferred: via ArcadeInput actions
        if (ArcadeInput.Instance != null)
        {
            if (ArcadeInput.Instance.ButtonStartPressedThisFrame ||
                ArcadeInput.Instance.ButtonAPressedThisFrame)
            {
                return true;
            }
        }

        // 2) Fallback (just in case something goes wrong with Input System)
        bool keyboardPressed =
            (Keyboard.current != null &&
             (Keyboard.current.spaceKey.wasPressedThisFrame ||
              Keyboard.current.enterKey.wasPressedThisFrame));

        bool gamepadPressed =
            (Gamepad.current != null &&
             (Gamepad.current.buttonSouth.wasPressedThisFrame ||
              Gamepad.current.startButton.wasPressedThisFrame));

        return keyboardPressed || gamepadPressed;
    }

    private void StartGame()
    {
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
    private void DebugAllButtons()
    {
        if (ArcadeInput.Instance == null)
            return;

        if (ArcadeInput.Instance.ButtonAPressedThisFrame)
            Debug.Log("Button A Pressed");
        if (ArcadeInput.Instance.ButtonBPressedThisFrame)
            Debug.Log("Button B Pressed");
        if (ArcadeInput.Instance.ButtonCPressedThisFrame)
            Debug.Log("Button C Pressed");
        if (ArcadeInput.Instance.ButtonDPressedThisFrame)
            Debug.Log("Button D Pressed");
        if (ArcadeInput.Instance.ButtonEPressedThisFrame)
            Debug.Log("Button E Pressed");
        if (ArcadeInput.Instance.ButtonFPressedThisFrame)
            Debug.Log("Button F Pressed");
        if (ArcadeInput.Instance.ButtonStartPressedThisFrame)
            Debug.Log("Button Start Pressed");
    }
    
    
}