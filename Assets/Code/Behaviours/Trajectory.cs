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
    public Queue<StateVector> renderingQueue;
    public Queue<GameObject> arrowsQueue;

    public GameObject arrowMarker;
    public LineRenderer lineRenderer;
    public int maxColorSamples = 8;
    public int plotResolution;
    public float arrowApparentSize;
    public Color lowAcceleration = Color.green;
    public Color highAcceleration = Color.red;

    public bool isRedrawing;
    public bool needRedraw;

    void Awake()
    {
        stateVectorQueue = new Queue<StateVector>();
        renderingQueue = new Queue<StateVector>();
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

    public void Enqueue(StateVector newStateVector, TrajectoryRedrawMode redrawMode = TrajectoryRedrawMode.NoRedraw)
    {
        stateVectorQueue.Enqueue(newStateVector);
        newestStateVector = newStateVector;
        if (redrawMode != TrajectoryRedrawMode.NoRedraw)
        {
            if (redrawMode == TrajectoryRedrawMode.Incremental)
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, newStateVector.position);
            }
            else if (redrawMode == TrajectoryRedrawMode.Update)
            {
                StartCoroutine(RedrawTrajectoryAsync());
            }
        }
    }

    public StateVector Dequeue(TrajectoryRedrawMode redrawMode = TrajectoryRedrawMode.Decremental)
    {
        if (redrawMode != TrajectoryRedrawMode.NoRedraw)
        {
            if (redrawMode == TrajectoryRedrawMode.Decremental && renderingQueue.Count > 0 && renderingQueue.Peek().timestamp <= stateVectorQueue.Peek().timestamp)
            {
                Vector3[] tempArray = new Vector3[lineRenderer.positionCount];
                lineRenderer.GetPositions(tempArray);
                lineRenderer.SetPositions(tempArray.Skip(1).ToArray());
                renderingQueue.Dequeue();
                if (arrowsQueue.TryDequeue(out GameObject arrow))
                {
                    Destroy(arrow);
                }
            }
        }
        return stateVectorQueue.Dequeue();
    }

    IEnumerator RedrawTrajectoryAsync()
    {
        isRedrawing = true;
        StateVector[] stateVectorArray = stateVectorQueue.ToArray();
        int maxPoints = stateVectorArray.Length;
        int[] positionSampleIndices = SampleIndices(maxPoints, plotResolution);

        lineRenderer.positionCount = positionSampleIndices.Length;
        int realIndex = 0;
        int lineRendererIndex = 0;
        int loopCounter = 0;
        foreach(StateVector stateVector in stateVectorArray)
        {
            if (positionSampleIndices.Contains(realIndex))
            {
                lineRenderer.SetPosition(lineRendererIndex, stateVector.position);
                renderingQueue.Enqueue(stateVector);
                if (stateVector.activeForce.magnitude > 0)
                {
                    GameObject newArrow = Instantiate(arrowMarker, stateVector.position, Quaternion.LookRotation(stateVector.activeForce.normalized, Vector3.up));
                    newArrow.transform.localScale = new Vector3(arrowApparentSize, arrowApparentSize, arrowApparentSize);
                    arrowsQueue.Enqueue(newArrow);
                }
                lineRendererIndex++;
            }
            if (loopCounter >= Constants.COROUTINE_LOOP_BATCHSIZE / 10f)
            {
                loopCounter = 0;
                yield return new WaitForEndOfFrame();
            }

            realIndex++;
            loopCounter++;
        }

        isRedrawing = false;
        needRedraw = false;
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
                    float currentTimestamp = GravityManager.Instance.timeStamp - oldTimestamp;
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

    private int[] SampleIndices(int totalPoints, int numSamples)
    {
        List<int> sampledIndices = new List<int>();
        float step = (float)(totalPoints - 1) / (numSamples - 1);

        for (int i = 0; i < numSamples; i++)
        {
            int index = Mathf.RoundToInt(i * step);
            sampledIndices.Add(index);
        }

        return sampledIndices.ToArray();
    }
}
