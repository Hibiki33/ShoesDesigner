using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

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

        public Color color;

        private bool showWireframe = true;

        private List<GameObject> controllPoints;
        private List<List<float>> riggingWeight;

        public Material transparent;

        private void Awake()
        {
            mesh = GetComponent<MeshFilter>().mesh;

            controllPoints = new List<GameObject>();
            string pattern = @"^cp";
            Regex regex = new Regex(pattern);
            foreach (Transform child in transform)
            {
                if (regex.IsMatch(child.name))
                {
                    controllPoints.Add(child.gameObject);
                }
            }
            
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
                weight[dis[0].Item2] = dis[0].Item1 / (dis[0].Item1 + dis[1].Item1);
                weight[dis[1].Item2] = dis[1].Item1 / (dis[0].Item1 + dis[1].Item1);
                riggingWeight.Add(weight);
            }
        }

        private void Start()
        {
            SetWireframeMaterial();

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

            var meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = transparent;
        }

        private void Update()
        {
            UpdateMesh();
            UpdateRigging();
            UpdateWireframeStatus();
        }

        int cpIndex;

        private void UpdateMesh()
        {
            var nibPostion = seniorPen.GetNibGlobal();

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
        }

        private void UpdateWireframeStatus()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                showWireframe = !showWireframe;
            }
        }

        private void SetWireframeMaterial()
        {
            wireframeMaterial = new Material(Shader.Find("Custom/Wireframe"));
            wireframeMaterial.hideFlags = HideFlags.HideAndDontSave;
            wireframeMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
            wireframeMaterial.SetColor("_Color", color);
        }

        private void UpdateRigging()
        {
            
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
    }
}