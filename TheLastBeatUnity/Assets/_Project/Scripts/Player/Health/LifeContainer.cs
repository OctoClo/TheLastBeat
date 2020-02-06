using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeContainer : MonoBehaviour
{
    [SerializeField]
    GameObject slashAnimation;

    [SerializeField]
    Color hurtColor = Color.red;

    [SerializeField]
    Color healColor = Color.green;

    public enum StateHealthCell
    {
        EMPTY,
        HALF_EMPTY,
        FULL
    }

    StateHealthCell currentState;
    public StateHealthCell CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            ComputeNewState(currentState, value);
            currentState = value;
        }
    }

    void ComputeNewState(StateHealthCell previous , StateHealthCell actual)
    {

    }
}
