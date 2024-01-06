using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace ShoesDesigner
{
    public class Brush : MonoBehaviour
    {
        public Color color;
        private int colorIndex = 0;
        private Color[] colors = { Color.white, Color.blue, Color.red, Color.yellow, Color.green, Color.black };

        private GameObject nib;

        private Texture2D prevTexture;
        private Vector2 prevPosition;

        private BrushType brushType;
        private int width;

        private GameObject[] brushes;

        public Hand rightHand;

        private void Awake()
        {
            // color = Color.black;

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
            nib = GameObject.Find("Nib").gameObject;
            interactable = GetComponent<Interactable>();
        }

        private void Update()
        {
            if (IsTriggered())
            {
                Draw();
            }
            UpdateColor();
        }

        

        private void UpdateColor()
        {
            var bindTrigger = rightHand.bindTrigger;
            if (bindTrigger.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
                if (colorIndex != colors.Length - 1)
                {
                    colorIndex += 1;
                }
                else
                {
                    colorIndex = 0;
                }
                color = colors[colorIndex];
            }

            if (bindTrigger.GetState(SteamVR_Input_Sources.RightHand))
            {
                
            }
        }

        public Vector3 GetNibPosition()
        {
            return nib.transform.position;
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
#if ENABLE_VR
            var attachTrigger = rightHand.attachTrigger;
            if (attachTrigger.GetStateDown(SteamVR_Input_Sources.RightHand))
            {
            }

            if (attachTrigger.GetState(SteamVR_Input_Sources.RightHand))
            {
            }

            return attachTrigger.GetState(SteamVR_Input_Sources.RightHand);
#else
            return Input.GetMouseButton(0);
#endif
        }

        private Interactable interactable;

        private Hand.AttachmentFlags attachmentFlags = Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);

        private Vector3 oldPosition;
        private Quaternion oldRotation;

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

        private void Draw()
        {
            RaycastHit hit;
            if (!Physics.Raycast(nib.transform.position, nib.transform.forward, out hit))
            {
                return;
            }

            //var a = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //a.transform.position = hit.transform.position;
            //a.transform.localScale = Vector3.one;
            //Debug.Log(hit.transform.position);

            if ((hit.transform.position - nib.transform.position).magnitude > 0.3f)
            {
                return; // TODO: tweaks the distance
            }

            var meshEditor = hit.transform.GetComponent<MeshEditor>();
            if (meshEditor == null || !meshEditor.editable)
            {
                return;
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

            //if (prevTexture == null)
            //{
            //    prevTexture = texture;
            //}

            //if (prevTexture == texture)
            //{
            //    if ((prevPosition - pixelUV).magnitude < (width >> 2))
            //    {
                    DrawTexture(texture, pixelUV);
            //    }
            //    else
            //    {
            //        var inter = 1 / Mathf.Max(Mathf.Abs(pixelUV.x - prevPosition.x), Mathf.Abs(pixelUV.y - prevPosition.y) / (width >> 2));
            //        var num = 0.0f;
            //        while (num <= 1)
            //        {
            //            num += inter;
            //            DrawTexture(texture, Vector2.Lerp(prevPosition, pixelUV, 3 * Mathf.Pow(num, 2) - 2 * Mathf.Pow(num, 3)));
            //        }
            //        DrawTexture(texture, pixelUV);
            //    }
            //} 
            
            //prevTexture = texture;
            //prevPosition = pixelUV;
            texture.Apply();
        }

        private void DrawTexture(Texture2D texture, Vector2 pixelUV)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    //var oriColor = texture.GetPixel((int)pixelUV.x + i - (width >> 1), (int)pixelUV.y + j - (width >> 1));
                    //var resColor = color * color.a + (1 - color.a) * oriColor;
                    //texture.SetPixel((int)pixelUV.x + i - (width >> 1), (int)pixelUV.y + j - width >> 1, resColor);
                    texture.SetPixel((int)pixelUV.x + i - (width >> 1), (int)pixelUV.y + j - (width >> 1), color);
                }
            }
        }

        public enum BrushType : byte
        {   
            SMALL = 0,
            MEDIUM,
            BIG,
            LARGE
        }
    }
}
