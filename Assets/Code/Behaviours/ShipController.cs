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
    public void RPC_AddManeuver(BurnDirection burnDirection, float burnStrength, float burnDuration, PlayerRef playerRef)
    {
        if (body.Object.InputAuthority == playerRef)
        {
            Vector3 direction;

            switch (burnDirection)
            {
                case BurnDirection.Prograde:
                    direction = transform.forward;
                    break;
                case BurnDirection.Retrograde:
                    direction = -transform.forward;
                    break;
                case BurnDirection.Normal:
                    direction = transform.up;
                    break;
                case BurnDirection.AntiNormal:
                    direction = -transform.up;
                    break;
                case BurnDirection.RadialOut:
                    direction = transform.right;
                    break;
                case BurnDirection.RadialIn:
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
