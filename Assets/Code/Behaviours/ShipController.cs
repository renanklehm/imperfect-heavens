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
        body.mass = 0;
        
        foreach (FuelTank tank in fuelTanks)
        {
            tank.InitializeTank(1);
            body.mass += tank.currentFuelMass;
            body.mass += tank.dryMass;
        }

        foreach (Engine engine in engines)
        {
            engine.InitializeEngine();
            body.mass += engine.dryMass;
        }
    }

    private void FixedUpdate()
    {
 
    }

    public float GetCurrentFuelMass()
    {
        float totalFuelMass = 0;
        foreach (FuelTank tank in fuelTanks)
        {
            totalFuelMass += tank.currentFuelMass;
        }

        return totalFuelMass;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_AddManeuver(PlayerRef playerRef)
    {
        
    }

    public BurnData SimulateBurn(Vector3 direction, float startTime, float throttle, float deltaV)
    {
        return new BurnData(direction, startTime, throttle, deltaV, engines, fuelTanks);
    }
}
