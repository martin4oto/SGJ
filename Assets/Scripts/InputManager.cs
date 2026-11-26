using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-50)]
public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public GameObject playerInputHandlePrefab;
    public List<PlayerInputHandler> players = new();
    void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);
    }
    public void ConnectViaNumpad()
    {
        PlayerInputManager.instance.JoinPlayer(1, -1, "KeyboardRight", Keyboard.current);
        Debug.Log("Numpad");
    }

}
