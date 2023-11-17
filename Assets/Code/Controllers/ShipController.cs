using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FreeBody))]

public class ShipController : MonoBehaviour
{
    public float thrust;
    public FreeBody freeBody;
    public PhysicalBody body;

    private void Start()
    {
        freeBody = GetComponent<FreeBody>();
        body = GetComponent<PhysicalBody>();
    }

    private void Update()
    {
        transform.LookAt(body.currentStateVector.position + body.currentStateVector.velocity);
    }

    public void SetThrust(float _thrust)
    {
        thrust = _thrust;
    }

    public void AddThrust(int _direction)
    {
        Vector3 direction;
        switch (_direction)
        {
            case 1:
                direction = transform.forward;
                break;
            case -1:
                direction = -transform.forward;
                break;
            case 2:
                direction = transform.up;
                break;
            case -2:
                direction = -transform.up;
                break;
            case 3:
                direction = transform.right;
                break;
            case -3:
                direction = -transform.right;
                break;
            default:
                direction = Vector3.zero;
                break;
        }

        freeBody.AddForce(direction * thrust);
    }
}
