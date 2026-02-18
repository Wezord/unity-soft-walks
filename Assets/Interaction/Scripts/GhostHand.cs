using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostHand : MonoBehaviour
{

    public Hand hand;

    void FixedUpdate()
    {

        if (VRPlayer.player.ghost)
        {
            transform.position = hand.player.transform.TransformPoint(hand.position - hand.player.position);
            transform.rotation = hand.player.transform.rotation * hand.rotation;
        } else
        {
            gameObject.SetActive(false);
        }

    }
}
