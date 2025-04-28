using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FSM
{
    private Enum _currentState;
    private Dictionary<Enum,Dictionary<Func<bool>,Enum>> _stateMap = new Dictionary<Enum, Dictionary<Func<bool>, Enum>>();
    private Dictionary<zBehaviour, Action> _behaviourMap = new Dictionary<zBehaviour, Action>();
    
    public FSM(Enum startState){
        _currentState = startState;
    }
    public Enum GetCurrentState(){
        return _currentState;
    }
    public void UpdateState()
    {
        if (!_stateMap.ContainsKey(_currentState))
        {
            Debug.Log($"State {_currentState} not found in _stateMap.");
            return;
        }
        foreach (var item in _stateMap[_currentState])
        {
            if (item.Key()){
                _currentState = item.Value;
                return;
            }
        }
    }
    public void SetState(zBehaviour state){
        _currentState = state;
    }
    public void AddState(Enum fromState, Func<bool> transition, Enum toState){
        if (_stateMap.ContainsKey(fromState)){
            _stateMap[fromState][transition] = toState;
        }
        else {
            _stateMap[fromState] = new Dictionary<Func<bool>, Enum>
            {
                [transition] = toState
            };
        }
    }
    public void AddBehaviour(zBehaviour state, Action action){
        _behaviourMap[state] = action;
    }
    public void DoAction(zBehaviour state){
        if (_behaviourMap.TryGetValue(state, out var action))
        {
            action();
        }
        else
        {
            Debug.Log("No function mapped");
        }
    }
}

