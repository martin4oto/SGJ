using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization;
using UnityEngine.InputSystem;


namespace ArcadesTemplate.Core
{
    public enum ResultScreenType
    {
        None,
        Win,
        Lose
    }

    /// <summary>
    /// Persistent game-flow controller: Start -> Main -> Win/Lose (-> NameEntry) -> replay.
    /// Game Jam template: playing and replaying is always free (no credits).
    /// </summary>
    [DefaultExecutionOrder(-45)]
    public class ArcadeSceneManager : MonoBehaviour
    {
        public static ArcadeSceneManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string startSceneName = "StartScene";
        [SerializeField] private string mainSceneName = "MainScene";
        [SerializeField] private string winSceneName = "WinScene";
        [SerializeField] private string loseSceneName = "LoseScene";
        [SerializeField] private string nameEntrySceneName = "NameEntryScene";
        bool connectedInput;

        [Header("Exit")]
        [SerializeField] private float exitDoublePressWindow = 3f;
        public LocalizedString exitHintMessage;

        // Last game result
        public int LastScore { get; private set; }

        public ResultScreenType LastResultScreen { get; private set; } = ResultScreenType.None;

        public bool HasHandledNameEntryForLastScore { get; set; } = false;

        public bool HasSavedHighScoreForLastScore { get; set; } = false;

        public string StartSceneName => startSceneName;
        public string MainSceneName => mainSceneName;

        private float _lastExitPressTime = -999f;
        private bool _exitArmed = false;
        bool isLoading;
        bool numpad;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void LoadStartScreen()
        {
            LoadSceneSafe(startSceneName);
        }

        /// <summary>Starts (or restarts) the game. Always free.</summary>
        public void LoadMainGame()
        {
            LoadSceneSafe(mainSceneName);
        }

        /// <summary>Replays the game from a result screen. Always free, can be repeated indefinitely.</summary>
        public void Replay()
        {
            LoadMainGame();
        }

        public void LoadWinScreen(int score)
        {
            SetResult(score, ResultScreenType.Win);
            LoadSceneSafe(winSceneName);
        }

        public void LoadLoseScreen(int score)
        {
            SetResult(score, ResultScreenType.Lose);
            LoadSceneSafe(loseSceneName);
        }

        public void LoadNameEntryScreen()
        {
            LoadSceneSafe(nameEntrySceneName);
        }

        public void ReturnToLastResultScreen()
        {
            switch (LastResultScreen)
            {
                case ResultScreenType.Win:
                    LoadSceneSafe(winSceneName);
                    break;
                case ResultScreenType.Lose:
                    LoadSceneSafe(loseSceneName);
                    break;
                default:
                    LoadStartScreen();
                    break;
            }
        }

        private void SetResult(int score, ResultScreenType type)
        {
            LastScore = score;
            LastResultScreen = type;

            HasSavedHighScoreForLastScore = false;
            HasHandledNameEntryForLastScore = false;
        }

        private void LoadSceneSafe(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("[ArcadeSceneManager] Scene name is empty.");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"[ArcadeSceneManager] Scene '{sceneName}' is not in Build Settings (File > Build Profiles > Scene List).");
                return;
            }

            if (!isLoading){
                isLoading = true;
                SceneManager.LoadScene(sceneName);
            }
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode){
            isLoading = false;   
        }

        private void HandleExitInput(InputAction.CallbackContext context)
        {
            float now = Time.unscaledTime;

            if (!_exitArmed || (now - _lastExitPressTime) > exitDoublePressWindow)
            {
                _exitArmed = true;
                _lastExitPressTime = now;

                string hint = L(exitHintMessage);

                if (ArcadeMessageService.Instance != null)
                    ArcadeMessageService.Instance.ShowMessage(hint);
                else
                    Debug.Log("[ArcadeSceneManager] " + hint);
            }
            else
            {
                ExitGame();
            }
        }

        private string L(LocalizedString ls)
        {
            if (ls == null || ls.IsEmpty)
                return "PRESS EXIT AGAIN TO QUIT";

            ls.Arguments = null;
            return ls.GetLocalizedString();
        }

        public void ExitGame()
        {
            Application.Quit();
        }
        void Update()
        {
            if (!connectedInput && InputManager.instance.players.Count > 0){
                InputManager.instance.players[0].Subscribe("Exit", HandleExitInput);
                connectedInput = true;
            }
        }

        void OnEnable()
        {
            
            SceneManager.sceneLoaded += SceneLoaded;
        }
        void OnDisable()
        {
            InputManager.instance.players[0].Unsubscribe("Exit", HandleExitInput);
            SceneManager.sceneLoaded -= SceneLoaded;
        }
    }
}
