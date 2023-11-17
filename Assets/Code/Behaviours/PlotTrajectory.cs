using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlotTrajectory : MonoBehaviour
{
    public GameObject arrowMarker;
    public LineRenderer lineRenderer;
    public int maxColorSamples = 8;
    public float plotResolution;
    public float arrowApparentSize;
    public Color lowAcceleration = Color.green;
    public Color highAcceleration = Color.red;

    private Queue<GameObject> arrows;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        arrows = new Queue<GameObject>();
    }

    public void DropFirstPoint()
    {
        Vector3[] tempArray = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(tempArray);
        lineRenderer.SetPositions(tempArray.Skip(1).ToArray());
        GameObject arrow;
        if (arrows.TryDequeue(out arrow))
        {
            Destroy(arrow);
        }
    }

    public void UpdateCurrentPosition(Vector3 position)
    {
        lineRenderer.SetPosition(0, position);
    }

    public void DrawTrajectory(Trajectory _trajectory)
    {
        Trajectory trajectory = _trajectory.Copy();
        Vector3[] positions = trajectory.GetPositions();
        float[] accelerations = trajectory.GetAccelerationsAsFloat();

        maxColorSamples = Mathf.Clamp(maxColorSamples, 2, positions.Length);
        int[] colorSampleIndices = SampleIndices(positions.Length, maxColorSamples);

        lineRenderer.positionCount = positions.Length;
        for (int i = 0; i < positions.Length; i++)
        {
            StateVector currentVector = trajectory.Dequeue();
            lineRenderer.SetPosition(i, currentVector.position);
            if (currentVector.activeForce.magnitude > 0)
            {
                GameObject newArrow = Instantiate(arrowMarker, currentVector.position, Quaternion.LookRotation(currentVector.activeForce.normalized, Vector3.up));
                newArrow.transform.localScale = new Vector3(arrowApparentSize, arrowApparentSize, arrowApparentSize);
                arrows.Enqueue(newArrow);
            }
        }

        GradientColorKey[] colorKeys = new GradientColorKey[maxColorSamples];
        for (int i = 0; i < maxColorSamples; i++)
        {
            int index = colorSampleIndices[i];
            float t = Mathf.InverseLerp(0f, Mathf.Max(accelerations), accelerations[index]);
            colorKeys[i] = new GradientColorKey(Color.Lerp(lowAcceleration, highAcceleration, t), Mathf.InverseLerp(0f, positions.Length - 1, index));
        }

        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, lineRenderer.colorGradient.alphaKeys);
        lineRenderer.colorGradient = gradient;
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
