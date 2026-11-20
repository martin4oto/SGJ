using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

[InitializeOnLoad]
public class PlayFromBeginning : EditorWindow
{
    private const string LastScenePrefKey = "PlayFromBeginning_LastScenePath";
    private const string StartedByToolPrefKey = "PlayFromBeginning_StartedByTool";

    // Change this to your actual start scene path
    private const string StartScenePath = "Assets/Scenes/ArcadeTemplate/StartScene.unity";

    private const int ButtonsPerRow = 3;
    private const int ButtonWidth = 140;
    private const int ButtonHeight = 44;

    static PlayFromBeginning()
    {
        // Make sure we only subscribe once (safe even across domain reloads)
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("Window/PlayWindow")]
    public static void ShowWindow()
    {
        GetWindow<PlayFromBeginning>("PlayWindow");
    }

    private void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Play From StartScene", GUILayout.Height(30)))
            {
                PlayFromScene(StartScenePath);
            }

            if (GUILayout.Button("Return to Last Scene", GUILayout.Height(30)))
            {
                LoadLastScene();
            }
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(6);

        GUILayout.Label("All Scenes (Build Settings):", EditorStyles.boldLabel);

        string[] scenes = (from s in EditorBuildSettings.scenes where s.enabled select s.path).ToArray();
        if (scenes.Length == 0)
        {
            EditorGUILayout.HelpBox("No enabled scenes in Build Settings.", MessageType.Info);
            return;
        }

        int current = 0;
        while (current < scenes.Length)
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < ButtonsPerRow && current < scenes.Length; i++)
            {
                string scenePath = scenes[current];
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);

                if (GUILayout.Button(sceneName, GUILayout.Width(ButtonWidth), GUILayout.Height(ButtonHeight)))
                {
                    OpenSceneSafely(scenePath);
                }

                current++;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.HelpBox(
            "Play From StartScene will:\n" +
            "- Save/record your current scene\n" +
            "- Open StartScene\n" +
            "- Enter Play Mode\n" +
            "- When you STOP play (manually or otherwise), it auto-returns to your last scene.\n\n" +
            "Return to Last Scene button is manual fallback.",
            MessageType.None);
    }

    private static void PlayFromScene(string scenePath)
    {
        if (!File.Exists(scenePath))
        {
            Debug.LogError($"Start scene not found at path:\n{scenePath}");
            return;
        }

        // Ask to save modified scenes to avoid losing work
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        // Record current scene path before switching
        Scene currentScene = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(currentScene.path))
            EditorPrefs.SetString(LastScenePrefKey, currentScene.path);

        // Mark that THIS play session was started by our tool
        EditorPrefs.SetBool(StartedByToolPrefKey, true);

        // Open target scene, then play
        EditorSceneManager.OpenScene(scenePath);
        EditorApplication.isPlaying = true; // Unity 6000+ supported way
    }

    private static void OpenSceneSafely(string scenePath)
    {
        if (!File.Exists(scenePath))
        {
            Debug.LogError($"Scene not found:\n{scenePath}");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EditorSceneManager.OpenScene(scenePath);
    }

    private static void LoadLastScene()
    {
        string lastScenePath = EditorPrefs.GetString(LastScenePrefKey, "");

        if (string.IsNullOrEmpty(lastScenePath))
        {
            Debug.LogError("No last scene path recorded yet. Click 'Play From StartScene' at least once.");
            return;
        }

        if (!File.Exists(lastScenePath))
        {
            Debug.LogError($"Last scene no longer exists:\n{lastScenePath}");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EditorSceneManager.OpenScene(lastScenePath);
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // We want to react AFTER Unity has fully returned to edit mode.
        if (state != PlayModeStateChange.EnteredEditMode)
            return;

        // Only auto-return if play was started via our tool
        if (!EditorPrefs.GetBool(StartedByToolPrefKey, false))
            return;

        // Clear flag first so we don't loop / affect future normal play sessions
        EditorPrefs.SetBool(StartedByToolPrefKey, false);

        // Now return to last scene
        LoadLastScene();
    }
}
