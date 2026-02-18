using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveOnTerrain : MonoBehaviour
{
    public Terrain terrain;

    public void RandomRespawn()
    {
		var rand = new Vector3(Random.value * terrain.terrainData.size.x, 0, Random.value * terrain.terrainData.size.z);
		var pos = terrain.transform.position + rand;
		transform.position = pos;
		SetHeight();
	}

    public void SetHeight()
    {
		var coords = (transform.position - terrain.transform.position) / terrain.terrainData.size.x * terrain.terrainData.heightmapResolution;
		var height = terrain.terrainData.GetHeight((int)coords.x, (int)coords.z) + terrain.transform.position.y;
		var pos = transform.position;
		pos.y = height + 0.2f;
		transform.position = pos;
	}

}
