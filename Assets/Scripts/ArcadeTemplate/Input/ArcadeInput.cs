using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcadesTemplate.Input
{
    [DefaultExecutionOrder(-200)]
    public class ArcadeInput : MonoBehaviour
    {
        public static ArcadeInput Instance { get; private set; }

        private ArcadeControls _controls;

        public ArcadeControls.ArcadeActions Actions => _controls.Arcade;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _controls = new ArcadeControls();
        }

        private void OnEnable()
        {
            _controls?.Enable();
        }

        private void OnDisable()
        {
            _controls?.Disable();
        }


        // ---- Helpers ----

        public Vector2 Move => _controls.Arcade.Move.ReadValue<Vector2>();

        public bool ButtonAPressedThisFrame =>
            _controls.Arcade.ButtonA.WasPressedThisFrame();

        public bool ButtonBPressedThisFrame =>
            _controls.Arcade.ButtonB.WasPressedThisFrame();

        public bool ButtonCPressedThisFrame =>
            _controls.Arcade.ButtonC.WasPressedThisFrame();

        public bool ButtonDPressedThisFrame =>
            _controls.Arcade.ButtonD.WasPressedThisFrame();

        public bool ButtonEPressedThisFrame =>
            _controls.Arcade.ButtonE.WasPressedThisFrame();

        public bool ButtonFPressedThisFrame =>
            _controls.Arcade.ButtonF.WasPressedThisFrame();

        public bool ButtonStartPressedThisFrame =>
            _controls.Arcade.ButtonStart.WasPressedThisFrame();

        public bool ButtonExitPressedThisFrame =>
            _controls.Arcade.ButtonExit.WasPressedThisFrame();
    }
}