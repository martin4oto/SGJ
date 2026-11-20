using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using ArcadesTemplate.Input;
using ArcadesTemplate.Core;

public class JoystickTester : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI joystickText;
    [SerializeField] private AudioSource soundTestSource;
    
    [Header("Score")]
    [SerializeField] private int score = 0;

    [Header("Hold-to-end settings (seconds)")]
    [SerializeField] private float holdToWinSeconds = 3f;
    [SerializeField] private float holdToLoseSeconds = 3f;
    [SerializeField] private float holdToToggleDetailsSeconds = 1.0f; // tweak as you like

    [SerializeField] private float holdToSoundTestSeconds = 3.0f;




    [Header("UI")]
    [Tooltip("Show the underlying technical control names (e.g. North/Y, button2). Keep OFF for non-technical users.")]
    [SerializeField] private bool showTechnicalNames = false;

    [Tooltip("If a trigger is considered pressed above this value.")]
    [SerializeField] private float triggerPressedThreshold = 0.5f;

    [System.Serializable]
    private class ArcadeMapEntry
    {
        [Header("Shown on screen")]
        public string arcadeLabel;     // e.g. "A", "Play", "ChangeGame"
        public string helpText;        // e.g. "(Top)", "Start", etc. optional

        [Header("Gamepad mapping")]
        public GamepadPart gamepadPart;

        [Header("Generic USB encoder mapping")]
        public int joystickButtonNumber; // e.g. 2 => "button2". Use -1 if not applicable.
    }

    private enum GamepadPart
    {
        None,
        SouthA,
        EastB,
        WestX,
        NorthY,
        LB,
        RB,
        Start,
        Select,
        LT,
        RT,
        LS,
        RS,
        DpadUp,
        DpadRight,
        DpadDown,
        DpadLeft,
    }

    [Header("Arcade mapping")]
    [SerializeField] private ArcadeMapEntry[] arcadeMap = new ArcadeMapEntry[]
    {
        new ArcadeMapEntry { arcadeLabel="A",          helpText="",    gamepadPart=GamepadPart.NorthY, joystickButtonNumber=-1 },
        new ArcadeMapEntry { arcadeLabel="B",          helpText="",  gamepadPart=GamepadPart.EastB,  joystickButtonNumber=2  },
        new ArcadeMapEntry { arcadeLabel="C",          helpText="", gamepadPart=GamepadPart.SouthA, joystickButtonNumber=3  },
        new ArcadeMapEntry { arcadeLabel="D",          helpText="",   gamepadPart=GamepadPart.WestX,  joystickButtonNumber=4  },
        new ArcadeMapEntry { arcadeLabel="E",          helpText="",       gamepadPart=GamepadPart.LB,     joystickButtonNumber=5  },
        new ArcadeMapEntry { arcadeLabel="F",          helpText="",       gamepadPart=GamepadPart.RB,     joystickButtonNumber=6  },

        new ArcadeMapEntry { arcadeLabel="Play",       helpText="(Left)",    gamepadPart=GamepadPart.Start,  joystickButtonNumber=10 },
        new ArcadeMapEntry { arcadeLabel="ChangeGame", helpText="(Right)",   gamepadPart=GamepadPart.Select, joystickButtonNumber=9  },
    };

    private float _bHeldTime = 0f;
    private float _cHeldTime = 0f;
    private float _dHeldTime = 0f;
    private float _eHeldTime = 0f;
    
    private bool _detailsToggledThisHold = false;
    
    private bool _soundToggledThisHold = false;
    private bool _soundTestActive = false;

    private string _lastPressed = "-";

    private void Update()
    {
        var sb = new StringBuilder(512);

        var g = Gamepad.current;
        var j = Joystick.current;

        HandleHoldActions();

        sb.AppendLine("ARCADE CONTROL TEST");
        sb.AppendLine("-------------------");

        if (g != null)
        {
            sb.AppendLine($"Controller: {g.displayName}");
            AppendMovementSummary(sb, g);
            AppendArcadePanel(sb, g, j); // CHANGED: pass joystick too for combined technical details
            AddScoreEdges_Gamepad(g, j); // CHANGED: pass joystick too for combined technical details
        }
        else if (j != null)
        {
            sb.AppendLine($"Controller: {j.displayName}");
            AppendMovementSummary(sb, j);
            AppendArcadePanel(sb, null, j);
            AddScoreEdges_Joystick(j);   // CHANGED: fixed trigger edge mapping
        }
        else
        {
            sb.AppendLine("Controller: (none) - using KEYBOARD");
            AppendKeyboardPanel(sb);
        }

        // Score from the arcade actions so keyboard, gamepad and USB encoder all count the same way.
        AddScoreEdges_ArcadeInput();

        sb.AppendLine();
        sb.AppendLine($"Last pressed: {_lastPressed}");
        sb.AppendLine($"Score: {score}");
        AppendHoldHints(sb);

        if (joystickText != null)
            joystickText.text = sb.ToString();
        
        if (_soundTestActive && soundTestSource != null && !soundTestSource.isPlaying)
        {
            _soundTestActive = false;
        }
    }

    // --------------------------
    // Movement info
    // --------------------------
    private void AppendMovementSummary(StringBuilder sb, Gamepad g)
    {
        var ls = g.leftStick.ReadValue();
        var d = g.dpad;

        sb.AppendLine();
        sb.AppendLine("Movement:");
        sb.Append("  Stick: ");
        sb.Append($"X={ls.x:0.00} Y={ls.y:0.00}");
        sb.AppendLine();

        sb.Append("  DPad:  ");
        sb.Append(d.up.isPressed ? "U " : "- ");
        sb.Append(d.right.isPressed ? "R " : "- ");
        sb.Append(d.down.isPressed ? "D " : "- ");
        sb.Append(d.left.isPressed ? "L" : "-");
        sb.AppendLine();
    }

    private void AppendMovementSummary(StringBuilder sb, Joystick j)
    {
        var stick = j.TryGetChildControl<StickControl>("stick");
        var hat = j.TryGetChildControl<DpadControl>("hat");

        sb.AppendLine();
        sb.AppendLine("Movement:");

        if (stick != null)
            sb.AppendLine($"  Stick: X={stick.x.ReadValue():0.00} Y={stick.y.ReadValue():0.00}");
        else
            sb.AppendLine("  Stick: (not found)");

        if (hat != null)
        {
            sb.Append("  Hat:   ");
            sb.Append(hat.up.isPressed ? "U " : "- ");
            sb.Append(hat.right.isPressed ? "R " : "- ");
            sb.Append(hat.down.isPressed ? "D " : "- ");
            sb.Append(hat.left.isPressed ? "L" : "-");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("  Hat:   (not found)");
        }
    }

    // --------------------------
    // Arcade panel
    // --------------------------
    private void AppendArcadePanel(StringBuilder sb, Gamepad g, Joystick j)
    {
        sb.AppendLine();
        sb.AppendLine("Press buttons on the cabinet:");
        sb.AppendLine();

        sb.Append("Pressed now: ");
        bool any = false;

        for (int i = 0; i < arcadeMap.Length; i++)
        {
            var entry = arcadeMap[i];
            bool pressed = IsArcadePressed(g, j, entry);
            if (pressed)
            {
                any = true;
                sb.Append('[').Append(entry.arcadeLabel).Append("] ");
            }
        }

        if (!any) sb.Append("(none)");
        sb.AppendLine();
        sb.AppendLine();

        for (int i = 0; i < arcadeMap.Length; i++)
        {
            var entry = arcadeMap[i];
            bool pressed = IsArcadePressed(g, j, entry);

            sb.Append(pressed ? "[ON ] " : "[OFF] ");
            sb.Append(entry.arcadeLabel);

            if (!string.IsNullOrWhiteSpace(entry.helpText))
                sb.Append(' ').Append(entry.helpText);

            // CHANGED: show BOTH variations when enabled
            if (showTechnicalNames)
            {
                string tech = BuildCombinedTechnicalDetails(entry, g, j);
                if (!string.IsNullOrEmpty(tech))
                    sb.Append("  (").Append(tech).Append(')');
            }

            sb.AppendLine();
        }
    }

    private bool IsArcadePressed(Gamepad g, Joystick j, ArcadeMapEntry entry)
    {
        if (g != null) return IsArcadePressedOnGamepad(g, entry);
        if (j != null) return IsArcadePressedOnJoystick(j, entry);
        return false;
    }

    private bool IsArcadePressedOnGamepad(Gamepad g, ArcadeMapEntry entry)
    {
        switch (entry.gamepadPart)
        {
            case GamepadPart.SouthA:  return g.buttonSouth.isPressed;
            case GamepadPart.EastB:   return g.buttonEast.isPressed;
            case GamepadPart.WestX:   return g.buttonWest.isPressed;
            case GamepadPart.NorthY:  return g.buttonNorth.isPressed;

            case GamepadPart.LB:      return g.leftShoulder.isPressed;
            case GamepadPart.RB:      return g.rightShoulder.isPressed;

            case GamepadPart.Start:   return g.startButton.isPressed;
            case GamepadPart.Select:  return g.selectButton.isPressed;

            case GamepadPart.LT:      return g.leftTrigger.ReadValue() > triggerPressedThreshold;
            case GamepadPart.RT:      return g.rightTrigger.ReadValue() > triggerPressedThreshold;

            case GamepadPart.LS:      return g.leftStickButton.isPressed;
            case GamepadPart.RS:      return g.rightStickButton.isPressed;

            case GamepadPart.DpadUp:    return g.dpad.up.isPressed;
            case GamepadPart.DpadRight: return g.dpad.right.isPressed;
            case GamepadPart.DpadDown:  return g.dpad.down.isPressed;
            case GamepadPart.DpadLeft:  return g.dpad.left.isPressed;

            default:
                return false;
        }
    }

    private bool IsArcadePressedOnJoystick(Joystick j, ArcadeMapEntry entry)
    {
        if (entry.joystickButtonNumber >= 0)
        {
            string name = $"button{entry.joystickButtonNumber}";
            var b = j.TryGetChildControl<ButtonControl>(name);
            return b != null && b.isPressed;
        }

        if (entry.joystickButtonNumber == -1)
        {
            var trigger =
                j.TryGetChildControl<AxisControl>("trigger") ??
                j.TryGetChildControl<AxisControl>("z") ??
                j.TryGetChildControl<AxisControl>("rz");

            return trigger != null && trigger.ReadValue() > triggerPressedThreshold;
        }

        return false;
    }

    // --------------------------
    // CHANGED: combined technical details (both gamepad + joystick)
    // --------------------------
    private string BuildCombinedTechnicalDetails(ArcadeMapEntry entry, Gamepad g, Joystick j)
    {
        string gp = GetGamepadTechnicalName(entry.gamepadPart);
        string js = GetJoystickTechnicalName(entry, j);

        // If one side is missing, still show the other.
        if (string.IsNullOrEmpty(gp) && string.IsNullOrEmpty(js)) return "";
        if (string.IsNullOrEmpty(gp)) return $"Joystick: {js}";
        if (string.IsNullOrEmpty(js)) return $"Gamepad: {gp}";
        return $"Gamepad: {gp} | Joystick: {js}";
    }

    private string GetGamepadTechnicalName(GamepadPart part)
    {
        switch (part)
        {
            case GamepadPart.SouthA: return "South/A";
            case GamepadPart.EastB: return "East/B";
            case GamepadPart.WestX: return "West/X";
            case GamepadPart.NorthY: return "North/Y";
            case GamepadPart.LB: return "LB";
            case GamepadPart.RB: return "RB";
            case GamepadPart.Start: return "Start";
            case GamepadPart.Select: return "Select";
            case GamepadPart.LT: return "LT";
            case GamepadPart.RT: return "RT";
            case GamepadPart.LS: return "LS";
            case GamepadPart.RS: return "RS";
            case GamepadPart.DpadUp: return "DPad Up";
            case GamepadPart.DpadRight: return "DPad Right";
            case GamepadPart.DpadDown: return "DPad Down";
            case GamepadPart.DpadLeft: return "DPad Left";
            default: return "";
        }
    }

    private string GetJoystickTechnicalName(ArcadeMapEntry entry, Joystick j)
    {
        if (entry.joystickButtonNumber >= 0)
            return $"button{entry.joystickButtonNumber}";

        if (entry.joystickButtonNumber == -1)
        {
            // If a joystick is present, show the actual matched axis name. Otherwise show the possible aliases.
            if (j != null)
            {
                var trigger =
                    j.TryGetChildControl<AxisControl>("trigger") ??
                    j.TryGetChildControl<AxisControl>("z") ??
                    j.TryGetChildControl<AxisControl>("rz");

                if (trigger != null) return trigger.name;
            }
            return "trigger/z/rz";
        }

        return "";
    }

    // --------------------------
    // Score + last-pressed (edge detection)
    // --------------------------
    // CHANGED: pass joystick so last-pressed can include combined detail if you want it
    private void AddScoreEdges_Gamepad(Gamepad g, Joystick j)
    {
        for (int i = 0; i < arcadeMap.Length; i++)
        {
            var entry = arcadeMap[i];

            if (WasArcadePressedThisFrameOnGamepad(g, entry))
            {

                if (showTechnicalNames)
                    _lastPressed = $"{entry.arcadeLabel} ({BuildCombinedTechnicalDetails(entry, g, j)})";
                else
                    _lastPressed = entry.arcadeLabel;
            }
        }
    }

    private bool WasArcadePressedThisFrameOnGamepad(Gamepad g, ArcadeMapEntry entry)
    {
        switch (entry.gamepadPart)
        {
            case GamepadPart.SouthA:  return g.buttonSouth.wasPressedThisFrame;
            case GamepadPart.EastB:   return g.buttonEast.wasPressedThisFrame;
            case GamepadPart.WestX:   return g.buttonWest.wasPressedThisFrame;
            case GamepadPart.NorthY:  return g.buttonNorth.wasPressedThisFrame;

            case GamepadPart.LB:      return g.leftShoulder.wasPressedThisFrame;
            case GamepadPart.RB:      return g.rightShoulder.wasPressedThisFrame;

            case GamepadPart.Start:   return g.startButton.wasPressedThisFrame;
            case GamepadPart.Select:  return g.selectButton.wasPressedThisFrame;

            case GamepadPart.LT:
                return g.leftTrigger.ReadValue() > triggerPressedThreshold &&
                       g.leftTrigger.ReadValueFromPreviousFrame() <= triggerPressedThreshold;

            case GamepadPart.RT:
                return g.rightTrigger.ReadValue() > triggerPressedThreshold &&
                       g.rightTrigger.ReadValueFromPreviousFrame() <= triggerPressedThreshold;

            default:
                return false;
        }
    }

    // CHANGED: fixed joystick trigger edge handling (uses joystickButtonNumber == -1 entry, not LT/RT)
    private void AddScoreEdges_Joystick(Joystick j)
    {
        for (int i = 0; i < arcadeMap.Length; i++)
        {
            var entry = arcadeMap[i];

            // Digital button edges
            if (entry.joystickButtonNumber >= 0)
            {
                string name = $"button{entry.joystickButtonNumber}";
                var b = j.TryGetChildControl<ButtonControl>(name);
                if (b == null) continue;

                if (b.wasPressedThisFrame)
                {
                    _lastPressed = showTechnicalNames
                        ? $"{entry.arcadeLabel} (Joystick: {name} | Gamepad: {GetGamepadTechnicalName(entry.gamepadPart)})"
                        : entry.arcadeLabel;
                }

                continue;
            }

            // Trigger axis edge (THIS is your "A" mapping on joystick)
            if (entry.joystickButtonNumber == -1)
            {
                var trigger =
                    j.TryGetChildControl<AxisControl>("trigger") ??
                    j.TryGetChildControl<AxisControl>("z") ??
                    j.TryGetChildControl<AxisControl>("rz");

                if (trigger == null) continue;

                bool downThisFrame =
                    trigger.ReadValue() > triggerPressedThreshold &&
                    trigger.ReadValueFromPreviousFrame() <= triggerPressedThreshold;

                if (downThisFrame)
                {
                    _lastPressed = showTechnicalNames
                        ? $"{entry.arcadeLabel} (Joystick: {trigger.name} | Gamepad: {GetGamepadTechnicalName(entry.gamepadPart)})"
                        : entry.arcadeLabel;
                }
            }
        }
    }

    // --------------------------
    // Keyboard fallback (Editor / PC without arcade hardware)
    // --------------------------
    private void AppendKeyboardPanel(StringBuilder sb)
    {
        sb.AppendLine();
        sb.AppendLine("Movement: WASD / Arrow keys");
        if (ArcadeInput.Instance != null)
        {
            var m = ArcadeInput.Instance.Move;
            sb.AppendLine($"  Move: X={m.x:0.00} Y={m.y:0.00}");
        }

        sb.AppendLine();
        sb.AppendLine("Keys: A=U  B=I  C=O  D=J  E=K  F=L  Start=T/Enter  Exit=Y/Esc");
        sb.Append("Pressed now: ");

        bool any = false;
        if (ArcadeInput.Instance != null)
        {
            var a = ArcadeInput.Instance.Actions;
            any |= AppendIfPressed(sb, a.ButtonA, "A");
            any |= AppendIfPressed(sb, a.ButtonB, "B");
            any |= AppendIfPressed(sb, a.ButtonC, "C");
            any |= AppendIfPressed(sb, a.ButtonD, "D");
            any |= AppendIfPressed(sb, a.ButtonE, "E");
            any |= AppendIfPressed(sb, a.ButtonF, "F");
            any |= AppendIfPressed(sb, a.ButtonStart, "Play");
            any |= AppendIfPressed(sb, a.ButtonExit, "ChangeGame");
        }

        if (!any) sb.Append("(none)");
        sb.AppendLine();
    }

    private static bool AppendIfPressed(StringBuilder sb, InputAction action, string label)
    {
        if (!action.IsPressed()) return false;
        sb.Append('[').Append(label).Append("] ");
        return true;
    }

    private void AddScoreEdges_ArcadeInput()
    {
        var input = ArcadeInput.Instance;
        if (input == null) return;

        if (input.ButtonAPressedThisFrame) AddScore("A");
        if (input.ButtonBPressedThisFrame) AddScore("B");
        if (input.ButtonCPressedThisFrame) AddScore("C");
        if (input.ButtonDPressedThisFrame) AddScore("D");
        if (input.ButtonEPressedThisFrame) AddScore("E");
        if (input.ButtonFPressedThisFrame) AddScore("F");
        if (input.ButtonStartPressedThisFrame) AddScore("Play");
    }

    private void AddScore(string label)
    {
        score += 100;
        if (Gamepad.current == null && Joystick.current == null)
            _lastPressed = label; // device-specific labels are set by the gamepad/joystick edge checks
    }

    // --------------------------
    // Hold actions
    // --------------------------
    private void HandleHoldActions()
    {
        if (ArcadeInput.Instance == null) return;

        float dt = Time.unscaledDeltaTime;

        bool bDown = ArcadeInput.Instance.Actions.ButtonB.IsPressed();
        if (bDown)
        {
            _bHeldTime += dt;
            if (_bHeldTime >= holdToWinSeconds)
            {
                _bHeldTime = 0f;
                if (ArcadeSceneManager.Instance != null)
                    ArcadeSceneManager.Instance.LoadWinScreen(score);
            }
        }
        else _bHeldTime = 0f;

        bool cDown = ArcadeInput.Instance.Actions.ButtonC.IsPressed();
        if (cDown)
        {
            _cHeldTime += dt;
            if (_cHeldTime >= holdToLoseSeconds)
            {
                _cHeldTime = 0f;
                if (ArcadeSceneManager.Instance != null)
                    ArcadeSceneManager.Instance.LoadLoseScreen(score);
            }
        }
        else _cHeldTime = 0f;
        
        bool dDown = ArcadeInput.Instance.Actions.ButtonD.IsPressed();

        if (dDown)
        {
            _dHeldTime += dt;

            // Toggle only once per hold
            if (!_detailsToggledThisHold && _dHeldTime >= holdToToggleDetailsSeconds)
            {
                _detailsToggledThisHold = true;
                showTechnicalNames = !showTechnicalNames;

                // Optional: feedback
                _lastPressed = showTechnicalNames ? "Detailed mode: ON" : "Detailed mode: OFF";
            }
        }
        else
        {
            _dHeldTime = 0f;
            _detailsToggledThisHold = false;
        }
        
        
        bool eDown = ArcadeInput.Instance.Actions.ButtonE.IsPressed();

        if (eDown)
        {
            _eHeldTime += dt;

            if (!_soundToggledThisHold && _eHeldTime >= holdToSoundTestSeconds)
            {
                _soundToggledThisHold = true;
                ToggleSoundTest();
            }
        }
        else
        {
            _eHeldTime = 0f;
            _soundToggledThisHold = false;
        }


    }
    
    private void ToggleSoundTest()
    {
        if (soundTestSource == null)
        {
            Debug.LogWarning("[JoystickTester] Sound test AudioSource not assigned.");
            return;
        }

        if (_soundTestActive)
        {
            // Turn OFF
            soundTestSource.Stop();
            _soundTestActive = false;
            _lastPressed = "Sound Test: OFF";
        }
        else
        {
            // Turn ON (always restart)
            soundTestSource.Stop();   // ensures clean restart
            soundTestSource.time = 0f;
            soundTestSource.Play();

            _soundTestActive = true;
            _lastPressed = "Sound Test: ON";
        }
    }



    private void AppendHoldHints(StringBuilder sb)
    {
        sb.AppendLine();
        sb.AppendLine("Hold actions:");
        sb.AppendLine($"  Hold B = WIN    ({_bHeldTime:0.0}s / {holdToWinSeconds:0.0}s)");
        sb.AppendLine($"  Hold C = LOSE   ({_cHeldTime:0.0}s / {holdToLoseSeconds:0.0}s)");
        sb.AppendLine($"  Hold D = DETAILS ({_dHeldTime:0.0}s / {holdToToggleDetailsSeconds:0.0}s)");
        sb.AppendLine($"  Hold E = SOUND  ({_eHeldTime:0.0}s / {holdToSoundTestSeconds:0.0}s)");

    }
}
