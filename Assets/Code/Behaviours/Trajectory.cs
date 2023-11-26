using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Trajectory : NetworkBehaviour
{
    public StateVector newestStateVector;
    public Queue<StateVector> stateVectorQueue;
    public Queue<GameObject> arrowsQueue;

    public GameObject arrowMarker;
    public LineRenderer lineRenderer;
    public int maxColorSamples = 8;
    public int plotResolution;
    public float arrowApparentSize;
    public float maxSize;
    public Color lowAcceleration = Color.green;
    public Color highAcceleration = Color.red;

    public bool isRedrawing;
    public bool needRedraw;

    void Awake()
    {
        stateVectorQueue = new Queue<StateVector>();
        arrowsQueue = new Queue<GameObject>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void FixedUpdate()
    {
        if (!isRedrawing)
        {
            if (needRedraw || lineRenderer.positionCount == 0)
            {
                StartCoroutine(RedrawTrajectoryAsync());
            }
        }
    }

    public void ClearQueue()
    {
        stateVectorQueue.Clear();
        lineRenderer.positionCount = 0;
    }

    public void Enqueue(StateVector newStateVector)
    {
        stateVectorQueue.Enqueue(newStateVector);
        newestStateVector = newStateVector;
        StartCoroutine(RedrawTrajectoryAsync());
    }

    public StateVector Dequeue()
    {
        StateVector returnVector = stateVectorQueue.Dequeue();
        if (arrowsQueue.Count > 0) Destroy(arrowsQueue.Dequeue());
        Vector3[] oldPositions = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(oldPositions);
        lineRenderer.SetPositions(oldPositions.Skip(1).ToArray());
        return returnVector;
    }

    IEnumerator RedrawTrajectoryAsync()
    {
        isRedrawing = true;
        StateVector[] stateVectorArray = stateVectorQueue.ToArray();
        int maxPoints = stateVectorArray.Length;

        foreach (GameObject _ in arrowsQueue.ToArray()) Destroy(arrowsQueue.Dequeue());

        lineRenderer.positionCount = maxPoints;
        Vector3[] positions = new Vector3[maxPoints];
        int index = 0;
        int loopCounter = 0;
        foreach (StateVector stateVector in stateVectorArray)
        {
            positions[index] = stateVector.position;
            if (stateVector.activeForce.magnitude > 0)
            {
                GameObject newArrow = Instantiate(arrowMarker, stateVector.position, Quaternion.LookRotation(stateVector.activeForce.normalized, Vector3.up));
                newArrow.transform.localScale = new Vector3(arrowApparentSize, arrowApparentSize, arrowApparentSize);
                arrowsQueue.Enqueue(newArrow);
            }

            if (loopCounter >= Constants.COROUTINE_LOOP_BATCHSIZE)
            {
                loopCounter = 0;
                yield return new WaitForEndOfFrame();
            }

            index++;
            loopCounter++;
        }

        lineRenderer.SetPositions(positions);
        isRedrawing = false;
        needRedraw = false;
        maxSize = lineRenderer.positionCount;
    }

    public bool IsEmpty()
    {
        return (stateVectorQueue.Count == 0 ? true : false);
    }

    public StateVector Peek(float timestamp = 0f)
    {
        StateVector newStateVector = new StateVector();

        if (timestamp == 0f)
        {
            newStateVector = stateVectorQueue.Peek();
        }
        else
        {
            StateVector[] stateVectorArray = stateVectorQueue.ToArray();
            for (int i = 0; i < stateVectorArray.Length; i++)
            {
                if (stateVectorArray[i].timestamp >= timestamp)
                {
                    float oldTimestamp = stateVectorArray[i].timestamp;
                    float newTimestamp = stateVectorArray[i + 1].timestamp;
                    float currentTimestamp = GravityManager.Instance.timestamp - oldTimestamp;
                    newStateVector = StateVector.LerpVector(stateVectorArray[i], stateVectorArray[i + 1], currentTimestamp / newTimestamp);
                    break;
                }
            }
        }


        return newStateVector;
    }

    public float[] GetAccelerationsAsFloat()
    {
        float[] result = new float[stateVectorQueue.Count];
        StateVector[] tempArray = stateVectorQueue.ToArray();
        for (int i = 0; i < stateVectorQueue.Count; i++)
        {
            result[i] = tempArray[i].acceleration.magnitude;
        }
        return result;
    }
}
