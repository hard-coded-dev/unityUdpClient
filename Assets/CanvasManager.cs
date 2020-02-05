using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public Toggle prediction;
    public Toggle reconciliation;
    public Toggle interpolation;
    public static CanvasManager Instance { get; private set; } = null;

    private void Awake()
    {
        if( Instance != null && Instance != this )
            Destroy( gameObject );
        else
            Instance = this;
    }
}
