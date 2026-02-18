using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Map/Matrix Data")]
public class MatrixData : ScriptableObject
{
    public int chunkSize = 8;
    public int chunksX;
    public int chunksZ;

    [System.Serializable]
    public class Row
    {
        public List<int> values = new();
    }

    public List<Row> matrix = new();

    public void Initialize(Terrain terrain)
    {
        TerrainData data = terrain.terrainData;

        chunksX = Mathf.CeilToInt(data.size.x / chunkSize);
        chunksZ = Mathf.CeilToInt(data.size.z / chunkSize);

        matrix.Clear();
        for (int z = 0; z < chunksZ; z++)
        {
            Row r = new Row();
            for (int x = 0; x < chunksX; x++)
                r.values.Add(0);

            matrix.Add(r);
        }

        Debug.Log($"[EDITOR] Matrix initialized: {chunksX}×{chunksZ}");
    }
}
