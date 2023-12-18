using Newtonsoft.Json;
using UnityEngine;
using Fusion;
using System;
using UnityEditor;

[RequireComponent(typeof(FreeBody))]
public class ShipController : NetworkBehaviour
{
    public string shipName;
    [HideInInspector]
    public Body body;
    public Ship ship;

    private void Start()
    {
        TextAsset textAsset = Resources.Load<TextAsset>(shipName);
        if (textAsset != null)
        {
            string jsonContent = textAsset.text;
            ship = JsonConvert.DeserializeObject<Ship>(jsonContent);
        }
        else
        {
            throw new Exception("Ship file not found");
        }

        body = GetComponent<Body>();
        body.mass = ship.mass;
        body.SetName(shipName);
    }

    private void Update()
    {

        body.mass = ship.mass;
    }

    public void AddManeuver(Maneuver newManeuver)
    {
        body.bodySolver.SetManeuver(newManeuver);
    }
}