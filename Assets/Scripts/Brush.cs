using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShoesDesigner
{
    public class Brush : MonoBehaviour
    {
        public Color color;

        private Transform nib;

        private Texture2D prevTexture;
        private Vector2 prevPosition;

        private BrushType brushType;
        private int width;

        private GameObject[] brushes;

        private void Awake()
        {
            color = Color.black;

            brushes = new GameObject[4];
            brushes[0] = transform.Find("Brush_s").gameObject;
            brushes[1] = transform.Find("Brush_m").gameObject;
            brushes[2] = transform.Find("Brush_b").gameObject;
            brushes[3] = transform.Find("Brush_l").gameObject;
            brushes[0].SetActive(false);
            brushes[1].SetActive(false);
            brushes[2].SetActive(false);
            brushes[3].SetActive(false);
            SetBrushType(BrushType.MEDIUM);
        }

        private void Start()
        {
            nib = transform.Find("Nib");
        }

        private void Update()
        {
            if (IsTriggered())
            {
                Draw();
            }
        }

        public Vector3 GetNibPosition()
        {
            return nib.position;
        }

        public void SetBrushType(BrushType type)
        {
            brushes[(int)brushType].SetActive(false);
            
            brushType = type;
            switch (brushType)
            {
                case BrushType.SMALL:
                    width = 10;
                    brushes[0].SetActive(true);
                    break;
                case BrushType.MEDIUM:
                    brushes[1].SetActive(true);
                    width = 20;
                    break;
                case BrushType.BIG:
                    brushes[2].SetActive(true);
                    width = 30;
                    break;
                case BrushType.LARGE:
                    brushes[3].SetActive(true);
                    width = 40;
                    break;
            }
        }

        private bool IsTriggered()
        {   
            // TODO: Add support for VR controllers
            return Input.GetMouseButton(0);
        }

        private void Draw()
        {
            RaycastHit hit;
            if (!Physics.Raycast(nib.position, nib.forward, out hit))
            {
                return;
            }
            if ((hit.transform.position - nib.position).magnitude > 1.0f)
            {
                return; // TODO: tweaks the distance
            }

            var renderer = hit.collider.GetComponent<Renderer>();
            var meshCollider = hit.collider as MeshCollider;
            if (renderer == null || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null || meshCollider == null)
            {
                return;
            }

            var texture = renderer.material.mainTexture as Texture2D;
            var pixelUV = hit.textureCoord;
            pixelUV.x *= texture.width;
            pixelUV.y *= texture.height;

            if (prevTexture == texture)
            {
                if ((prevPosition - pixelUV).magnitude < (width >> 2))
                {
                    DrawTexture(texture, pixelUV);
                }
                else
                {
                    var inter = 1 / Mathf.Max(Mathf.Abs(pixelUV.x - prevPosition.x), Mathf.Abs(pixelUV.y - prevPosition.y) / (width >> 2));
                    var num = 0.0f;
                    while (num <= 1)
                    {
                        num += inter;
                        DrawTexture(texture, Vector2.Lerp(prevPosition, pixelUV, 3 * Mathf.Pow(num, 2) - 2 * Mathf.Pow(num, 3)));
                    }
                    DrawTexture(texture, pixelUV);
                }
            } 
            
            prevTexture = texture;
            prevPosition = pixelUV;
            texture.Apply();
        }

        private void DrawTexture(Texture2D texture, Vector2 pixelUV)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    var oriColor = texture.GetPixel((int)pixelUV.x + i - (width >> 1), (int)pixelUV.y + j - (width >> 1));
                    var resColor = color * color.a + (1 - color.a) * oriColor;
                    texture.SetPixel((int)pixelUV.x + i - (width >> 1), (int)pixelUV.y + j - width >> 1, resColor);
                }
            }
        }

        public enum BrushType : int
        {   
            SMALL = 0,
            MEDIUM,
            BIG,
            LARGE
        }
    }
}
