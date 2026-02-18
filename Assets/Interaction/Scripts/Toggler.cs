using UnityEngine;

public class Toggler : MonoBehaviour
{

    public GameObject target;

    private GenericPlayerInput input;

    private bool released = true;

    void Start()
    {
        input = GenericPlayerInput.GetInput(gameObject);
    }

    void FixedUpdate()
    {
        if (input.GetInteractButton() && released)
        {
            released = false;
            target.SetActive(!target.activeSelf);
        }

        if (!input.GetInteractButton())
        {
            released = true;
        }
    }
}
