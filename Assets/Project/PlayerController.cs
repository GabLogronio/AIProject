using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public Camera cam;

    private float InputX;
    private float InputZ;

    private float MovementSpeed = 4.5f;
    private float RotationSpeed = 0.15f;

    //Calculates the movement direction given the input and the camera direction
    protected Vector3 GetDirection()
    {
        var CamForward = cam.transform.forward;
        var CamRight = cam.transform.right;

        CamForward.y = CamRight.y = 0;

        return (CamForward * InputZ + CamRight * InputX).normalized;
    }

    // Update is called once per frame
    void Update () {

        InputX = Input.GetAxisRaw("Horizontal");
        InputZ = Input.GetAxisRaw("Vertical");
        float CurrentInput = Mathf.Sqrt(InputX * InputX + InputZ * InputZ);

        if(GetDirection() != Vector3.zero)
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(GetDirection()), RotationSpeed);
        transform.position += GetDirection() * MovementSpeed * Time.deltaTime;
    }
}
