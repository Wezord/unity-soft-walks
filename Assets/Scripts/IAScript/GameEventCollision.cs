using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Game/GameEventCollision")]
public class GameEventCollision : ScriptableObject
{
    private event Action<Collision> OnEventRaised;

    public void Raise(Collision collision)
    {
        OnEventRaised?.Invoke(collision);
    }

    public void RegisterListener(Action<Collision> callback)
    {
        OnEventRaised += callback;
    }

    public void UnregisterListener(Action<Collision> callback)
    {
        OnEventRaised -= callback;
    }
}

