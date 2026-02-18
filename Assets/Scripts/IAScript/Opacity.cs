using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Opacity : MonoBehaviour
{

    public float opacity = 1.0f;
    public Material mat;

    void Start()
    {
        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, opacity);
    }

}
