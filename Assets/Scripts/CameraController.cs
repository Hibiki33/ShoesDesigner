using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ShoesDesigner
{
    public class CameraController : MonoBehaviour
    {
        [Range(0.0f, 10.0f)]
        public float moveSpeed = 5.0f;
        [Range(0.0f, 200.0f)]
        public float rotationSpeed = 100.0f;

        private Vector2 prevMousePosition;

        private void Start()
        {

        }


        private void Update()
        {
            ProcessInput();
        }

        private void ProcessInput()
        {
            HandleMovement();
            HandleRotation();
        }

        private void HandleMovement()
        {
            float moveSpeedAdjusted = moveSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(Vector3.forward * moveSpeedAdjusted);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(Vector3.back * moveSpeedAdjusted);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(Vector3.left * moveSpeedAdjusted);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(Vector3.right * moveSpeedAdjusted);
            }
        }

        private void HandleRotation()
        {
            if (Input.GetMouseButtonDown(1))
            {
                prevMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButton(1))
            {
                Vector2 delta = (Vector2)Input.mousePosition - prevMousePosition;
                transform.Rotate(Vector3.up, delta.x * rotationSpeed * Time.deltaTime, Space.World);
                transform.Rotate(Vector3.right, -delta.y * rotationSpeed * Time.deltaTime, Space.Self);
                prevMousePosition = Input.mousePosition;
            }
        }
    }
}
