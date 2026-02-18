using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameters : MonoBehaviour
{

    public float p1 = 0;
    public float p2 = 0;
    public float p3 = 0;
    public float p4 = 0;

    public override string ToString()
    {
		return $"p1: {p1}\np2: {p2}\np3: {p3}\np4: {p4}";
	}

}
