# DHO Arcade – Game Jam Template

A free-to-play Unity starter for making games for the DevHubOne (DHO) arcade cabinet.
It gives you the arcade flow (start screen → game → win/lose → high-score name entry → replay),
the arcade controls and localization. **You only write the gameplay.**

Tested with **Unity 6000.3.10f1** (Unity 6.3). Use the same or a newer Unity 6 version.

---

## 1. Import & first run

1. Create a new Unity 6 project (Universal 3D / URP template recommended).
2. **Assets → Import Package → Custom Package…** and select `DHO-Arcade-GameJam-Template.unitypackage`.
3. After the import, the editor-only setup runs automatically (also available in
   **Tools → DHO Arcade → Setup Game Jam Template**). It:
   - installs the required packages if they are missing:
     `com.unity.localization` 1.5.13 (brings Addressables), `com.unity.inputsystem` 1.18.0,
     `com.unity.render-pipelines.universal` 17.3.0 and `com.unity.ugui` 2.0.0 (TextMeshPro),
   - registers the template's Localization and Addressables settings,
   - puts the template scenes into Build Settings (LoadingScene first),
   - imports the TextMeshPro Essential Resources if the project does not have them yet.
4. If Unity asks to enable the new Input System backend, click **Yes** (the editor restarts).

> If you see compile errors about `UnityEngine.Localization` right after import, wait for the package
> install to finish, or install **Localization** from *Window → Package Manager* and run the setup menu again.

## 2. Which scene to open

Open **`Assets/Scenes/ArcadeTemplate/StartScene.unity`** and press **Play**.
(`LoadingScene` is build index 0 and just loads StartScene. *Window → PlayWindow* has a
"Play From StartScene" button that always starts from the right scene.)

StartScene contains the persistent systems that survive scene changes (`DontDestroyOnLoad`):
`ArcadeInput`, `ArcadeSceneManager`, `LeaderboardSystem`, `MessagePopup`, `AttractModeController`.
**Always start Play Mode from StartScene/LoadingScene**; if you play MainScene directly these systems are missing.

## 3. Controls

| Arcade cabinet | Keyboard (Editor / PC) | Gamepad |
|---|---|---|
| Joystick | WASD / Arrow keys | Left stick / D-pad |
| A | U | Y / North |
| B | I | B / East |
| C | O | A / South |
| D | J | X / West |
| E | K | LB |
| F | L | RB |
| Play (Start) | T or Enter | Start |
| Change Game (Exit) – press twice | Y or Esc | Select / Back |

Bindings live in `Assets/Input/ArcadeControls.inputactions` (the C# wrapper `ArcadeControls.cs` is generated from it).

```csharp
using ArcadesTemplate.Input;

Vector2 move = ArcadeInput.Instance.Move;
if (ArcadeInput.Instance.ButtonAPressedThisFrame) { /* jump */ }
if (ArcadeInput.Instance.Actions.ButtonB.IsPressed()) { /* held */ }
```

## 4. The sample game (MainScene)

`MainScene` contains `JoystickTester`, a control-test "game":

- every button press = **+100 points**, the screen shows which control is pressed,
- **hold B for 3 s → WIN**, **hold C for 3 s → LOSE**,
- hold D 1 s = toggle technical names, hold E 3 s = sound test.

Use it to check the whole flow and your controller mapping.

## 5. Replacing the sample with your game

1. Edit `MainScene` (or create your own scene and change **Main Scene Name** on the
   `ArcadeSceneManager` prefab, and add the scene to Build Settings).
2. Delete the `JoystickTester` object.
3. Read input via `ArcadeInput.Instance` (see above).
4. When the game ends, report the result:

```csharp
using ArcadesTemplate.Core;

ArcadeSceneManager.Instance.LoadWinScreen(score);   // player won
ArcadeSceneManager.Instance.LoadLoseScreen(score);  // player lost
```

That's all. Start, results, high scores and replay are handled for you.

## 6. How the reusable scenes work

| Scene | What it does |
|---|---|
| `StartScene` | Title + leaderboard. **A / Start** → `LoadMainGame()`. Optional attract video after idle time. |
| `MainScene` | Your game. Call `LoadWinScreen(score)` / `LoadLoseScreen(score)`. |
| `WinScene` / `LoseScene` | Show the score. If it is a top-10 score, NameEntry opens first. **A / Start** = play again, **B** = back to StartScene. Shared logic: `ResultScreenController`. |
| `NameEntryScene` | 3-letter name with the joystick (up/down = letter, left/right = position), **A** = next/confirm, **B** = skip. Includes a profanity filter (`Resources/bad_words.txt`). |

Scores are saved locally in `Application.persistentDataPath/leaderboard.json` (top 10).

Texts are localized (English + Bulgarian) with Unity Localization: `Assets/Localization/LocalizationTables`.
Edit them in *Window → Asset Management → Localization Tables*.

## 7. Restart without credits

There is **no credit system**. `ArcadeSceneManager.LoadMainGame()` / `Replay()` always load the game immediately.
Players can replay indefinitely from the Win/Lose screens or the Start screen. No popup, no database,
no launcher is involved.

## 8. Exit / Change Game

Pressing **Exit** once shows "PRESS AGAIN TO EXIT"; pressing it again within 3 s quits
(`Application.Quit()` in a build, stops Play Mode in the Editor). The arcade launcher then shows its game menu.
Keep this behaviour – players must always be able to leave your game.

## 9. Building for the arcade (Linux x86_64)

1. Install the **Linux Build Support (Mono)** module for your Unity version in Unity Hub.
2. *File → Build Profiles* → **Linux** → Architecture **x86_64** → *Switch Platform*.
3. Check that the scene list starts with `LoadingScene`, then `StartScene`, `MainScene`, `WinScene`, `LoseScene`, `NameEntryScene`.
4. *Player Settings*: set **Company Name** / **Product Name** to your team and game.
   Recommended: Fullscreen Mode = *Fullscreen Window*, 1920×1080.
5. **Build**. Submit the whole output folder (executable, `*_Data` folder, `UnityPlayer.so`).
6. If possible, test the build on a Linux machine with a gamepad before submitting.

Localization uses Addressables; Unity builds the Addressables content automatically during the player build.
If texts are missing in a build, run *Window → Asset Management → Addressables → Groups → Build → New Build → Default Build Script* and build again.

## 10. Removed legacy arcade features

This template was derived from the internal DHO arcade template. The following were **removed**:

- the credit system (credit manager, SQLite credit database, credit checks, credit consumption, credit popups,
  `DHO_DB_PATH` environment variable, test database, credit helper script),
- the vending/reward hardware (vending controller, serial-port communication, dispense commands,
  vending status UI and texts, "hold A to vend" in the joystick tester),
- the "requires credit" replay mode.

## 11. Do not assume on the real machine

- no network / internet access, no online services or accounts,
- no credits, coin or payment API, no vending or other hardware besides joystick + buttons,
- no keyboard or mouse for players (keyboard keys are for development only),
- no admin rights, no access to files outside your game folder and `Application.persistentDataPath`,
- no specific OS tools or shell commands; don't start other processes,
- the machine may be restarted at any time – save only what you need (e.g. the leaderboard).

The optional `ATTRACT_MODE` environment variable (`1`/`0`) enables the idle attract video
(`Assets/StreamingAssets/demo_video.webm`); it is off by default and the game works without it.

> This template contains no hardware or credit code, but it does **not** sandbox anything:
> any game binary can still run arbitrary code. Games are reviewed before they are installed on the arcade.
