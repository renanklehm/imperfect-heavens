using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

[RequireComponent(typeof(FreeBody))]
public class ShipController : NetworkBehaviour
{
    public FuelTank[] fuelTanks;
    public Engine[] engines;

    [HideInInspector]
    public Body body;

    private void Start()
    {
        body = GetComponent<Body>();
        foreach (FuelTank tank in fuelTanks)
        {
            foreach (Fuel fuel in Resources.LoadAll<Fuel>("ScriptableObjects/Fuel"))
            {
                if (tank.fuelType == fuel.fuelType) tank.InitializeTank(fuel, 1);
            }
            if (!tank.isInitialized) Debug.LogError("Fuel tank not initialized");
        }

        foreach (Engine engine in engines)
        {
            foreach (FuelTank tank in fuelTanks)
            {
                if (engine.fuelType == tank.fuelType) engine.InitializeEngine(tank);
            }
            if (!engine.isInitialized) Debug.LogError("Engine fuel tank not found");
        }
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
        }
    }

    public float GerBurnDuration(float throttle, float deltaV)
    {
        float totalThrust = 0;
        foreach (Engine engine in engines)
        {
            totalThrust += engine.FireEngine(throttle, false);
        }
        float acceleration = totalThrust / body.mass;
        return deltaV / acceleration;
    }
}
