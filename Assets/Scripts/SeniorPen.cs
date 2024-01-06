using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace ShoesDesigner
{
    [RequireComponent(typeof(Interactable))]
    public class SeniorPen : MonoBehaviour
    {
        private Interactable interactable;

        private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);

        private Vector3 oldPosition;
        private Quaternion oldRotation;

        private void Start()
        {
            interactable = GetComponent<Interactable>();
        }

        private void Update()
        {
#if ENABLE_VR
#else
            UpdateTransform();
#endif
        }

        private void OnHandHoverBegin(Hand hand)
        {
        }

        private void OnHandHoverEnd(Hand hand)
        {
        }

        private void HandHoverUpdate(Hand hand)
        {
            var startingGrabType = hand.GetGrabStarting();
            bool isGrabEnding = hand.IsGrabEnding(this.gameObject);

            if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
            {
                oldPosition = transform.position;
                oldRotation = transform.rotation;
                hand.HoverLock(interactable);
                hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
            }
            else if (isGrabEnding)
            {
                hand.DetachObject(gameObject);
                hand.HoverUnlock(interactable);
                transform.position = oldPosition;
                transform.rotation = oldRotation;
            }
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


