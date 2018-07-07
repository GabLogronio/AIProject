using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmBehaviour : GeneralBehaviour
{
    public GameObject Rooster;
    private float timer = ChicksParametersManager.AlarmTime;

    override public void ExecuteBehaviour(GameObject target)
    {
        if (timer < ChicksParametersManager.AlarmTime) timer += Time.deltaTime;
        if (timer >= ChicksParametersManager.AlarmTime)
        {
            Rooster.GetComponent<RoosterBehaviour>().ActivateAlarm(target);
            timer = 0;
        }

    }
}
