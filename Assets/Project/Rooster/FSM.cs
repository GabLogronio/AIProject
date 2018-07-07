using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate bool FSMCondition();
public delegate void FSMAction();

public class FSMTransition
{
    public FSMCondition condition;

    private List<FSMAction> actions = new List<FSMAction>();

    public FSMTransition(FSMCondition condition, FSMAction[] actions = null)
    {
        this.condition = condition;
        if (actions != null) this.actions.AddRange(actions);
    }

    public void Fire()
    {
        if (actions != null)
            foreach (FSMAction a in actions)
                a();
    }
}

public class FSMState
{
    public List<FSMAction> enterActions = new List<FSMAction>();
    public List<FSMAction> stayActions = new List<FSMAction>();
    public List<FSMAction> exitActions = new List<FSMAction>();

    private Dictionary<FSMTransition, FSMState> links;

    public FSMState()
    {
        links = new Dictionary<FSMTransition, FSMState>();
    }

    public void AddTransition(FSMTransition transition, FSMState target)
    {
        links[transition] = target;
    }

    public FSMTransition VerifyTransitions()
    {
        foreach (FSMTransition t in links.Keys)
            if (t.condition())
                return t;

        return null;
    }

    public FSMState NextState(FSMTransition t)
    {
        return links[t];
    }

    public void Enter()
    {
        foreach (FSMAction a in enterActions)
            a();
    }

    public void Stay()
    {
        foreach (FSMAction a in stayActions)
            a();
    }

    public void Exit()
    {
        foreach (FSMAction a in exitActions)
            a();
    }
}

public class FSM
{

    public FSMState current;

    public FSM(FSMState state)
    {
        current = state;
        current.Enter();
    }


    public void Update()
    {
        FSMTransition transition = current.VerifyTransitions();
        if (transition != null)
        {
            current.Exit();
            transition.Fire();
            current = current.NextState(transition);
            Debug.Log(current);
            current.Enter();
        }
        else
        {
            current.Stay();
        }
    }
}