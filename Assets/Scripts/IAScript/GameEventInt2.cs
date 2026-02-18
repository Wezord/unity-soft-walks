using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/GameEvent (int,int, int)")]
public class GameEventInt2 : ScriptableObject
{
    private event Action<int, int, int> OnEventRaised = delegate { };

    public void Raise(int a, int b, int c)
    {
        OnEventRaised.Invoke(a, b, c);
    }

    public void RegisterListener(Action<int, int, int> listener)
    {
        OnEventRaised += listener;
    }

    public void UnregisterListener(Action<int, int, int> listener)
    {
        OnEventRaised -= listener;
    }
}
