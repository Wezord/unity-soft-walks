using UnityEngine;

public static class TerrainChunkMeshBuilder
{
    public static Mesh BuildChunkMesh(TerrainData data, int chunkX, int chunkZ, int resolution, float chunkSize)
    {
        int vertsPerLine = resolution + 1;

        Vector3[] vertices = new Vector3[vertsPerLine * vertsPerLine];
        int[] triangles = new int[resolution * resolution * 6];

        float step = chunkSize / resolution;

        int tri = 0;

        // Micro-optimisations : pré-calcul des réciproques et des offsets de chunk,
        // et clamp des coordonnées normalisées avant l'échantillonnage.
        float invSizeX = data.size.x != 0f ? 1f / data.size.x : 0f;
        float invSizeZ = data.size.z != 0f ? 1f / data.size.z : 0f;
        float baseWorldX = chunkX * chunkSize;
        float baseWorldZ = chunkZ * chunkSize;

        for (int z = 0; z <= resolution; z++)
        {
            float lz = z * step;
            float worldZ = baseWorldZ + lz;
            float normalizedZ = Mathf.Clamp01(worldZ * invSizeZ);

            for (int x = 0; x <= resolution; x++)
            {
                float lx = x * step;
                float worldX = baseWorldX + lx;
                float normalizedX = Mathf.Clamp01(worldX * invSizeX);

                float height = data.GetInterpolatedHeight(normalizedX, normalizedZ);

                int i = x + z * vertsPerLine;
                vertices[i] = new Vector3(lx, height, lz);
            }
        }

        // Triangles
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int a = x + z * vertsPerLine;
                int b = a + 1;
                int c = a + vertsPerLine;
                int d = c + 1;

                triangles[tri++] = a;
                triangles[tri++] = d;
                triangles[tri++] = b;

                triangles[tri++] = a;
                triangles[tri++] = c;
                triangles[tri++] = d;
            }
        }

        Mesh m = new Mesh();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        m.vertices = vertices;
        m.triangles = triangles;

        m.RecalculateNormals();
        m.RecalculateBounds();

        return m;
    }
}
