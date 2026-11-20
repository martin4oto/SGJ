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
        if (Gamepad.all.Count != 0){
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                Debug.Log(Gamepad.all[i].deviceId + " " + Gamepad.all[i].name);
            }

            for (int i = 0; i < 2; i++)
            {
                if (i < Gamepad.all.Count)
                {
                    Debug.Log(Gamepad.all[i].deviceId);
                    GameObject handlerObject = Instantiate(playerInputHandlePrefab, transform);
                    players.Add(handlerObject.GetComponent<PlayerInputHandler>());
                    players[i].boundToDeviceId = Gamepad.all[i].deviceId;
                }
            }
        }else{
            for (int i = 0; i < Joystick.all.Count; i++)
            {
                Debug.Log(Joystick.all[i].deviceId + " " + Joystick.all[i].name);
            }

            for (int i = 0; i < 2; i++)
            {
                if (i < Joystick.all.Count)
                {
                    Debug.Log(Joystick.all[i].deviceId);
                    GameObject handlerObject = Instantiate(playerInputHandlePrefab, transform);
                    players.Add(handlerObject.GetComponent<PlayerInputHandler>());
                    players[i].boundToDeviceId = Joystick.all[i].deviceId;
                }
            }
        }
        
    }
}
