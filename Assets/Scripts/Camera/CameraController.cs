using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float keyboardInputSensitivity = 10f;
    [SerializeField] float mouseInputSensitivity = 10f;
    [SerializeField] bool mouseInputContinious = false;
    [SerializeField] bool mouseInputInverse = false;
    [SerializeField] Transform bottomLeftBorder;
    [SerializeField] Transform topRightBorder;

    Vector3 input;
    Vector3 pointOfOrigin;

    private void Update()
    {
        ResetInput();
        MoveCameraInput();
        MoveCamera();
    }

    private void ResetInput()
    {
        input = Vector3.zero;
    }

    private void MoveCamera()
    {
        Vector3 pos = transform.position;
        pos += input * Time.deltaTime;
        pos.x = Mathf.Clamp(pos.x, bottomLeftBorder.position.x, topRightBorder.position.x);
        pos.z = Mathf.Clamp(pos.z, bottomLeftBorder.position.z, topRightBorder.position.z);
        transform.position = pos;
    }

    private void MoveCameraInput()
    {
        AxisInput();
        MouseInput();
    }

    private void AxisInput()
    {
        input.x += Input.GetAxisRaw("Horizontal") * keyboardInputSensitivity;
        input.z += Input.GetAxisRaw("Vertical") * keyboardInputSensitivity;

    }

    private void MouseInput()
    { 
        if (Input.GetMouseButtonDown(0))
            pointOfOrigin = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector3 mouseInput = Input.mousePosition;
            float inverseScale = mouseInputInverse ? -1f : 1f;
            input.x += (mouseInput.x - pointOfOrigin.x) * mouseInputSensitivity * inverseScale;
            input.z += (mouseInput.y - pointOfOrigin.y) * mouseInputSensitivity * inverseScale;
            

            if (mouseInputContinious == false)
                pointOfOrigin = mouseInput;
        }
            
    }
}
