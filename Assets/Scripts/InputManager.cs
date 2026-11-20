using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-50)]
public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    public GameObject playerInputHandlePrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    int boundInputs;
    public List<PlayerInputHandler> players = new();
    void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Debug.Log(Gamepad.all[i].deviceId + " " + Gamepad.all[i].name);
        }

        for (int i = 0; i < 2; i++)
        {
            if (boundInputs < 2 && boundInputs < Gamepad.all.Count)
            {
                Debug.Log(Gamepad.all[boundInputs].deviceId);
                GameObject handlerObject = Instantiate(playerInputHandlePrefab, transform);
                players.Add(handlerObject.GetComponent<PlayerInputHandler>());
                players[boundInputs].boundToDeviceId = Gamepad.all[boundInputs].deviceId;
                boundInputs++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
