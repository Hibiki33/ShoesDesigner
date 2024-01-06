using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace ShoesDesigner
{
    public class StepManager : MonoBehaviour
    {
        public static StepManager instance;

        private Step step = Step.PREPARE;

        public bool VRMode = true;

        [Space]
        public Hand rightHand;

        [Space]
        public GameObject[] shoesParts;

        [Header("Scene Objects")]
        public GameObject prepareHint;
        public GameObject generateHint;
        public GameObject penEditor;
        public GameObject brushSet;
        public GameObject fluidSolver;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                step = Step.PREPARE;
            }
            else
            {
                Destroy(gameObject);
            }
        }


        private void Start()
        {

        }

        private void Update()
        {
            UpdateStep();
        }


        public enum Step : ushort
        {
            PREPARE = 0,
            GENERATE,
            BIGEDIT,
            SMALLEDIT,
            PAINT,
            SIMULATE,
        }

        public Step GetCurrentStep()
        {
            return step;
        }

        private void UpdateStep()
        {
            var nextTrigger = rightHand.nextTrigger;
            if (nextTrigger.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                SetNextStep();
            }

            var previousTrigger = rightHand.previousTrigger;
            if (previousTrigger.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                SetPreviousStep();
            }
        }

        public void SetNextStep()
        {
            switch (step)
            {
                case Step.PREPARE:
                    prepareHint.SetActive(false);
                    generateHint.SetActive(true);
                    break;
                case Step.GENERATE:
                    penEditor.SetActive(true);
                    foreach (var part in shoesParts)
                    {
                        part.GetComponent<MeshEditor>().SetTransparentMaterial();
                    }
                    break;
                case Step.BIGEDIT:
                    foreach (var part in shoesParts)
                    {
                        part.GetComponent<MeshEditor>().ResetMaterial();
                    }
                    break;
                case Step.SMALLEDIT:
                    penEditor.SetActive(false);
                    brushSet.SetActive(true);
                    foreach (var part in shoesParts)
                    {
                        var meshRenderer = part.GetComponent<MeshEditor>();
                        if (meshRenderer.editable)
                        {
                            meshRenderer.SetEditableMaterial();
                        }
                    }
                    break;
                case Step.PAINT:
                    brushSet.SetActive(false);
                    fluidSolver.SetActive(true);
                    break;
                case Step.SIMULATE:
                    break;
                default:
                    break;
            }
            step = step != Step.SIMULATE ? step + 1 : step;
        }

        public void SetPreviousStep()
        {
            switch (step)
            {
                case Step.PREPARE:
                    break;
                case Step.GENERATE:
                    prepareHint.SetActive(true);
                    generateHint.SetActive(false);
                    break;
                case Step.BIGEDIT:
                    penEditor.SetActive(false);
                    break;
                case Step.SMALLEDIT:

                    break;
                case Step.PAINT:

                    break;
                case Step.SIMULATE:

                    break;
                default:
                    break;
            }
            step = step != Step.PREPARE ? step - 1 : step; // forbid to go back to PREPARE
        }
    }
}
