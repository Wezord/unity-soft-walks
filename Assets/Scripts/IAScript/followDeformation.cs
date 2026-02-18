using UnityEngine;

public class followDeformation : MonoBehaviour
{
    public Transform player;         // ton personnage
    public float patchSize = 8f;     // taille du plan
    public float repositionThreshold = 2f; // distance avant recentrage

    private Vector3 lastCenterPos;

    void Start()
    {
        // Initialise le dernier centre
        lastCenterPos = player.position;
        MovePatchToPlayer();
    }

    void Update()
    {
        float dist = Vector3.Distance(player.position, lastCenterPos);

        // Si le joueur s’est éloigné du centre du patch → recentrer
        if (dist > repositionThreshold)
        {
            MovePatchToPlayer();
        }
    }

    void MovePatchToPlayer()
    {
        // centre sous le joueur
        Vector3 newPos = new Vector3(
            player.position.x,
            transform.position.y,
            player.position.z
        );

        transform.position = newPos;
        lastCenterPos = newPos;

        // IMPORTANT : reset les vertices vers l’état “plat”
        var soft = GetComponent<SoftGroundMeshScript>();
        if (soft)
        {
            soft.ResetDeformation();
        }
    }
}
