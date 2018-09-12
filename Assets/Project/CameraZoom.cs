using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour {

    public float zoomSpeed;

    public float CurrentZoom = 0f;

    // Update is called once per frame
    void Update() {

        if ((Input.GetAxis("Mouse ScrollWheel") > 0 && CurrentZoom < 3f) ||
            (Input.GetAxis("Mouse ScrollWheel") < 0 && CurrentZoom > 0f))
        {
            Zoom(Input.GetAxis("Mouse ScrollWheel"));
        }
    }

    private void Zoom(float zoom)
    {
        transform.Translate(Vector3.forward * zoomSpeed * zoom * Time.deltaTime);
        CurrentZoom += zoom;
    }
}
