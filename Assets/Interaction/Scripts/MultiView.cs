using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiView : MonoBehaviour
{

    public GameObject[] views;

    void Update()
    {
        for(int i = 0; i<views.Length; i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1+i))
            {
                views[i].SetActive(true);
                foreach(GameObject view in views)
                {
					if(view != views[i]) view.SetActive(false);
				}
            }
        }
    }
}
