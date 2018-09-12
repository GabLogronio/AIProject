using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour {

    private float VerBorderPercentage = 0.1f;
    private float HorBorderPercentage = 0.05f;

    public float CameraSpeed = 10f;

    // Update is called once per frame
    void Update ()
    {

        if (Input.GetKey(KeyCode.Z)) RotateCamera(true);
        else if (Input.GetKey(KeyCode.X)) RotateCamera(false);

        MoveCamera();

    }

    //TO CHANGE: USE EULER ANGLES
    private void RotateCamera(bool direction) 
    {
        if(direction) transform.Rotate(45f * Vector3.up * Time.deltaTime);
        else transform.Rotate(45f * Vector3.up * Time.deltaTime * -1);

    }

    private void MoveCamera()
    {
        //transform.position = ( Light.transform.position + Shadow.transform.position) / 2 ;
        if (Input.mousePosition.y > Screen.height - Screen.height * VerBorderPercentage)
            transform.Translate(Vector3.forward * CameraSpeed * Time.deltaTime);

        if (Input.mousePosition.y < Screen.height * VerBorderPercentage)
            transform.Translate(-Vector3.forward * CameraSpeed * Time.deltaTime);

        if (Input.mousePosition.x > Screen.width - Screen.width * HorBorderPercentage)
            transform.Translate(Vector3.right * CameraSpeed * Time.deltaTime);

        if (Input.mousePosition.x < Screen.width * HorBorderPercentage)
            transform.Translate(-Vector3.right * CameraSpeed * Time.deltaTime);
    }
}
