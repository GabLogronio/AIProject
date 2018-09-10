using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public Vector3 StartingPosition;
    public Vector3 ReachingPosition;
    private float timer = 0f;
    public float StayingTime = 2f;
    public float Speed;
    private bool BackAndForth = true;
    private bool StayingStill = false;

    // Use this for initialization
    void Start()
    {

        transform.position = StartingPosition;

    }

    // Update is called once per frame
    void Update()
    {

        if (timer >= StayingTime)
        {
            if (BackAndForth)
            {
                if (transform.position != ReachingPosition)
                    transform.position = Vector3.MoveTowards(transform.position, ReachingPosition, Speed * Time.deltaTime);
                else
                {
                    BackAndForth = false;
                    timer = 0f;
                }
            }

            else
            {
                if (transform.position != StartingPosition)
                    transform.position = Vector3.MoveTowards(transform.position, StartingPosition, Speed * Time.deltaTime);
                else
                {
                    BackAndForth = true;
                    timer = 0f;
                }
            }
        }
        else
        {
            timer += Time.deltaTime;
        }

    }
}
