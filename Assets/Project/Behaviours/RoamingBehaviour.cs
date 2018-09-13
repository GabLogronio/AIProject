using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RoamingBehaviour : GeneralBehaviour {

    private Vector3 CurrentDirection;
    private Vector3 LastRoosterPosition;
    private NavMeshAgent agent;
    private float TimeToChange = 0f;
    private float StopTime = 0.75f;
    private float timer = 0f;
    bool reset = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        CurrentDirection = transform.position;
        LastRoosterPosition = transform.position;
        TimeToChange = Random.Range(1.5f, 2.5f);
        StopTime = Random.Range(0.5f, 0.75f);
    }

    override public void ExecuteBehaviour(GameObject target)
    {
        Vector3 RoosterMovement = target.transform.position - LastRoosterPosition;
        timer += Time.deltaTime;

        if (RoosterMovement.magnitude <= 0.001f)
        {
            if (timer >= TimeToChange || reset)
            {
                do
                {
                    float RandomX = Random.Range(0.5f, 7.5f);
                    if (RandomX > 4f) RandomX -= 8;
                    float RandomZ = Random.Range(0.5f, 7.5f);
                    if (RandomZ > 4f) RandomZ -= 8;
                    CurrentDirection = new Vector3(target.transform.position.x + RandomX, target.transform.position.y, target.transform.position.z + RandomZ);

                } while (Vector3.Distance(CurrentDirection, target.transform.position) > 4f);
                timer = 0;
            }
            else if (timer > (TimeToChange - StopTime) && timer < TimeToChange) CurrentDirection = transform.position;

            agent.speed = 2f;
        }

        else
        {
            agent.speed = RoosterMovement.magnitude / Time.deltaTime;
            CurrentDirection = CurrentDirection + (RoosterMovement);
        }

        Debug.DrawLine(transform.position, CurrentDirection, Color.green);
        agent.SetDestination(CurrentDirection);
        LastRoosterPosition = target.transform.position;
        reset = false;

    }//--------------------VERSIONE CAMBIA DIREZIONE OGNI TOT SECONDI

    public void ResetDirection()
    {
        reset = true;
    }
}
