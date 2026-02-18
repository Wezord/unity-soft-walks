using UnityEngine;

public class SetModifiableContactPair : MonoBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        collider.hasModifiableContacts = true;
    }
}
