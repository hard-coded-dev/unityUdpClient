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
    public float angularSpeed = 60.0f;

    public Transform delayedTransform;

    public void Awake()
    {
        var cubeRenderer = GetComponentInChildren<Renderer>();
        material = cubeRenderer.material;
    }

    public void Update()
    {
        if( isLocalPlayer )
        {
            Transform curTransform = transform;
            if( Input.GetKey( KeyCode.W ) )
            {
                curTransform.position += curTransform.forward * Time.deltaTime * moveSpeed;
            }
            if( Input.GetKey( KeyCode.S ) )
            {
                curTransform.position -= curTransform.forward * Time.deltaTime * moveSpeed;
            }
            if( Input.GetKey( KeyCode.A ) )
            {
                curTransform.position -= curTransform.right * Time.deltaTime * moveSpeed;
            }
            if( Input.GetKey( KeyCode.D ) )
            {
                curTransform.position += curTransform.right * Time.deltaTime * moveSpeed;
            }

            // mouse right drag
            if( Input.GetMouseButton( 1 ) )
            {
                float rotation = Input.GetAxis( "Mouse X" ) * angularSpeed;
                curTransform.Rotate( Vector3.up, rotation );
            }

            if( CanvasManager.Instance.prediction.isOn )
            {
                StartCoroutine( UpdateTransform( curTransform, NetworkMan.Instance.estimatedLag ) );
            }
            else
            {
                transform.position = curTransform.position;
                transform.rotation = curTransform.rotation;
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

    IEnumerator UpdateTransform( Transform newTransform, float waittingTime )
    {
        yield return new WaitForSeconds( waittingTime );
        transform.position = newTransform.position;
        transform.rotation = newTransform.rotation;
    }

    public void SetColor( Color color )
    {
        material.SetColor( "_Color", color );
    }
}
