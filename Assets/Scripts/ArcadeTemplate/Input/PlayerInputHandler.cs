using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-50)]
public class PlayerInputHandler : MonoBehaviour
{
    Dictionary<string, List<Action<InputAction.CallbackContext>>> actionFunctionPairsButtons = new();
    List<UnsubscribeTicket> unsubscribeList = new();
    PlayerInput playerInput;
    public int boundToDeviceId;
    bool autoSwitch = true;
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.onActionTriggered += HandleInput;

        actionFunctionPairsButtons.Add("ButtonA", new ());
        actionFunctionPairsButtons.Add("ButtonB", new ());
        actionFunctionPairsButtons.Add("ButtonC", new ());
        actionFunctionPairsButtons.Add("Stick", new ());
        actionFunctionPairsButtons.Add("Start", new ());
        actionFunctionPairsButtons.Add("Exit", new ());
    }
    void Start()
    {
        if (autoSwitch) playerInput.SwitchCurrentControlScheme("Default", InputSystem.GetDeviceById(boundToDeviceId));
    }

    public void ChangeControlScheme(string newScheme)
    {
        autoSwitch = false;
        playerInput.SwitchCurrentControlScheme(newScheme, InputSystem.GetDeviceById(boundToDeviceId));
    }

    public void Subscribe(string action, Action<InputAction.CallbackContext> func)
    {
        actionFunctionPairsButtons[action].Add(func);
    }

    public void Unsubscribe(string action, Action<InputAction.CallbackContext> func)
    {
        unsubscribeList.Add(new UnsubscribeTicket(action, func));
    }
    void UnsubscribeList()
    {
        foreach(UnsubscribeTicket ticket in unsubscribeList)
        {
            actionFunctionPairsButtons[ticket.action].Remove(ticket.func);
        }
        unsubscribeList.Clear();
    }
    void HandleInput(InputAction.CallbackContext context)
    {
        UnsubscribeList();

        List<Action<InputAction.CallbackContext>> funcs = actionFunctionPairsButtons[context.action.name];
        foreach (Action<InputAction.CallbackContext> func in funcs)
        {
            func?.Invoke(context);
        }

        
    }
    void OnDisable()
    {
        playerInput.onActionTriggered -= HandleInput;
    }
    void OnEnable()
    {
        playerInput.onActionTriggered += HandleInput;
    }
    class UnsubscribeTicket
    {
        public string action;
        public Action<InputAction.CallbackContext> func;
        public UnsubscribeTicket(string _action, Action<InputAction.CallbackContext> _func)
        {
            action = _action;
            func = _func;
        }
    }
}
