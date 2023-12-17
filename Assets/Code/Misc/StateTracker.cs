using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StateTracker
{
    private bool[] stateHistory;

    public StateTracker()
    {
        stateHistory = new bool[2];
    }

    public void SetState(bool state)
    {
        stateHistory[1] = stateHistory[0];
        stateHistory[0] = state;
    }

    public bool IsUp()
    {
        return stateHistory[0] && !stateHistory[1];
    }

    public bool IsDown()
    {
        return !stateHistory[0] && stateHistory[1];
    }

    public bool IsOn()
    {
        return stateHistory[0];
    }
}
