using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public class Trajectory : NetworkBehaviour
{
    public int maxSize;
    public bool needRedraw;
    public bool isManeuver;

    public int Count { get { return stateVectorQueue.Count; } }

    [HideInInspector]
    public StateVector newestStateVector;
    [HideInInspector]
    public Body body;
    [HideInInspector]
    public LineMesh lineRenderer;

    private Queue<StateVector> stateVectorQueue;
    public StateVector[] stateVectorArray;


    public StateVector this[int index]
    {
        get
        {
            return stateVectorArray[index];
        }
    }

    private void Awake()
    {
        stateVectorQueue = new Queue<StateVector>();
        lineRenderer = GetComponent<LineMesh>();
    }

    public void InitializeTrajectory(Body body)
    {
        this.body = body;
        transform.parent = null;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        DrawTrajectoryAsync();
    }

    public void SetManeuver(StateVector[] maneuvers)
    {
        StateVector[] stateVectors = stateVectorQueue.ToArray();
        ClearQueue();

        foreach (StateVector originalVector in stateVectors)
        {
            if (originalVector.timestamp < maneuvers[0].timestamp)
            {
                Enqueue(originalVector);
            }
            else
            {
                foreach (StateVector newVector in maneuvers)
                {
                    Enqueue(newVector);
                }
                break;
            }
        }

        DrawTrajectoryAsync();
    }

    public void ClearQueue()
    {
        stateVectorQueue.Clear();
        stateVectorArray = null;
    }

    public void Enqueue(StateVector newStateVector)
    {
        stateVectorQueue.Enqueue(newStateVector);
        newestStateVector = newStateVector;
        needRedraw = true;
        if (Count > maxSize) maxSize = Count;
        stateVectorArray = stateVectorQueue.ToArray();
    }

    public StateVector Dequeue()
    {
        StateVector returnVector = stateVectorQueue.Dequeue();

        List<List<Vector3>> oldPositions = lineRenderer.Positions;
        if (oldPositions.Count < stateVectorQueue.Count)
        {
            DrawTrajectoryAsync();
        }
        else
        {
            oldPositions[0].RemoveAt(0);
            lineRenderer.SetLinesFromPoints(oldPositions);
        }

        stateVectorArray = stateVectorQueue.ToArray();
        return returnVector;
    }

    public bool IsEmpty()
    {
        return (stateVectorQueue.Count == 0 ? true : false);
    }

    public StateVector Peek(float timestamp = 0f)
    {
        StateVector returnVector = new StateVector();

        if (timestamp == 0f)
        {
            returnVector = stateVectorQueue.Peek();
        }
        else
        {
            for (int i = 0; i < stateVectorArray.Length; i++)
            {
                if (stateVectorArray[i].timestamp >= timestamp)
                {
                    float oldTimestamp = stateVectorArray[i].timestamp;
                    float newTimestamp = stateVectorArray[i + 1].timestamp;
                    float currentTimestamp = GravityManager.Instance.timestamp - oldTimestamp;
                    returnVector = StateVector.LerpVector(stateVectorArray[i], stateVectorArray[i + 1], currentTimestamp / newTimestamp);
                    break;
                }
            }
        }
        return returnVector;
    }

    public float[] GetAccelerationsAsFloat()
    {
        float[] result = new float[stateVectorArray.Length];
        for (int i = 0; i < stateVectorArray.Length; i++)
        {
            result[i] = stateVectorArray[i].acceleration.magnitude;
        }
        return result;
    }

    public Vector3[] GetPositions()
    {
        Vector3[] positions = new Vector3[stateVectorArray.Length];
        for (int i = 0; i < stateVectorArray.Length; i++)
        {
            positions[i] = stateVectorArray[i].position;
        }

        return positions;
    }

    private void DrawTrajectoryAsync()
    {
        if (stateVectorArray.Length == 0) return;

        needRedraw = false;

        List<List<Vector3>> trajectoryPositions = new List<List<Vector3>> { new List<Vector3>() };
        int index = 0;
        bool breakLoop = false;
        foreach (StateVector stateVector in stateVectorArray)
        {
            if (breakLoop) break;
            if (Vector3.Distance(stateVector.position, stateVectorArray[0].position) <= 0.5f && index > 10) breakLoop = true;

            trajectoryPositions[0].Add(stateVector.position - transform.position);
            index++;
        }

        if (!isManeuver)
        {
            trajectoryPositions[0][0] = body.transform.position;
            if (trajectoryPositions[0].Count < stateVectorQueue.Count)
            {
                trajectoryPositions[0][trajectoryPositions[0].Count - 1] = body.transform.position;
            }
        }

        lineRenderer.SetLinesFromPoints(trajectoryPositions);
    }
}
