using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace ShoesDesigner
{
    public class SeniorPen : MonoBehaviour
    {

        private void Start()
        {
            
        }

        private void Update()
        {
            UpdateTransform();
        }

        private void UpdateTransform()
        {
            var camera = Camera.main;
            var cameraPosition = camera.transform.position;
            var cameraForward = camera.transform.forward;

            // Calculate the point on the plane in front of the camera
            var pointOnPlane = cameraPosition + cameraForward * 0.5f;

            // Convert the mouse position to world coordinates on the plane
            var mousePosition = Input.mousePosition;
            var ray = camera.ScreenPointToRay(mousePosition);
            var plane = new Plane(cameraForward, pointOnPlane);

            if (plane.Raycast(ray, out var enter))
            {
                var hitPoint = ray.GetPoint(enter);

                // Set the pen's position
                transform.position = hitPoint;

                // Set the pen's rotation to look at the (0,0,0) point
                var directionToOrigin = Vector3.zero - transform.position;
                transform.rotation = Quaternion.LookRotation(-directionToOrigin);
            }   
        }

        public Vector3 GetNibGlobal()
        {
            return transform.Find("Nib").position;
        }
    }
    
}


