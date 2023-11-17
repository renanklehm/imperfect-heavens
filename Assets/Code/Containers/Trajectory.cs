using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory
{
    public Queue<StateVector> stateVector;
    public StateVector newestStateVector;

    public Trajectory (StateVector initialStateVector, int _nSteps)
    {
        stateVector = new Queue<StateVector>(_nSteps);
        stateVector.Enqueue(initialStateVector);
    }

    public void Enqueue(StateVector newStateVector)
    {
        stateVector.Enqueue(newStateVector);
        newestStateVector = newStateVector;
    }

    public Trajectory Copy()
    {
        StateVector[] tempStateVectors = stateVector.ToArray();
        Trajectory newCopy = new Trajectory(stateVector.Peek(), stateVector.Count);

        for (int i = 1; i < tempStateVectors.Length; i++)
        {
            newCopy.Enqueue(tempStateVectors[i]);
        }

        return newCopy;
    }

    public StateVector Dequeue()
    {
        return stateVector.Dequeue();
    }

    public bool IsEmpty()
    {
        return (stateVector.Count == 0 ? true : false);
    }

    public StateVector Peek()
    {
        return stateVector.Peek();
    }

    public Vector3[] GetPositions()
    {
        Vector3[] result = new Vector3[stateVector.Count];
        StateVector[] tempArray = stateVector.ToArray();
        for (int i = 0; i < stateVector.Count; i++)
        {
            result[i] = tempArray[i].position;
        }
        return result;
    }

    public Vector3[] GetPositionsEqualTime(float timeInterval)
    {
        List<Vector3> result = new List<Vector3>();
        StateVector[] stateArray = stateVector.ToArray();
        float currentTime = stateArray[0].timestamp;

        for (int i = 1; i < stateArray.Length; i++)
        {
            if (stateArray[i].timestamp - currentTime >= timeInterval)
            {
                result.Add(stateArray[i].position);
                currentTime = stateArray[i].timestamp;
            }
        }

        return result.ToArray();
    }

    public Vector3[] GetAccelerations()
    {
        Vector3[] result = new Vector3[stateVector.Count];
        StateVector[] tempArray = stateVector.ToArray();
        for (int i = 0; i < stateVector.Count; i++)
        {
            result[i] = tempArray[i].acceleration;
        }
        return result;
    }

    public float[] GetAccelerationsAsFloat()
    {
        float[] result = new float[stateVector.Count];
        StateVector[] tempArray = stateVector.ToArray();
        for (int i = 0; i < stateVector.Count; i++)
        {
            result[i] = tempArray[i].acceleration.magnitude;
        }
        return result;
    }
}
