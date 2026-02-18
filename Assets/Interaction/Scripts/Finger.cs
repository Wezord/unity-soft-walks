using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finger : MonoBehaviour
{

    [HideInInspector]
    public Hand hand;

    [Range(0, 1)] public float position;

    public Phalanx[] phalanxes = new Phalanx[3];


    public void Initialize(Hand hand)
    {
        this.hand = hand;
        foreach (Phalanx phalanx in phalanxes)
        {
            phalanx.Initialize(this);
        }
    }

}
