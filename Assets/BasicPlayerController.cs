using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class BasicPlayerController : MonoBehaviour {

    public Camera cam;
    public LayerMask GroundLayer;

    [Header("Control Parameters")]
    public float JumpHeight;
    public float JumpLength;

    protected Rigidbody rb;
    protected NavMeshAgent agent;
    protected CapsuleCollider CapsuleColl;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        CapsuleColl = GetComponent<CapsuleCollider>();
    }

    void Update()
    {

        if (IsGrounded())
        {

            if (Input.GetMouseButtonDown(0))
            {
                agent.enabled = true;
                Move();

            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
        }

    }

    void Move()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            agent.SetDestination(hit.point);
        }
    }

    void Jump()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            agent.enabled = false;
            Vector3 JumpDirection = new Vector3((hit.point.x - transform.position.x), 0, (hit.point.z - transform.position.z));
            if (JumpDirection.magnitude > JumpLength) JumpDirection = JumpDirection.normalized * JumpLength;
            rb.velocity = new Vector3(JumpDirection.x, JumpHeight, JumpDirection.z);
        }
    }

    bool IsGrounded()
    {
        return Physics.CheckCapsule(CapsuleColl.bounds.center, new Vector3(CapsuleColl.bounds.center.x, CapsuleColl.bounds.min.y, CapsuleColl.bounds.center.z), CapsuleColl.radius * 0.9f, GroundLayer);
    }

}
