using System.Linq;
using UnityEngine;

public class SoftGroundMeshScript : MonoBehaviour
{
    public float radius;
    public float force;
    public float recoverySpeed;

    public float deformationLength = 200f;

    public bool isX = false;
    public bool isY = true;
    public bool isZ = false;

    private Mesh mesh;
    private Vector3[] baseVertices;
    private Vector3[] deformedVertices;

    private MeshCollider meshCollider;
    private int frameCounter = 0;
    public int colliderUpdateRate = 8; // update toutes les 8 frames
    
    public float maxDepth = 0f;
    public float limitDepth = -0.2f;

    public GameEventFloat maxDepthChangeEvent;

    public GameEventCollision collisionSoft;

    void Start()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        //if(collisionSoft != null)
        //    collisionSoft.RegisterListener(OnCollisionStayEvent); 
        mesh = Instantiate(mf.mesh);
        mf.mesh = mesh;

        baseVertices = mesh.vertices;
        deformedVertices = mesh.vertices;

        meshCollider = GetComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        if (maxDepthChangeEvent != null)
        {
            maxDepthChangeEvent.RegisterListener((float newDepth) =>
            {
            maxDepth = newDepth;
            });
        }
    }

    void OnDisable()
    {
        //if(collisionSoft != null)
        //    collisionSoft.UnregisterListener(OnCollisionStayEvent);
    }

    void Update()
    {
        bool changed = false;

        // Retour progressif
        for (int i = 0; i < deformedVertices.Length; i++)
        {
            if (deformedVertices[i].y > -9000f) // vertex non détruit
            {
                Vector3 newV = Vector3.Lerp(
                    deformedVertices[i],
                    baseVertices[i],
                    Time.deltaTime * recoverySpeed
                );

                if (newV != deformedVertices[i])
                {
                    deformedVertices[i] = newV;
                    changed = true;
                }
            }
        }

        if (changed)
        {
            mesh.vertices = deformedVertices;
            mesh.RecalculateNormals();

            // Mise à jour du MeshCollider (pas chaque frame)
            frameCounter++;
            if (frameCounter >= colliderUpdateRate)
            {
                frameCounter = 0;
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = mesh;
            }
        }
    }



    void OnCollisionStay(Collision col)
    {   
        float impactForce = col.rigidbody != null ? col.rigidbody.mass : 1f;
        float speed = col.relativeVelocity.magnitude;
        //Debug.Log("Collision speed: " + speed);
        //Debug.Log("Impact force: " + impactForce);
        float deformationFactor = Mathf.Clamp(Mathf.Sqrt(impactForce) / 4f, 0.15f, 0.85f);
        //Debug.Log("Deformation factor: " + deformationFactor);
        //Debug.Log("COLLISION !");
        Vector3 point = transform.InverseTransformPoint(col.contacts[0].point);
        bool changed = false;

        for (int i = 0; i < deformedVertices.Length; i++)
        {
            float dist = Vector3.Distance(deformedVertices[i], point);

            if (dist < radius)
            {
                // DESTRUCTION X
                if (isX)
                {
                    deformedVertices[i].y = -deformationLength;
                    changed = true;
                    continue;
                }

                // DESTRUCTION Z
                if (isZ)
                {
                    deformedVertices[i].y = -deformationLength;
                    changed = true;
                    continue;
                }

                // DÉFORMATION Y
                if (isY)
                {
                    //deformedVertices[i].y -= Mathf.Min((1f - dist / radius) * force * col.impulse.magnitude * Random.Range(0f, 1f) * Mathf.Log(speed + 1) * deformationFactor, maxDepth);
                    //if(maxDepth > 0 && (deformedVertices[i].y - maxDepth) > limitDepth && deformedVertices[i].y > -maxDepth )
                    if((deformedVertices[i].y - 0.05) > limitDepth)
                    {
                        deformedVertices[i].y -= 0.05f;
                        changed = true;
                    }
                }
            }
        }

        if (changed)
        {
            mesh.vertices = deformedVertices;
            mesh.RecalculateNormals();

            // Update du MeshCollider immédiat en cas de collision
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }

    public void ResetDeformation()
    {
        for (int i = 0; i < deformedVertices.Length; i++)
            deformedVertices[i] = baseVertices[i];

        mesh.vertices = deformedVertices;
        mesh.RecalculateNormals();

        if (meshCollider)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }
}