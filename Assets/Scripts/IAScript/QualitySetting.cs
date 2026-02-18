using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualitySetting : MonoBehaviour
{
    public int qualityLevel = 0; // Set the desired quality level here
    
    void Awake()
    {
        QualitySettings.SetQualityLevel(qualityLevel, true);
    }
}
