using UnityEngine;
using System.Collections.Generic;

public class chunkManager : MonoBehaviour
{
    public Terrain terrain;
    public Transform player;

    public float chunkSize = 8f;    // Chunk = 8x8 comme demandé
    public int resolution = 32;

    public float loadDistance = 20f;
    public float unloadDistance = 40f;

    public float minForce = 0.01f;
    public float maxForce = 0.5f;

    public float minSize = 0.01f;
    public float maxSize = 0.5f;

    public float minRecovery = 0.01f;
    public float maxRecovery = 0.5f;

    public int lod0Resolution = 64;
    public int lod1Resolution = 32;
    public int lod2Resolution = 16;
    public int lod3Resolution = 8;

    public float lod0Distance = 4f;
    public float lod1Distance = 8f;
    public float lod2Distance = 12f;

    public float maxDepth = 0f;

    public bool isTraining = false;
    
    public MatrixData matrixData;

    public float limitDepth = -100000f;

    public int chunkNb = 4;

    Dictionary<Vector2Int, ChunkData> chunks = new Dictionary<Vector2Int, ChunkData>();

    public GameEvent resetChunksEvent;

    public GameEventFloat maxDepthChangeEvent;

    public GameEventCollision collisionSoft;

    TerrainData data;
    Vector3 terrainOffset;

    public enum ChunkLOD
    {
        LOD0,   // Haute résolution
        LOD1,   // Moyen
        LOD2,   // Bas
        LOD3    // Très bas
    }

    public enum SurfaceType
    {
        Solid,
        Snow,
        Mud,
        Sand
    }

    SurfaceType ComputeSurfaceType(int chunkValue)
    {
        if (chunkValue == 0) return SurfaceType.Solid;
        if (chunkValue == 1) return SurfaceType.Snow;
        if (chunkValue == 2) return SurfaceType.Mud;
        if (chunkValue == 3) return SurfaceType.Sand;
        return SurfaceType.Solid;
    }

    SoftGroundMeshScript SoftGroundCompoenent(SurfaceType st, GameObject go, Vector2Int id)
    {
        switch (st)
        {
            case SurfaceType.Snow:
                // Config neige
                minForce = 0.05f;
                maxForce = 0.15f;
                minSize = 0.05f;
                maxSize = 0.015f;
                minRecovery = 0f;
                maxRecovery = 0f;
                break;
            case SurfaceType.Sand:
                minForce = 0.1f;
                maxForce = 0.7f;
                minSize = 0.1f;
                maxSize = 0.6f;
                minRecovery = 0.05f;
                maxRecovery = 0.15f;
                break;
            case SurfaceType.Mud:
                minForce = 0.5f;
                maxForce = 0.7f;
                minSize = 0.05f;
                maxSize = 0.15f;
                minRecovery = 0.02f;
                maxRecovery = 0.06f;
                break;
            default:
                minForce = 0.1f;
                maxForce = 0.7f;
                minSize = 0.1f;
                maxSize = 0.6f;
                minRecovery = 0.05f;
                maxRecovery = 0.3f;
                break;
        }

        var sg = go.AddComponent<SoftGroundMeshScript>();
        //Debug.Log("valeur du chunk : " + matrixData.matrix[id.y].values[id.x]);
        sg.radius = Random.Range(minSize, maxSize);
        sg.force = Random.Range(minForce, maxForce);
        sg.recoverySpeed = Random.Range(minRecovery, maxRecovery);

        return sg;
    }

    public void depthChange(float depth)
    {
        maxDepth = depth;
        //Debug.Log("New max depth: " + maxDepth);
    }

    ChunkLOD ComputeLOD(float d)
    {
        if (d < lod0Distance) return ChunkLOD.LOD0;
        if (d < lod1Distance) return ChunkLOD.LOD1;
        if (d < lod2Distance) return ChunkLOD.LOD2;
        return ChunkLOD.LOD3;
    }

    int GetResolutionForLOD(ChunkLOD lod)
    {
        switch (lod)
        {
            case ChunkLOD.LOD0: return lod0Resolution;
            case ChunkLOD.LOD1: return lod1Resolution;
            case ChunkLOD.LOD2: return lod2Resolution;
            case ChunkLOD.LOD3: return lod3Resolution;
        }
        return lod3Resolution;
    }

    class ChunkData
    {
        public GameObject go;
        public ChunkLOD lod;
    }

    void ReplaceChunkLOD(Vector2Int id, ChunkLOD newLOD)
    {
        Destroy(chunks[id].go);
        GenerateChunk(id, newLOD);
    }

    void Start()
    {
        data = terrain.terrainData;
        terrainOffset = terrain.GetPosition();
        
        resetChunksEvent.RegisterListener(resetChunk);
        maxDepthChangeEvent.RegisterListener(depthChange);

    }

    void Update()
    {
        Vector3 pos = player.position;

        // Position relative au terrain
        float localX = pos.x - terrainOffset.x;
        float localZ = pos.z - terrainOffset.z;

        // Chunk dans lequel est le joueur
        int cx = Mathf.FloorToInt(localX / chunkSize);
        int cz = Mathf.FloorToInt(localZ / chunkSize);

        // Génère autour
        for (int x = cx - chunkNb; x <= cx + chunkNb; x++)
        {
            for (int z = cz - chunkNb; z <= cz + chunkNb; z++)
            {
                Vector2Int id = new(x, z);

                Vector3 center = terrainOffset + new Vector3(
                    x * chunkSize + chunkSize / 2,
                    0,
                    z * chunkSize + chunkSize / 2
                );

                // 🔥 Utilise distance XZ uniquement (ignore Y)
                float distXZ = Vector2.Distance(
                    new Vector2(pos.x, pos.z),
                    new Vector2(center.x, center.z)
                );

                if (distXZ > loadDistance)
                    continue;

                // Déterminer le LOD ciblé
                ChunkLOD targetLOD = ComputeLOD(distXZ);

                // Le chunk n'existe pas → création
                if (!chunks.ContainsKey(id))
                {
                    GenerateChunk(id, targetLOD);
                    continue;
                }

                // Si le LOD n'est pas le bon → remplacement
                if (chunks[id].lod != targetLOD)
                {
                    ReplaceChunkLOD(id, targetLOD);
                }
            }
        }

        // Décharger les chunks
        List<Vector2Int> toRemove = new();
        foreach (var kv in chunks)
        {
            Vector2Int id = kv.Key;

            Vector3 center = terrainOffset + new Vector3(
                id.x * chunkSize + chunkSize / 2,
                0,
                id.y * chunkSize + chunkSize / 2
            );

            // 🔥 Même correction ici
            float distXZ = Vector2.Distance(
                new Vector2(pos.x, pos.z),
                new Vector2(center.x, center.z)
            );

            if (distXZ > unloadDistance)
                toRemove.Add(id);
        }

        foreach (var id in toRemove)
        {
            Destroy(chunks[id].go);
            chunks.Remove(id);
        }
    }

    void GenerateChunk(Vector2Int id, ChunkLOD lod)
    {

        int resolution = GetResolutionForLOD(lod);
        Mesh m = TerrainChunkMeshBuilder.BuildChunkMesh(data, id.x, id.y, resolution, chunkSize);

        GameObject go = new GameObject($"Chunk_{id.x}_{id.y}");
        go.transform.parent = transform;

        go.transform.position = terrain.GetPosition() + new Vector3(
            id.x * chunkSize,
            0f,
            id.y * chunkSize
        );

        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        var mc = go.AddComponent<MeshCollider>();

        /*var csg = go.AddComponent<CompatibleSoftGround>();
        csg.collisionSoft = collisionSoft;
        csg.beta = 0.4f;
        csg.maxDepthChangeEvent = maxDepthChangeEvent;
        csg.C = 300000f;
        csg.g = 20000f;
        csg.unitWeight = 20000f;
        csg.friction = 100f;
        csg.angularFriction = 100f;
        csg.maxDepth = 0;
        csg.nbCell = 5;
        csg.nbDt = 5;*/

        mf.mesh = m;
        mr.sharedMaterial = terrain.materialTemplate;

        mc.sharedMesh = null;
        mc.sharedMesh = m;

        var rb = go.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        if (lod == ChunkLOD.LOD0){
            if (!isTraining){
                //Debug.Log("valeur du chunk : " + matrixData.matrix[id.y].values[id.x]);
                SurfaceType st = ComputeSurfaceType(matrixData.matrix[id.y].values[id.x]);
                SoftGroundCompoenent(st, go, id);
            }
            else {
                var sg = go.AddComponent<SoftGroundMeshScript>();
                sg.radius = Random.Range(minSize, maxSize);
                sg.force = Random.Range(minForce, maxForce);
                sg.recoverySpeed = Random.Range(minRecovery, maxRecovery);
                sg.limitDepth = limitDepth;
                sg.collisionSoft = collisionSoft;

                //float depth = Random.Range(0.01f, 0.2f);
                sg.maxDepth = 0f;
                sg.maxDepthChangeEvent = maxDepthChangeEvent;
                
                //maxDepthChangeEvent.Raise(depth);
            }
        }



        go.tag = "chunk";

        chunks[id] = new ChunkData { go = go, lod = lod };
    }

    public void resetChunk()
    {
        foreach (var kv in chunks)
        {
            Destroy(kv.Value.go);
        }
        chunks.Clear();
    }
}
