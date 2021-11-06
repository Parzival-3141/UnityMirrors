using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamMirrorHelper : MonoBehaviour
{
    // Add component to Main Camera to render Mirrors
    Mirror[] mirrors;

    void Awake()
    {
        mirrors = FindObjectsOfType<Mirror>();
    }

    void OnPreCull()
    {
        for (int i = 0; i < mirrors.Length; i++)
        {
            mirrors[i].Render();
        }
    }
}
