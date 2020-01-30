using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUnit : MonoBehaviour
{
    public string id;
    public TextMeshProUGUI clientIdText;
    public bool isLocalPlayer;
    public GameObject cameraSpot;
    Material material;

    public float moveSpeed = 1.0f;
    public float angularSpeed = 40.0f;

    public void Awake()
    {
        var cubeRenderer = GetComponentInChildren<Renderer>();
        material = cubeRenderer.material;
    }

    public void Update()
    {
        if( isLocalPlayer )
        {
            if( Input.GetKey( KeyCode.W ) )
            {
                transform.position += transform.forward * Time.deltaTime * moveSpeed;
            }
            if( Input.GetKey( KeyCode.S ) )
            {
                transform.position -= transform.forward * Time.deltaTime * moveSpeed;
            }
            if( Input.GetKey( KeyCode.A ) )
            {
                transform.position -= transform.right * Time.deltaTime * moveSpeed;
            }
            if( Input.GetKey( KeyCode.D ) )
            {
                transform.position += transform.right * Time.deltaTime * moveSpeed;
            }
            if( Input.GetKey( KeyCode.Q ) )
            {
                transform.Rotate( Vector3.up, -Time.deltaTime * angularSpeed );
            }
            if( Input.GetKey( KeyCode.E ) )
            {
                transform.Rotate( Vector3.up, Time.deltaTime * angularSpeed );
            }
        }
        else
        {
            clientIdText.transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void SetId( string clientId, bool isLocal )
    {
        id = clientId;
        if( clientIdText != null )
        {
            clientIdText.text = id.Split( new char[] { '(', ',', ')' } )[2];
            clientIdText.color = isLocal ? Color.red : Color.gray;
        }

        isLocalPlayer = isLocal;
        if( isLocal )
        {
            Camera.main.transform.parent = cameraSpot.transform;
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;
        }
    }

    public void SetColor( Color color )
    {
        material.SetColor( "_Color", color );
    }
}
