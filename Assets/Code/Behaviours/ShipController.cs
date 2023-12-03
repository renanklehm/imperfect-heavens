using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

[RequireComponent(typeof(FreeBody))]
public class ShipController : NetworkBehaviour
{
    private Body body;

    private void Start()
    {
        body = GetComponent<Body>();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_AddManeuver(MotionVector burnDirection, float burnStrength, float burnDuration, PlayerRef playerRef)
    {
        if (body.Object.InputAuthority == playerRef)
        {
            Vector3 direction;

            switch (burnDirection)
            {
                case MotionVector.Prograde:
                    direction = transform.forward;
                    break;
                case MotionVector.Retrograde:
                    direction = -transform.forward;
                    break;
                case MotionVector.Normal:
                    direction = transform.up;
                    break;
                case MotionVector.AntiNormal:
                    direction = -transform.up;
                    break;
                case MotionVector.RadialOut:
                    direction = transform.right;
                    break;
                case MotionVector.RadialIn:
                    direction = -transform.right;
                    break;
                default:
                    direction = Vector3.zero;
                    break;
            }

            body.GetComponent<FreeBody>().AddForce(direction * burnStrength, burnDuration);
        }
    }

}
