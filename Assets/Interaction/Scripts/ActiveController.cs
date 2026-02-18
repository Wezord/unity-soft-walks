using UnityEngine;

public class ActiveController : MonoBehaviour
{
    public GenericPlayerInput input;
    public GameObject ui = null;

    private void Awake()
    {
        FixedUpdate();
    }

    void FixedUpdate()
    {
        if (input == null) return;
        if(input.IsActive()) {
            input.gameObject.SetActive(true);
            if (ui) ui.SetActive(false);
            enabled = false;
        } else if (input.gameObject != null)
        {
			input.gameObject.SetActive(false);
		}
    }

}
