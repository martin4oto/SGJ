using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace DHOArcade.GameJamSetup
{
    /// <summary>
    /// Editor-only setup for the DHO Arcade Game Jam template.
    ///
    /// A .unitypackage cannot carry project settings, so after importing the template this
    /// script (in its own assembly with no dependencies, so it compiles even while packages
    /// are missing):
    ///   1. installs the required Unity packages (Localization, Input System, URP, uGUI),
    ///   2. registers the template's Localization + Addressables settings,
    ///   3. adds the template scenes to Build Settings.
    ///
    /// It runs automatically once per project and can be re-run from
    /// Tools > DHO Arcade > Setup Game Jam Template.
    /// Nothing here is included in player builds.
    /// </summary>
    [InitializeOnLoad]
    internal static class GameJamTemplateSetup
    {
        // Package dependencies of the template (tested with Unity 6000.3).
        private static readonly KeyValuePair<string, string>[] RequiredPackages =
        {
            new KeyValuePair<string, string>("com.unity.inputsystem", "1.18.0"),
            new KeyValuePair<string, string>("com.unity.localization", "1.5.13"),
            new KeyValuePair<string, string>("com.unity.render-pipelines.universal", "17.3.0"),
            new KeyValuePair<string, string>("com.unity.ugui", "2.0.0"),
        };

        private static readonly string[] TemplateScenes =
        {
            "Assets/Scenes/ArcadeTemplate/LoadingScene.unity",
            "Assets/Scenes/ArcadeTemplate/StartScene.unity",
            "Assets/Scenes/ArcadeTemplate/MainScene.unity",
            "Assets/Scenes/ArcadeTemplate/WinScene.unity",
            "Assets/Scenes/ArcadeTemplate/LoseScene.unity",
            "Assets/Scenes/ArcadeTemplate/NameEntryScene.unity",
        };

        private const string LocalizationSettingsPath = "Assets/Localization/Localization Settings.asset";
        private const string AddressablesDefaultObjectPath = "Assets/AddressableAssetsData/DefaultObject.asset";
        private const string LocalizationConfigKey = "com.unity.localization.settings";
        private const string AddressablesConfigKey = "com.unity.addressableassets";

        private const string DoneKeyPrefix = "DHOArcade.GameJamSetup.Done.";

        private static ListRequest _listRequest;
        private static AddAndRemoveRequest _addRequest;
        private static bool _interactive;

        static GameJamTemplateSetup()
        {
            if (Application.isBatchMode)
                return;

            if (EditorPrefs.GetBool(DoneKeyPrefix + ProjectKey, false))
                return;

            EditorApplication.delayCall += () => Run(false);
        }

        private static string ProjectKey => Application.dataPath.GetHashCode().ToString("X");

        [MenuItem("Tools/DHO Arcade/Setup Game Jam Template")]
        private static void RunFromMenu() => Run(true);

        private static void Run(bool interactive)
        {
            _interactive = interactive;
            _listRequest = Client.List(true, true);
            EditorApplication.update += WaitForList;
        }

        private static void WaitForList()
        {
            if (!_listRequest.IsCompleted)
                return;
            EditorApplication.update -= WaitForList;

            if (_listRequest.Status != StatusCode.Success)
            {
                Debug.LogError("[GameJamSetup] Could not list packages: " + _listRequest.Error?.message);
                return;
            }

            var installed = new HashSet<string>(_listRequest.Result.Select(p => p.name));
            var missing = RequiredPackages.Where(p => !installed.Contains(p.Key))
                                          .Select(p => p.Key + "@" + p.Value)
                                          .ToArray();

            if (missing.Length == 0)
            {
                ConfigureProject();
                return;
            }

            Debug.Log("[GameJamSetup] Installing required packages: " + string.Join(", ", missing));
            _addRequest = Client.AddAndRemove(missing, null);
            EditorApplication.update += WaitForAdd;
        }

        private static void WaitForAdd()
        {
            if (!_addRequest.IsCompleted)
                return;
            EditorApplication.update -= WaitForAdd;

            if (_addRequest.Status != StatusCode.Success)
            {
                Debug.LogError("[GameJamSetup] Package install failed: " + _addRequest.Error?.message +
                               "\nInstall them manually via Window > Package Manager, then run Tools > DHO Arcade > Setup Game Jam Template.");
                return;
            }

            // Scripts now recompile; after the domain reload this class runs again and,
            // with all packages present, configures the project.
            Debug.Log("[GameJamSetup] Packages installed. Setup will finish after scripts recompile.");
        }

        private static void ConfigureProject()
        {
            var log = new List<string>();

            RegisterConfigObject(LocalizationConfigKey, LocalizationSettingsPath, log);
            RegisterConfigObject(AddressablesConfigKey, AddressablesDefaultObjectPath, log);
            AddScenesToBuildSettings(log);
            EnsureTmpEssentialResources(log);

            AssetDatabase.SaveAssets();

            // Retry on the next domain reload if something was not ready yet (e.g. assets still importing).
            bool complete = !log.Any(l => l.StartsWith("WARNING"));
            EditorPrefs.SetBool(DoneKeyPrefix + ProjectKey, complete);

            string summary = log.Count == 0 ? "Project already configured." : string.Join("\n", log);
            Debug.Log("[GameJamSetup] " + summary +
                      "\nOpen Assets/Scenes/ArcadeTemplate/StartScene.unity (or Window > PlayWindow) and press Play.");

            if (_interactive)
                EditorUtility.DisplayDialog("DHO Arcade Game Jam Template", summary, "OK");
        }

        private const string TmpEssentialsPackage = "Packages/com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage";

        private static void EnsureTmpEssentialResources(List<string> log)
        {
            // TextMeshPro needs its settings asset (normally imported via Window > TextMeshPro > Import TMP Essential Resources).
            if (AssetDatabase.FindAssets("TMP Settings t:ScriptableObject").Length > 0 ||
                AssetDatabase.LoadMainAssetAtPath("Assets/TextMesh Pro/Resources/TMP Settings.asset") != null)
                return;

            if (!System.IO.File.Exists(System.IO.Path.GetFullPath(TmpEssentialsPackage)))
            {
                log.Add("WARNING: TMP Essential Resources not found - use Window > TextMeshPro > Import TMP Essential Resources.");
                return;
            }

            AssetDatabase.ImportPackage(TmpEssentialsPackage, false);
            log.Add("Imported TMP Essential Resources.");
        }

        private static void RegisterConfigObject(string key, string assetPath, List<string> log)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (asset == null)
            {
                log.Add($"WARNING: {assetPath} not found (config '{key}' not set).");
                return;
            }

            if (EditorBuildSettings.TryGetConfigObject(key, out Object current) && current == asset)
                return;

            EditorBuildSettings.AddConfigObject(key, asset, true);
            log.Add($"Registered {assetPath} as '{key}'.");
        }

        private static void AddScenesToBuildSettings(List<string> log)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            bool changed = false;

            // Keep the template flow first, in order, and preserve any other scenes after it.
            var ordered = new List<EditorBuildSettingsScene>();
            foreach (var path in TemplateScenes)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                {
                    log.Add("WARNING: missing scene " + path);
                    continue;
                }

                var existing = scenes.FirstOrDefault(s => s.path == path);
                if (existing == null || !existing.enabled)
                    changed = true;
                ordered.Add(new EditorBuildSettingsScene(path, true));
            }

            foreach (var s in scenes)
                if (!TemplateScenes.Contains(s.path))
                    ordered.Add(s);

            for (int i = 0; i < TemplateScenes.Length && i < scenes.Count; i++)
                if (scenes[i].path != TemplateScenes[i])
                    changed = true;

            if (!changed)
                return;

            EditorBuildSettings.scenes = ordered.ToArray();
            log.Add("Build Settings scene list updated (LoadingScene first).");
        }
    }
}
