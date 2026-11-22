using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-50)]
public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public GameObject playerInputHandlePrefab;
    public List<PlayerInputHandler> players = new();
    int connected;
    void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);
        if (Gamepad.all.Count != 0){

            for (int i = 0; i < 2; i++)
            {
                if (i < Gamepad.all.Count)
                {
                    Debug.Log("Gamepad");
                    GameObject handlerObject = Instantiate(playerInputHandlePrefab, transform);
                    players.Add(handlerObject.GetComponent<PlayerInputHandler>());
                    players[i].boundToDeviceId = Gamepad.all[i].deviceId;
                    connected++;
                }
            }
        }
        if (connected < 2 && Joystick.all.Count != 0){
            for (int i = 0; i < Joystick.all.Count; i++)
            {
                Debug.Log("JS" + Joystick.all[i].deviceId + " " + Joystick.all[i].name);
            }

            for (int i = 0; i < 2 - connected; i++)
            {
                if (i < Joystick.all.Count)
                {
                    Debug.Log("Joystick");
                    GameObject handlerObject = Instantiate(playerInputHandlePrefab, transform);
                    players.Add(handlerObject.GetComponent<PlayerInputHandler>());
                    players[i].boundToDeviceId = Joystick.all[i].deviceId;
                    connected++;
                }
            }
        }
        if (connected < 2 && InputSystem.devices.Count != 0){

            if (connected == 0)
            {
                Debug.Log("Double keyboard");
                GameObject handlerObject = Instantiate(playerInputHandlePrefab, transform);
                players.Add(handlerObject.GetComponent<PlayerInputHandler>());
                players[0].boundToDeviceId = InputSystem.devices[0].deviceId;
                players[0].ChangeControlScheme("Keyboard");
                connected++;

                handlerObject = Instantiate(playerInputHandlePrefab, transform);
                players.Add(handlerObject.GetComponent<PlayerInputHandler>());
                players[1].boundToDeviceId = InputSystem.devices[0].deviceId;
                players[1].ChangeControlScheme("KeyboardRight");
                connected++;
            }else{
                Debug.Log("1 keyboard");
                GameObject handlerObject = Instantiate(playerInputHandlePrefab, transform);
                players.Add(handlerObject.GetComponent<PlayerInputHandler>());
                players[1].boundToDeviceId = InputSystem.devices[0].deviceId;
                connected++;
            }
        }

        if (connected != 2)
        {   
            Debug.LogError("Unable to find 2 input devices! Exiting.");
            Application.Quit();
        }
        
    }
}
