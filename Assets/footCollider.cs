using UnityEngine;

public class footCollider : MonoBehaviour
{
    public CapsuleCollider capsuleCollider;
    public BoxCollider toe;
    public BoxCollider foot;

    void FixedUpdate()
    {
        if (!capsuleCollider || !toe || !foot)
            return;

        // Centres réels en world
        Vector3 footWorld = foot.transform.TransformPoint(foot.center);
        Vector3 toeWorld  = toe.transform.TransformPoint(toe.center);

        // Direction réelle pied -> orteils
        Vector3 axis = toeWorld - footWorld;
        float length = axis.magnitude;
        Vector3 direction = axis.normalized;

        // Centre exact
        transform.position = footWorld + direction * (length * 0.5f);

        // Rotation parfaitement alignée sur l'axe réel
        transform.rotation = Quaternion.LookRotation(direction, foot.transform.up);

        // Radius cohérent (largeur du pied)
        float width = foot.size.x * foot.transform.lossyScale.x;
        capsuleCollider.radius = width * 0.6f;

        // Hauteur réelle
        capsuleCollider.height = length + capsuleCollider.radius * 2f + 0.1f;

        Debug.DrawLine(footWorld, toeWorld, Color.red);
    }
}