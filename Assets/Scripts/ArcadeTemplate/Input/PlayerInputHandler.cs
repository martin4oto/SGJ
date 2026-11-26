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
        actionFunctionPairsButtons.Add("NumpadJoin", new ());

        Subscribe("NumpadJoin" , HandleConnectViaNumpad);

        transform.SetParent(InputManager.instance.transform);

        InputManager.instance.players.Add(this);
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
    void HandleConnectViaNumpad(InputAction.CallbackContext context)
    {
        if (InputManager.instance.players.Count != 1) return;
        InputManager.instance.ConnectViaNumpad();
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
