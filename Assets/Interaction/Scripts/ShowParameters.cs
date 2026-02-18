using TMPro;
using UnityEngine;

public class ShowParameters : MonoBehaviour
{

	public TMP_Text text;
    public Parameters parameters;

    void Update()
    {
        if (!VRPlayerInput.configuring)
        {
            text.text = "";
            return;
        }
		text.text = parameters.ToString();
    }
}
