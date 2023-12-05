using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Trajectory : NetworkBehaviour
{
    public StateVector newestStateVector;
    public Queue<StateVector> stateVectorQueue;
    public Queue<StateVector> maneuverQueue;

    public Body body;
    public GameObject trajectoryMarkerPrefab;
    public GameObject arrowMarkerPrefab;
    public LineMesh lineRenderer;
    public int maxColorSamples = 8;
    public int plotResolution;
    public float arrowApparentSize;
    public float maxSize;
    public Color lowAcceleration = Color.green;
    public Color highAcceleration = Color.red;

    public bool isRedrawing;
    public bool needRedraw;

    private MarkerBehaviour marker;

    void Awake()
    {
        stateVectorQueue = new Queue<StateVector>();
        maneuverQueue = new Queue<StateVector>();
        lineRenderer = GetComponent<LineMesh>();
        marker = Instantiate(trajectoryMarkerPrefab).GetComponent<MarkerBehaviour>();
    }

    private void Start()
    {
        marker.body = body;
    }

    private void Update()
    {
        if (!isRedrawing && needRedraw) StartCoroutine(DrawTrajectoryAsync());

        float minDistance = float.PositiveInfinity;
        StateVector selectedStateVector = new StateVector();
        Vector2 mousePosition = Input.mousePosition;
        List<List<Vector3>> renderedPositions = lineRenderer.Positions;

        if (!GameManager.Instance.isPlanningManeuver && !GameManager.Instance.isRotatingCamera)
        {
            for (int i = 0; i < renderedPositions[0].Count - 1; i++)
            {
                Vector2 startPoint = Camera.main.WorldToScreenPoint(renderedPositions[0][i]);
                Vector2 endPoint = Camera.main.WorldToScreenPoint(renderedPositions[0][i + 1]);
                Vector2 lineVector = endPoint - startPoint;
                Vector2 mouseVector = endPoint - mousePosition;
                float lerpFactor = Mathf.Clamp(Vector2.Dot(mouseVector, lineVector) / Vector2.Dot(lineVector, lineVector), 0f, 1f);
                lerpFactor = 1 - lerpFactor;
                Vector2 _closestPoint = new Vector2(startPoint.x + lerpFactor * (endPoint.x - startPoint.x), startPoint.y + lerpFactor * (endPoint.y - startPoint.y));
                float distance = Vector2.Distance(mousePosition, _closestPoint);
                if (distance <= minDistance)
                {
                    minDistance = distance;
                    selectedStateVector = LerpVector(i, lerpFactor);
                }
            }

            if (minDistance <= Constants.MOUSE_HOVER_SCREEN_DISTANCE)
            {
                marker.isHovering = true;
                marker.UpdateMarker(selectedStateVector, body);
            }
            else
            {
                marker.isHovering = false;
            }
        }

        renderedPositions[0][0] = body.transform.position;
        if (renderedPositions[0].Count < stateVectorQueue.Count)
        {
            renderedPositions[0][renderedPositions[0].Count - 1] = body.transform.position;
        }

        lineRenderer.SetLinesFromPoints(renderedPositions);
    }

    public void SetManeuver()
    {
        StateVector[] stateVectors = stateVectorQueue.ToArray();
        StateVector[] maneuvers = maneuverQueue.ToArray();
        stateVectorQueue.Clear();
        maneuverQueue.Clear();
        var test = new List<List<Vector3>>();
        test.Add(new List<Vector3>());
        test[0].Add(Vector3.zero);
        test[0].Add(Vector3.zero);
        test.Add(new List<Vector3>());
        test[1].Add(Vector3.zero);
        test[1].Add(Vector3.zero);
        lineRenderer.SetLinesFromPoints(test);

        foreach (StateVector originalVector in stateVectors)
        {
            if (originalVector.timestamp < maneuvers[0].timestamp)
            {
                Enqueue(originalVector, false);
            }
            else
            {
                foreach (StateVector newVector in maneuvers)
                {
                    Enqueue(newVector, false);
                }
                break;
            }
        }

        StartCoroutine(DrawTrajectoryAsync());
    }

    public void ClearQueue(bool isManeuver)
    {
        if (isManeuver)
        {
            maneuverQueue.Clear();
        }
        else
        {
            stateVectorQueue.Clear();
        }
    }

    public void Enqueue(StateVector newStateVector, bool isManeuver)
    {
        if (isManeuver)
        {
            maneuverQueue.Enqueue(newStateVector);
        }
        else
        {
            stateVectorQueue.Enqueue(newStateVector);
            newestStateVector = newStateVector;
        }
        needRedraw = true;
    }

    public StateVector Dequeue()
    {
        StateVector returnVector = stateVectorQueue.Dequeue();

        List<List<Vector3>> oldPositions = lineRenderer.Positions;
        if (oldPositions.Count < stateVectorQueue.Count)
        {
            StartCoroutine(DrawTrajectoryAsync());
        }
        else
        {
            oldPositions[0].RemoveAt(0);
            lineRenderer.SetLinesFromPoints(oldPositions);
        }

        return returnVector;
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

    public Vector3[] GetPositions()
    {
        StateVector[] stateVectors = stateVectorQueue.ToArray();
        Vector3[] positions = new Vector3[stateVectors.Length];
        for (int i = 0; i < stateVectors.Length; i++)
        {
            positions[i] = stateVectors[i].position;
        }

        return positions;
    }

    public StateVector LerpVector(int startIndex, float factor)
    {
        StateVector[] stateVectors = stateVectorQueue.ToArray();
        return StateVector.LerpVector(stateVectors[startIndex], stateVectors[startIndex + 1], factor);
    }

    IEnumerator DrawTrajectoryAsync()
    {
        isRedrawing = true;
        List<List<Vector3>> trajectoryPositions = new List<List<Vector3>>();
        trajectoryPositions.Add(new List<Vector3>());

        StateVector[] stateVectorArray = stateVectorQueue.ToArray();
        maxSize = stateVectorArray.Length;
        int index = 0;
        int loopCounter = 0;
        bool breakLoop = false;
        foreach (StateVector stateVector in stateVectorArray)
        {
            if (breakLoop) break;
            if (Vector3.Distance(stateVector.position, stateVectorArray[0].position) <= 0.5f && index != 0) breakLoop = true;

            trajectoryPositions[0].Add(stateVector.position - transform.position);
            if (loopCounter >= Constants.COROUTINE_LOOP_BATCHSIZE)
            {
                loopCounter = 0;
                yield return new WaitForEndOfFrame();
            }

            index++;
            loopCounter++;
        }


        if (maneuverQueue.Count > 0)
        {
            trajectoryPositions.Add(new List<Vector3>());
            stateVectorArray = maneuverQueue.ToArray();
            index = 0;
            loopCounter = 0;
            breakLoop = false;
            foreach (StateVector stateVector in stateVectorArray)
            {
                if (breakLoop) break;
                if (Vector3.Distance(stateVector.position, stateVectorArray[0].position) <= 0.5f && index != 0) breakLoop = true;

                trajectoryPositions[1].Add(stateVector.position - transform.position);
                if (loopCounter >= Constants.COROUTINE_LOOP_BATCHSIZE)
                {
                    loopCounter = 0;
                    yield return new WaitForEndOfFrame();
                }

                index++;
                loopCounter++;
            }
        }

        lineRenderer.SetLinesFromPoints(trajectoryPositions);
        isRedrawing = false;
        needRedraw = false;
    }
}
