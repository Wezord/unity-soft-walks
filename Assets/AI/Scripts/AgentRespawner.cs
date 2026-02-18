using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentRespawner : MonoBehaviour
{
    public GameObject agentPrefab;
    public Vector3 spawnPosition;
    public Quaternion spawnRotation;

    void Start()
    {
        AgentEvents ev = GetComponent<AgentEvents>();
        ev.endEpisodeEvent += Respawn;

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    private void Respawn()
    {
        Destroy(gameObject);
        Instantiate(agentPrefab, spawnPosition, spawnRotation);
    }

}
