using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FleeBehaviour : GeneralBehaviour
{
    override public void ExecuteBehaviour(GameObject target)
    {
        Vector3 EscapeDirection = transform.position - target.transform.position;
        //AddForce opposite to the players direction
        //agent.SetDestination(transform.position + (EscapeDirection.normalized) / 3f);
    }
}
