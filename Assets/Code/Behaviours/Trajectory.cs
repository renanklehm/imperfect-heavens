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
        arrowsQueue = new Queue<GameObject>();
        lineRenderer = GetComponent<LineMesh>();
        marker = Instantiate(trajectoryMarkerPrefab).GetComponent<MarkerBehaviour>();
    }

    private void Update()
    {
        if (!isRedrawing && needRedraw) StartCoroutine(DrawTrajectoryAsync());

        float minDistance = float.PositiveInfinity;
        StateVector selectedStateVector = new StateVector();
        Vector2 mousePosition = Input.mousePosition;
        List<List<Vector3>> worldPositions = lineRenderer.Positions;

        for (int i = 0; i < worldPositions[0].Count - 1; i++)
        {
            Vector2 startPoint = Camera.main.WorldToScreenPoint(worldPositions[0][i]);
            Vector2 endPoint = Camera.main.WorldToScreenPoint(worldPositions[0][i + 1]);
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


        worldPositions[0][0] = body.transform.position;
        if (worldPositions[0].Count < stateVectorQueue.Count)
        {
            worldPositions[0][worldPositions[0].Count - 1] = body.transform.position;
        }

        lineRenderer.SetLinesFromPoints(worldPositions);
    }

    public void ClearQueue()
    {
        stateVectorQueue.Clear();
    }

    public void Enqueue(StateVector newStateVector)
    {
        stateVectorQueue.Enqueue(newStateVector);
        newestStateVector = newStateVector;
        StartCoroutine(DrawTrajectoryAsync());
    }

    public StateVector Dequeue()
    {
        StateVector returnVector = stateVectorQueue.Dequeue();
        if (arrowsQueue.Count > 0) Destroy(arrowsQueue.Dequeue());
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
        StateVector[] stateVectorArray = stateVectorQueue.ToArray();
        maxSize = stateVectorArray.Length;

        foreach (GameObject _ in arrowsQueue.ToArray()) Destroy(arrowsQueue.Dequeue());

        List<List<Vector3>> positions = new List<List<Vector3>>();
        positions.Add(new List<Vector3>());

        int index = 0;
        int loopCounter = 0;
        bool breakLoop = false;
        foreach (StateVector stateVector in stateVectorArray)
        {
            if (breakLoop)
            {
                break;
            }

            if (Vector3.Distance(stateVector.position, stateVectorArray[0].position) <= 0.5f && index != 0)
            {
                breakLoop = true;
            }

            positions[0].Add(stateVector.position - transform.position);
            if (stateVector.activeForce.magnitude > 0)
            {
                GameObject newArrow = Instantiate(arrowMarkerPrefab, stateVector.position, Quaternion.LookRotation(stateVector.activeForce.normalized, Vector3.up));
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

        lineRenderer.SetLinesFromPoints(positions);
        isRedrawing = false;
        needRedraw = false;
    }
}
