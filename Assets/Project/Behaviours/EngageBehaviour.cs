using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngageBehaviour : GeneralBehaviour
{
    private float timer = HensParametersManager.AttackTime;
    public float PushForce = 350f;

    override public void ExecuteBehaviour(GameObject target)
    {
        if (timer < HensParametersManager.AttackTime) timer += Time.deltaTime;
        if (timer >= HensParametersManager.AttackTime)
        {
            target.GetComponent<Rigidbody>().AddForce(transform.forward * PushForce, ForceMode.Impulse);
            target.GetComponent<Rigidbody>().AddForce(transform.up * PushForce, ForceMode.Impulse);
            transform.LookAt(target.transform);
            timer = 0;
        }
    }
}
