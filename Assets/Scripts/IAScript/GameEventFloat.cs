using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/GameEventFloat")]
public class GameEventFloat : ScriptableObject
{
    private event Action<float> OnEventRaised = delegate { };

    public void Raise(float value)
    {
        OnEventRaised.Invoke(value);
    }

    public void RegisterListener(Action<float> listener)
    {
        OnEventRaised += listener;
    }

    public void UnregisterListener(Action<float> listener)
    {
        OnEventRaised -= listener;
    }
}
