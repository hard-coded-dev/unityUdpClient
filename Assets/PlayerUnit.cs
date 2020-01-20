using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : MonoBehaviour
{
    public string id;
    Material material;

    public void Awake()
    {
        var cubeRenderer = GetComponentInChildren<Renderer>();
        material = cubeRenderer.material;
    }

    public void SetColor( Color color )
    {
        material.SetColor( "_Color", color );
    }
}
