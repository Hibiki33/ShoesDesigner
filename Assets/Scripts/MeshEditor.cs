using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace ShoesDesigner
{
    public class MeshEditor : MonoBehaviour
    {
        private Mesh mesh;
        private Vector3[] vertices;
        private HashSet<Vector2Int> edges;

        private Material wireframeMaterial;

        public GameObject penEditor;
        private SeniorPen seniorPen;

        private Vector3 prevNibPosition;

        private float radio = 0.02f;
        private SortedSet<Tuple<float, int>> affected;

        public Color color = Color.black;

        private bool showWireframe = true;

        public Hand rightHand;

        public List<GameObject> controllPoints;
        private List<List<float>> riggingWeight;
        private int cpIndex;
        
        private Material initialMaterial;
        
        [Space]
        public Material transparentMaterial;

        [Space]
        public bool editable;
        public Material editableMaterial;

        private void Awake()
        {
            mesh = GetComponent<MeshFilter>().mesh;

            // controllPoints = new List<GameObject>();
            // string pattern = @"^cp";
            // Regex regex = new Regex(pattern);
            // foreach (Transform child in transform)
            // {
            //     if (regex.IsMatch(child.name))
            //     {
            //         controllPoints.Add(child.gameObject);
            //     }
            // }

            riggingWeight = new List<List<float>>();
            foreach (var vertex in mesh.vertices)
            {
                var worldVertex = transform.TransformPoint(vertex);

                // selected 2 neareast controll points
                var dis = new List<Tuple<float, int>>();
                for (var i = 0; i < controllPoints.Count; ++i)
                {
                    var cp = controllPoints[i];
                    var cpPosition = cp.transform.position;
                    var d = (cpPosition - worldVertex).magnitude;
                    dis.Add(new Tuple<float, int>(d, i));
                }
                dis.Sort();
                var weight = new List<float>();
                for (var i = 0; i < controllPoints.Count; ++i)
                {
                    weight.Add(0);
                }
                weight[dis[0].Item2] = dis[2].Item1 / (dis[0].Item1 + dis[1].Item1 + dis[2].Item1);
                weight[dis[1].Item2] = dis[1].Item1 / (dis[0].Item1 + dis[1].Item1 + dis[2].Item1);
                weight[dis[2].Item2] = dis[0].Item1 / (dis[0].Item1 + dis[1].Item1 + dis[2].Item1);
                riggingWeight.Add(weight);
            }

            var meshRenderer = GetComponent<MeshRenderer>();
            initialMaterial = meshRenderer.material;
        }

        private void Start()
        {
            SetWireframeMaterial();

            // mesh = GetComponent<MeshFilter>().mesh;

            edges = new HashSet<Vector2Int>();
            vertices = mesh.vertices;

            for(int i = 0; i < mesh.triangles.Length; i += 3)
            {
                edges.Add(new Vector2Int(mesh.triangles[i], mesh.triangles[i + 1]));
                edges.Add(new Vector2Int(mesh.triangles[i + 1], mesh.triangles[i + 2]));
                edges.Add(new Vector2Int(mesh.triangles[i + 2], mesh.triangles[i]));
            }

            if (penEditor == null)
            {
                penEditor = GameObject.Find("PenEditor");
            }
            seniorPen = penEditor.GetComponent<SeniorPen>();
            
            if (rightHand == null)
            {
                rightHand = GameObject.Find("Player/SteamVRObjects/RightHand").GetComponent<Hand>();
            }
        }

        private void Update()
        {
            UpdateMesh();
            if (StepManager.instance.GetCurrentStep() == StepManager.Step.BIGEDIT)
            {
                UpdateRigging();
            }
            
            UpdateWireframeStatus();
        }

        private void UpdateMesh()
        {
            var nibPostion = seniorPen.GetNibGlobal();

#if ENABLE_VR
            var attachTrigger = rightHand.attachTrigger;
            if (attachTrigger.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                prevNibPosition = nibPostion;
                affected = new SortedSet<Tuple<float, int>>();

                for (var i = 0; i < vertices.Length; ++i)
                {
                    var worldVertex = transform.TransformPoint(vertices[i]);
                    var dis = (worldVertex - nibPostion).magnitude;
                    if (dis < radio)
                    {
                        affected.Add(new Tuple<float, int>(dis, i));
                    }
                }

                //var a = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //a.transform.position = nibPostion;
                //a.transform.localScale = Vector3.one * 0.1f;

                // affected = new SortedSet<Tuple<float, int>>(affected.Take(6));
            }

            if (attachTrigger.GetState(SteamVR_Input_Sources.RightHand))
            {
                foreach (var (dis, index) in affected)
                {
                    var worldVertex = transform.TransformPoint(vertices[index]);
                    worldVertex += (nibPostion - prevNibPosition);
                    vertices[index] = transform.InverseTransformPoint(worldVertex);
                }
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh.RecalculateUVDistributionMetrics();
                prevNibPosition = nibPostion;
            }

#else
            if (Input.GetMouseButtonDown(0))
            {
                prevNibPosition = nibPostion;
                affected = new SortedSet<Tuple<float, int>>();

                for (var i = 0; i < vertices.Length; ++i)
                {
                    var worldVertex = transform.TransformPoint(vertices[i]);
                    var dis = (worldVertex - nibPostion).magnitude;
                    if (dis < radio)
                    {
                        affected.Add(new Tuple<float, int>(dis, i));
                    }
                }
                // affected = new SortedSet<Tuple<float, int>>(affected.Take(6));
            }

            if (Input.GetMouseButton(0))
            {
                foreach (var (dis, index) in affected)
                {
                    var worldVertex = transform.TransformPoint(vertices[index]);
                    worldVertex += (nibPostion - prevNibPosition);
                    vertices[index] = transform.InverseTransformPoint(worldVertex);
                }
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh.RecalculateUVDistributionMetrics();
                prevNibPosition = nibPostion;
            }
#endif
        }

        private void UpdateRigging()
        {
            var nibPostion = seniorPen.GetNibGlobal();

#if ENABLE_VR

            var bindTrigger = rightHand.bindTrigger;
            if (bindTrigger.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                prevNibPosition = nibPostion;

                cpIndex = 0;
                var dis = (controllPoints[cpIndex].transform.position - nibPostion).magnitude;
                for (var i = 1; i < controllPoints.Count; ++i)
                {
                    var tmp_dis = (controllPoints[i].transform.position - nibPostion).magnitude;
                    if (tmp_dis < dis)
                    {
                        cpIndex = i;
                        dis = tmp_dis;
                    }
                }

                if (dis > radio)
                {
                    cpIndex = -1;
                }
            }

            if (bindTrigger.GetState(SteamVR_Input_Sources.RightHand))
            {
                if (cpIndex < 0)
                {
                    return;
                }

                var prevPos = controllPoints[cpIndex].transform.position;
                var newPos = prevPos + (nibPostion - prevNibPosition) * 0.1f;
                controllPoints[cpIndex].transform.position = newPos;
                for (var i = 0; i < vertices.Length; ++i)
                {
                    var worldVertex = transform.TransformPoint(vertices[i]);
                    worldVertex += (nibPostion - prevNibPosition) * riggingWeight[i][cpIndex];
                    vertices[i] = transform.InverseTransformPoint(worldVertex);
                }
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh.RecalculateUVDistributionMetrics();
                prevNibPosition = nibPostion;
            }

#else
            if (Input.GetMouseButtonDown(2))
            {
                prevNibPosition = nibPostion;

                cpIndex = 0;
                var dis = (controllPoints[cpIndex].transform.position - nibPostion).magnitude;
                for (var i = 1; i < controllPoints.Count; ++i)
                {
                    var tmp_dis = (controllPoints[i].transform.position - nibPostion).magnitude;
                    if (tmp_dis < dis)
                    {
                        cpIndex = i;
                        dis = tmp_dis;
                    }
                }   
            }

            if (Input.GetMouseButton(2))
            {
                var prevPos = controllPoints[cpIndex].transform.position;
                var newPos = prevPos + (nibPostion - prevNibPosition);
                controllPoints[cpIndex].transform.position = newPos;
                for (var i = 0; i < vertices.Length; ++i)
                {
                    var worldVertex = transform.TransformPoint(vertices[i]);
                    worldVertex += (nibPostion - prevNibPosition) * riggingWeight[i][cpIndex];
                    vertices[i] = transform.InverseTransformPoint(worldVertex);
                }
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh.RecalculateUVDistributionMetrics();
                prevNibPosition = nibPostion;
            }
#endif
        }

        private void UpdateWireframeStatus()
        {
#if ENABLE_VR

#else
            if (Input.GetKeyDown(KeyCode.B))
            {
                showWireframe = !showWireframe;
            }
#endif
        }

        private void SetWireframeMaterial()
        {
            wireframeMaterial = new Material(Shader.Find("Custom/Wireframe"));
            wireframeMaterial.hideFlags = HideFlags.HideAndDontSave;
            wireframeMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            wireframeMaterial.SetColor("_Color", color);
        }

        private void OnRenderObject()
        {
            if (!showWireframe)
            {
                return;
            }
            wireframeMaterial.SetPass(0);

            GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.LINES);
            foreach(var edge in edges)
            {
                GL.Vertex(vertices[edge.x]);
                GL.Vertex(vertices[edge.y]);
            }
            GL.End();
        }

        public void ResetMaterial()
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = initialMaterial;
        }

        public void SetTransparentMaterial()
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = transparentMaterial;
        }

        public void SetEditableMaterial()
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = editableMaterial;
        }
    }
}