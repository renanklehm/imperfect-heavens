using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineMesh))]
public class TEST : MonoBehaviour
{
    private LineMesh lineMesh;

    void Update()
    {
        lineMesh = GetComponent<LineMesh>();

        List<List<Vector3>> spiral = new List<List<Vector3>>();
        spiral.Add(new List<Vector3>());

        spiral[0].Add(new Vector3(0f, 0f, 0f));
        spiral[0].Add(new Vector3(0f, 10f, 0f));
        spiral[0].Add(new Vector3(10f, 10f, 0f));
        spiral[0].Add(new Vector3(10f, -100f, 1000f));
        spiral[0].Add(new Vector3(-100f, -100f, 1000f));

        lineMesh.SetLinesFromPoints(spiral);
    }

}