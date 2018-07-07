using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementGoToDelegate : MonoBehaviour {

    private MovementStatus status;
    public Vector3 destination;

    public float minLinearSpeed = 0.0f;
    public float maxLinearSpeed = 8f;
    public float maxAngularSpeed = 5f;

    //------------------------------------------------------ SEEK PARAMETERS ------------------------------------------------------
    public float gas = 30f;
    public float steer = 40f;
    public float brake = 20f;
    public float brakeAt = 3f;
    public float stopAt = 0.01f;

    //------------------------------------------------------ AVOID PARAMETERS ------------------------------------------------------
    public float sightRange = 8f;
    public float sightAngle = 55f;
    public float backpedal = 10f;

    //------------------------------------------------------ DRAG PARAMETERS ------------------------------------------------------
    public float linearDrag = 3f;
    public float angularDrag = 3f;

    private void Start()
    {
        status = new MovementStatus();
        status.movementDirection = transform.forward;
    }

    void FixedUpdate()
    {
        Vector3 totalAcceleration = Vector3.zero;

        totalAcceleration += SeekAcceleration(status);
        totalAcceleration += AvoidAcceleration(status);
        totalAcceleration += DragAcceleration(status);

        if (totalAcceleration.magnitude != 0f)
        {

            Vector3 tangentComponent = Vector3.Project(totalAcceleration, status.movementDirection);
            Vector3 normalComponent = totalAcceleration - tangentComponent;

            float tangentAcc = tangentComponent.magnitude * Vector3.Dot(tangentComponent.normalized, status.movementDirection);
            Vector3 right = Quaternion.Euler(0f, 90f, 0f) * status.movementDirection.normalized;
            float rotationAcc = normalComponent.magnitude * Vector3.Dot(normalComponent.normalized, right) * 360f;

            float t = Time.deltaTime;

            float tangentDelta = status.linearSpeed * t + 0.5f * tangentAcc * t * t;
            float rotationDelta = status.angularSpeed * t + 0.5f * rotationAcc * t * t;

            status.linearSpeed += tangentAcc * t;
            status.angularSpeed += rotationAcc * t;

            status.linearSpeed = Mathf.Clamp(status.linearSpeed, minLinearSpeed, maxLinearSpeed);
            status.angularSpeed = Mathf.Clamp(status.angularSpeed, -maxAngularSpeed, maxAngularSpeed);

            Rigidbody rb = GetComponent<Rigidbody>();
            rb.MovePosition(rb.position + transform.forward * tangentDelta);
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotationDelta, 0f));

            status.movementDirection = transform.forward;
        }

    }

    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
    }

    public void SetSpeed(float newMaxSpeed)
    {
        maxLinearSpeed = newMaxSpeed;
    }

    private Vector3 SeekAcceleration(MovementStatus status)
    {
        if (destination != Vector3.zero)
        {
            Vector3 verticalAdj = new Vector3(destination.x, transform.position.y, destination.z);
            Vector3 toDestination = (verticalAdj - transform.position);

            if (toDestination.magnitude > stopAt)
            {
                Vector3 tangentComponent = Vector3.Project(toDestination.normalized, status.movementDirection);
                Vector3 normalComponent = (toDestination.normalized - tangentComponent);
                return (tangentComponent * (toDestination.magnitude > brakeAt ? gas : -brake)) + (normalComponent * steer);
            }
            else
            {
                return Vector3.zero;
            }
        }
        else
        {
            return Vector3.zero;
        }
    }

    private Vector3 AvoidAcceleration(MovementStatus status)
    {

        CapsuleCollider coll = GetComponent<CapsuleCollider>();

        bool leftHit = Physics.CapsuleCast(coll.bounds.center, 
                new Vector3(coll.bounds.center.x, coll.bounds.min.y, coll.bounds.center.z), 
                coll.radius, 
                Quaternion.Euler(0f, -sightAngle, 0f) * status.movementDirection,
                sightRange);

        bool centerHit = Physics.CapsuleCast(coll.bounds.center,
                new Vector3(coll.bounds.center.x, coll.bounds.min.y, coll.bounds.center.z),
                coll.radius,
                status.movementDirection,
                sightRange);
            
        bool rightHit = Physics.CapsuleCast(coll.bounds.center,
                new Vector3(coll.bounds.center.x, coll.bounds.min.y, coll.bounds.center.z),
                coll.radius,
                Quaternion.Euler(0f, sightAngle, 0f) * status.movementDirection,
                sightRange);

        Vector3 right = Quaternion.Euler(0f, 90f, 0f) * status.movementDirection.normalized;

        if (leftHit && !centerHit && !rightHit)
        {
            return right * steer;
        }
        else if (leftHit && centerHit && !rightHit)
        {
            return right * steer * 2f;
        }
        else if (leftHit && centerHit && rightHit)
        {
            return -status.movementDirection.normalized * backpedal;
        }
        else if (!leftHit && centerHit && rightHit)
        {
            return -right * steer * 2f;
        }
        else if (!leftHit && !centerHit && rightHit)
        {
            return -right * steer;
        }
        else if (!leftHit && centerHit && !rightHit)
        {
            return right * steer;
        }

        return Vector3.zero;
    }

    private Vector3 DragAcceleration(MovementStatus status)
    {
        return -(status.movementDirection.normalized * status.linearSpeed / linearDrag)
               - ((Quaternion.Euler(0f, 90f, 0f) * status.movementDirection.normalized) * status.angularSpeed / angularDrag);
    }
}
