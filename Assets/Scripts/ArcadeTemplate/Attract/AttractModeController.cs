using System;                 // <-- add
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using ArcadesTemplate.Input;
using System.IO;

namespace ArcadesTemplate.Attract
{
    public class AttractModeController : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private bool attractModeEnabled = true;
        [SerializeField] private float idleTimeToStart = 180f;

        [Header("Env Config")]
        [Tooltip("Environment variable controlling attract mode. 1/0, true/false, yes/no, on/off.")]
        [SerializeField] private string attractModeEnvVarName = "ATTRACT_MODE";

        [Tooltip("Default when env var is missing/invalid.")]
        [SerializeField] private bool defaultAttractModeEnabled = false;

        [Header("Video Source")]
        [Tooltip("Filename inside Assets/StreamingAssets (include extension). Example: demo_video.webm")]
        [SerializeField] private string streamingVideoFileName = "demo_video.webm";

        [Header("Video")]
        [SerializeField] private GameObject videoRoot;
        [SerializeField] private VideoPlayer videoPlayer;

        [Header("Input")]
        [SerializeField] private float moveDeadZone = 0.1f;

        [Header("Debug")]
        [SerializeField] private bool enableVideoLogging = true;

        // Optional debug visibility in the Inspector
        [SerializeField] private string resolvedAttractModeInspector;

        private float _lastInputTime;
        private bool _videoFound = false;
        private bool _isPlaying = false;

        private ulong _lastLoggedFrame = ulong.MaxValue;

        private void Awake()
        {
            _lastInputTime = Time.unscaledTime;

            // Resolve from env BEFORE we early-return / setup video
            ResolveAttractModeFromEnv();

            if (videoPlayer == null && videoRoot != null)
                videoPlayer = videoRoot.GetComponentInChildren<VideoPlayer>(true);

            if (videoRoot != null)
                videoRoot.SetActive(false);

            if (!attractModeEnabled)
                return;

            if (videoPlayer == null)
            {
                Debug.LogWarning("[AttractModeController] No VideoPlayer assigned. Attract mode disabled.");
                attractModeEnabled = false;
                return;
            }

            videoPlayer.playOnAwake = false;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.skipOnDrop = false;
            videoPlayer.sendFrameReadyEvents = true;

            if (enableVideoLogging)
                HookVideoPlayerLogging(videoPlayer);

            Debug.Log($"[AttractModeController] Platform={Application.platform} Unity={Application.unityVersion}");

            string fullPath = Path.Combine(Application.streamingAssetsPath, streamingVideoFileName);

#if UNITY_ANDROID && !UNITY_EDITOR
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = fullPath;
#else
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = fullPath;
#endif

            Debug.Log($"[AttractModeController] Video URL = {videoPlayer.url}");

#if !UNITY_ANDROID || UNITY_EDITOR
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[AttractModeController] Video not found at: {fullPath}. Attract mode disabled.");
                attractModeEnabled = false;
                return;
            }
#endif

            _videoFound = true;
        }

        private void ResolveAttractModeFromEnv()
        {
            // default is disabled (as requested)
            bool enabled = defaultAttractModeEnabled;

            string raw = Environment.GetEnvironmentVariable(attractModeEnvVarName);

            if (!string.IsNullOrEmpty(raw))
            {
                string v = raw.Trim().ToLowerInvariant();

                // Accept common forms: 1/0, true/false, yes/no, on/off
                if (v == "1" || v == "true" || v == "yes" || v == "y" || v == "on")
                    enabled = true;
                else if (v == "0" || v == "false" || v == "no" || v == "n" || v == "off")
                    enabled = false;
                else
                    Debug.LogWarning($"[AttractModeController] Invalid {attractModeEnvVarName}='{raw}'. Using default={defaultAttractModeEnabled}.");
            }

            attractModeEnabled = enabled;
            resolvedAttractModeInspector = $"{attractModeEnvVarName}={(raw ?? "<unset>")} -> enabled={attractModeEnabled}";
            Debug.Log($"[AttractModeController] Attract mode enabled={attractModeEnabled} (env {attractModeEnvVarName}={(raw ?? "<unset>")}, default={defaultAttractModeEnabled})");
        }

        private void OnDestroy()
        {
            if (enableVideoLogging && videoPlayer != null)
                UnhookVideoPlayerLogging(videoPlayer);
        }

        private void Update()
        {
            if (!attractModeEnabled || !_videoFound)
                return;

            bool anyInput = DetectAnyInput();

            if (anyInput)
            {
                _lastInputTime = Time.unscaledTime;
                if (_isPlaying) StopAttractVideo();
            }
            else
            {
                float idleTime = Time.unscaledTime - _lastInputTime;
                if (!_isPlaying && idleTime >= idleTimeToStart)
                    StartAttractVideo();
            }
        }

        private bool DetectAnyInput()
        {
            bool hasInput = false;

            if (ArcadeInput.Instance != null)
            {
                var actions = ArcadeInput.Instance.Actions;

                if (actions.Move.ReadValue<Vector2>().sqrMagnitude > (moveDeadZone * moveDeadZone))
                    hasInput = true;

                if (actions.ButtonA.WasPressedThisFrame() ||
                    actions.ButtonB.WasPressedThisFrame() ||
                    actions.ButtonC.WasPressedThisFrame() ||
                    actions.ButtonD.WasPressedThisFrame() ||
                    actions.ButtonE.WasPressedThisFrame() ||
                    actions.ButtonF.WasPressedThisFrame() ||
                    actions.ButtonStart.WasPressedThisFrame() ||
                    actions.ButtonExit.WasPressedThisFrame())
                {
                    hasInput = true;
                }
            }

            if (!hasInput)
            {
                if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                    hasInput = true;

                var g = Gamepad.current;
                if (g != null)
                {
                    if (g.buttonSouth.wasPressedThisFrame ||
                        g.buttonNorth.wasPressedThisFrame ||
                        g.buttonEast.wasPressedThisFrame ||
                        g.buttonWest.wasPressedThisFrame ||
                        g.startButton.wasPressedThisFrame ||
                        g.selectButton.wasPressedThisFrame)
                    {
                        hasInput = true;
                    }

                    if (g.leftStick.ReadValue().sqrMagnitude > (moveDeadZone * moveDeadZone) ||
                        g.rightStick.ReadValue().sqrMagnitude > (moveDeadZone * moveDeadZone))
                    {
                        hasInput = true;
                    }
                }
            }

            return hasInput;
        }

        private void StartAttractVideo()
        {
            if (_isPlaying || videoPlayer == null || !_videoFound)
                return;

            Debug.Log("[AttractModeController] StartAttractVideo()");

            if (videoRoot != null)
                videoRoot.SetActive(true);

            videoPlayer.isLooping = true;

            _lastLoggedFrame = ulong.MaxValue;
            videoPlayer.Stop();
            videoPlayer.Prepare();

            _isPlaying = true;
        }

        private void StopAttractVideo()
        {
            if (!_isPlaying || videoPlayer == null)
                return;

            Debug.Log("[AttractModeController] StopAttractVideo()");

            videoPlayer.Stop();
            if (videoRoot != null)
                videoRoot.SetActive(false);

            _isPlaying = false;
        }

        public void SetAttractModeEnabled(bool enabled)
        {
            attractModeEnabled = enabled;

            if (!enabled && _isPlaying)
                StopAttractVideo();
        }

        private void HookVideoPlayerLogging(VideoPlayer vp)
        {
            vp.errorReceived += OnVideoErrorReceived;
            vp.prepareCompleted += OnPrepareCompleted;
            vp.loopPointReached += OnLoopPointReached;
            vp.started += OnStarted;
            vp.frameReady += OnFrameReady;
        }

        private void UnhookVideoPlayerLogging(VideoPlayer vp)
        {
            vp.errorReceived -= OnVideoErrorReceived;
            vp.prepareCompleted -= OnPrepareCompleted;
            vp.loopPointReached -= OnLoopPointReached;
            vp.started -= OnStarted;
            vp.frameReady -= OnFrameReady;
        }

        private void OnVideoErrorReceived(VideoPlayer vp, string message)
        {
            Debug.LogError($"[VideoPlayer] ERROR: {message}");
        }

        private void OnPrepareCompleted(VideoPlayer vp)
        {
            vp.Play();
        }

        private void OnStarted(VideoPlayer vp)
        {
            Debug.Log($"[VideoPlayer] started. time={vp.time:0.000} frame={vp.frame} isPlaying={vp.isPlaying}");
        }

        private void OnLoopPointReached(VideoPlayer vp)
        {
            Debug.Log("[VideoPlayer] loopPointReached");
        }

        private void OnFrameReady(VideoPlayer vp, long frameIdx)
        {
            ulong u = (ulong)frameIdx;
            if (_lastLoggedFrame == ulong.MaxValue || u - _lastLoggedFrame >= 60)
            {
                _lastLoggedFrame = u;
            }
        }
    }
}
