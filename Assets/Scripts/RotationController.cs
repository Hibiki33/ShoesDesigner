using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace ShoesDesigner
{
    public class RotationController : MonoBehaviour
    {
        public GameObject rotationPoint;
        public Hand rightHand;

        public float rotationSpeed = 20f;

        void Start()
        {

        }

        void Update()
        {
            var rotationTrigger = rightHand.rotationTrigger;

            if (rotationTrigger.GetStateDown(SteamVR_Input_Sources.RightHand))
            {

            }

            if (rotationTrigger.GetState(SteamVR_Input_Sources.RightHand))
            {
                transform.RotateAround(rotationPoint.transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
            }
        }
    }
}