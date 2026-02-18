using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/GameEvent")]
public class GameEvent : ScriptableObject
{
    private event Action OnEventRaised = delegate { };

    public void Raise(){
        OnEventRaised.Invoke();
    }

    public void RegisterListener(Action listener) => OnEventRaised += listener;

    public void UnregisterListener(Action listener) => OnEventRaised -= listener;
}
