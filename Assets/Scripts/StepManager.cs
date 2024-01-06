using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace ShoesDesigner
{
    public class StepManager : MonoBehaviour
    {
        public static StepManager instance;

        private static Step step = Step.PREPARE;

        public static bool VRMode = false;

        private static Dictionary<Step, List<GameObject>> tools;

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

            tools = new Dictionary<Step, List<GameObject>>();
            // Prepare
            var prepTools = new List<GameObject>();
            tools[Step.PREPARE] = prepTools;

            // Generate
            var geneTools = new List<GameObject>();
            tools[Step.GENERATE] = geneTools;

            // Edit
            var editTools = new List<GameObject>();
            var penEditor = GameObject.Find("PenEditor");
            penEditor.SetActive(false);
            editTools.Add(penEditor);
            tools[Step.EDIT] = editTools;

            // Paint
            var painTools = new List<GameObject>();
            var brushSet = GameObject.Find("BrushSet");
            brushSet.SetActive(false);
            painTools.Add(brushSet);
            tools[Step.PAINT] = painTools;

            // Simulate
            var simuTools = new List<GameObject>();
            var fuildSover = GameObject.Find("FuildSolver");
            fuildSover.SetActive(false);
            simuTools.Add(fuildSover);
            tools[Step.SIMULATE] = simuTools;

            // Render
            var rendTools = new List<GameObject>();
            tools[Step.RENDER] = rendTools;
        }


        private void Start()
        {

        }

        private void Update()
        {

        }


        public enum Step : ushort
        {
            PREPARE = 0,
            GENERATE,
            EDIT,
            PAINT,
            SIMULATE,
            RENDER,
        }

        public static Step GetCurrentStep()
        {
            return step;
        }

        public static void SetNextStep()
        {
            if (VRMode)
            {

            }
            else
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    step = step != Step.RENDER ? step + 1 : step;
                    foreach (var tool in tools[step])
                    {
                        tool.SetActive(false);
                    }
                    foreach (var tool in tools[step + 1])
                    {
                        tool.SetActive(true);
                    }
                }
            }
        }

        public static void SetPreviousStep()
        {
            // forbid to go back to PREPARE
            if (VRMode)
            {

            }
            else
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    step = step > Step.GENERATE ? step - 1 : step;
                    foreach (var tool in tools[step])
                    {
                        tool.SetActive(false);
                    }
                    foreach (var tool in tools[step - 1])
                    {
                        tool.SetActive(true);
                    }
                }
            }    
        }
    }
}
