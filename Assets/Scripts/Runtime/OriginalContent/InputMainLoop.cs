using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation.Samples;

namespace UnityEngine.XR.ARFoundation.Samples
{
    public class InputMainLoop : MonoBehaviour
    {
        private Vector2 Pos1;
        private Vector2 DeltaPos;

        public int State = 0; // 0: Transform mode; 1: Rotation mode
        [HideInInspector]
        public int? Mode = null; // 0: Transform/Rotate; 1: Falling(top half)
        [HideInInspector]
        public int? Direction = null; // 0: Roll; 1: Pitch; 2: Yaw
        public float TranslateSensitivity = 0.05f;
        public float YawPitchScale = 0.4f;
        public float OrientationDiameter = 0.5f;
        private float? Rotation1 = null;

        private float TimeStart;
        private bool Moved;
        public float TapTime = 0.2f;
        public float RotTapTime = 0.05f;
        public float SoftFallStartTime = 0.2f;

        private float HardFallStartY;
        public float HardFallYThresh = 0.2f;
        private float? HardFallDist = null;
        
        public float HudScale = 0.15f; // range: (0, 0.5)

        private GameObject TetrisPiece;
        private GameObject TetrisManager;
        private GameObject Plane;
        private GameObject AxesCanvas;

        public bool Snap = true;
        public float TranslateSnapThresh = 0.6f;
        public float RotationSnapThresh = 30f;

        // Start is called before the first frame update
        void Start()
        {
            TetrisManager = GameObject.Find("Tetris Manager");
            Plane = TetrisManager.transform.GetChild(1).GetChild(0).gameObject;
            AxesCanvas = transform.parent.GetChild(3).GetChild(0).gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount != 1) 
            {
                return;
            }

            Touch touch = Input.GetTouch(0); // get first touch since touch count is greater than zero
            
            if (touch.phase == TouchPhase.Began)
            {
                Pos1 = touch.position;
                HardFallStartY = touch.position.y;
                TimeStart = Time.time;
                Moved = false;
                if (touch.position.y < Screen.height/2 && touch.position.x < Screen.width*(1-HudScale))
                {
                    Mode = 0;
                }
                else if (touch.position.y >= Screen.height/2 && touch.position.x > Screen.width*HudScale && touch.position.x < Screen.width*(1-HudScale))
                {
                    Mode = 1;
                }
                else
                {
                    Mode = null;
                }
                
                return;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                TetrisManager.GetComponent<TetrisMainLoop>().SoftFalling = false;
                Direction = null;
                Rotation1 = null;

                // Snap to translation/rotation
                if (Snap)
                {
                    TetrisPiece = TetrisManager.GetComponent<TetrisMainLoop>().FallingObject;
                    if (Math.Abs(TetrisPiece.transform.localPosition.x - (Mathf.Round((TetrisPiece.transform.localPosition.x+TetrisPiece.GetComponent<ParentFlagScript>().SnapOffset.x)/2)*2-TetrisPiece.GetComponent<ParentFlagScript>().SnapOffset.x)) <= TranslateSnapThresh)
                    {
                        TetrisPiece.transform.localPosition = new Vector3 (Mathf.Round((TetrisPiece.transform.localPosition.x+1)/2)*2-1, TetrisPiece.transform.localPosition.y, TetrisPiece.transform.localPosition.z);
                    }
                    if (Math.Abs(TetrisPiece.transform.localPosition.z - (Mathf.Round((TetrisPiece.transform.localPosition.z+TetrisPiece.GetComponent<ParentFlagScript>().SnapOffset.z)/2)*2-TetrisPiece.GetComponent<ParentFlagScript>().SnapOffset.z)) <= TranslateSnapThresh)
                    {
                        TetrisPiece.transform.localPosition = new Vector3 (TetrisPiece.transform.localPosition.x, TetrisPiece.transform.localPosition.y, Mathf.Round((TetrisPiece.transform.localPosition.z+1)/2)*2-1);
                    }

                    if (Math.Abs(TetrisPiece.transform.eulerAngles.x - Mathf.Round(TetrisPiece.transform.eulerAngles.x/90)*90) <= RotationSnapThresh)
                    {
                        TetrisPiece.transform.eulerAngles = new Vector3 (Mathf.Round(TetrisPiece.transform.eulerAngles.x/90)*90, TetrisPiece.transform.eulerAngles.y, TetrisPiece.transform.eulerAngles.z);
                    }
                    if (Math.Abs(TetrisPiece.transform.eulerAngles.y - Mathf.Round(TetrisPiece.transform.eulerAngles.y/90)*90) <= RotationSnapThresh)
                    {
                        TetrisPiece.transform.eulerAngles = new Vector3 (TetrisPiece.transform.eulerAngles.x, Mathf.Round(TetrisPiece.transform.eulerAngles.y/90)*90, TetrisPiece.transform.eulerAngles.z);
                    }
                    if (Math.Abs(TetrisPiece.transform.eulerAngles.z - Mathf.Round(TetrisPiece.transform.eulerAngles.z/90)*90) <= RotationSnapThresh)
                    {
                        TetrisPiece.transform.eulerAngles = new Vector3 (TetrisPiece.transform.eulerAngles.x, TetrisPiece.transform.eulerAngles.y, Mathf.Round(TetrisPiece.transform.eulerAngles.z/90)*90);
                    }
                }
            }
            
            if (TetrisManager.GetComponent<TetrisMainLoop>().State == 0)
            {
                if (touch.phase == TouchPhase.Ended && Time.time - TimeStart < TapTime && touch.position.y < Screen.height/2 && touch.position.x < Screen.width*(1-HudScale))
                {
                    State = 1 - State;
                }

                return;
            }

            TetrisPiece = TetrisManager.GetComponent<TetrisMainLoop>().FallingObject;
            
            if (Mode == 0)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    Moved = true;
                    switch (State)
                    {
                        case 0:
                            DeltaPos = touch.position - Pos1;
                            Pos1 = touch.position;

                            TetrisPiece.transform.Translate(TranslateSensitivity * (DeltaPos.x * Plane.transform.right + DeltaPos.y * Plane.transform.forward), Space.World);
                            break;
                        case 1:
                            if (Direction == null)
                            {
                                DeltaPos = touch.position - Pos1;

                                if (DeltaPos == new Vector2 (0, 0))
                                {
                                    break;
                                }

                                Vector2 mult = new Vector2 (1, (DeltaPos.y > 0 ? 1 : -1) * (DeltaPos.x > 0 ? 1 : -1));

                                // https://www.desmos.com/calculator/kgzwrp4htc
                                Vector2[] positions = new Vector2[6] {mult * new Vector2(Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width + 1, 2)/(1+Mathf.Pow(DeltaPos.x/DeltaPos.y, 2))), -Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width + 1, 2)/(1+Mathf.Pow(DeltaPos.y/DeltaPos.x, 2)))),
                                                                      mult * new Vector2(-Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width + 1, 2)/(1+Mathf.Pow(DeltaPos.x/DeltaPos.y, 2))), Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width + 1, 2)/(1+Mathf.Pow(DeltaPos.y/DeltaPos.x, 2)))),
                                                                      mult * new Vector2(Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1/Mathf.Pow(YawPitchScale, 2)+Mathf.Pow(DeltaPos.x/DeltaPos.y/Mathf.Pow(YawPitchScale, 2), 2))), -Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1+Mathf.Pow(DeltaPos.y/DeltaPos.x*YawPitchScale, 2)))),
                                                                      mult * new Vector2(-Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1/Mathf.Pow(YawPitchScale, 2)+Mathf.Pow(DeltaPos.x/DeltaPos.y/Mathf.Pow(YawPitchScale, 2), 2))), Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1+Mathf.Pow(DeltaPos.y/DeltaPos.x*YawPitchScale, 2)))),
                                                                      mult * new Vector2(Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1+Mathf.Pow(DeltaPos.x/DeltaPos.y*YawPitchScale, 2))), -Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1/Mathf.Pow(YawPitchScale, 2)+Mathf.Pow(DeltaPos.y/DeltaPos.x/Mathf.Pow(YawPitchScale, 2), 2)))),
                                                                      mult * new Vector2(-Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1+Mathf.Pow(DeltaPos.x/DeltaPos.y*YawPitchScale, 2))), Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1/Mathf.Pow(YawPitchScale, 2)+Mathf.Pow(DeltaPos.y/DeltaPos.x/Mathf.Pow(YawPitchScale, 2), 2))))}; // Roll, Roll, Pitch, Pitch, Yaw, Yaw
                                
                                float MinDistance = Mathf.Infinity;
                                float distance;

                                for (int i = 0; i < 6; i++)
                                {
                                    distance = Vector2.Distance(positions[i], touch.position - new Vector2(Screen.width * 0.5f, Screen.height * 0.25f));
                                    
                                    if (distance < MinDistance)
                                    {
                                        MinDistance = distance;
                                        Direction = (int)Mathf.Floor(i/2);
                                    }
                                }
                            }
                            else
                            {
                                // https://www.desmos.com/calculator/hruesokoea
                                Vector2 Point;
                                float Rotation;

                                Vector2 NormPos = touch.position - new Vector2(Screen.width * 0.5f, Screen.height * 0.25f);

                                Vector2 mult = new Vector2 (NormPos.x > 0 ? 1 : -1, NormPos.y > 0 ? 1 : -1);
                                
                                switch (Direction)
                                {
                                    case 0: // Roll
                                        Point = mult * new Vector2(Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1+Mathf.Pow(NormPos.y/NormPos.x,2))), Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1+Mathf.Pow(NormPos.x/NormPos.y,2))));
                                        Rotation = Mathf.Atan2(Point.y, Point.x);
                                        if (Rotation1 != null)
                                        {
                                            TetrisPiece.transform.Rotate(Mathf.Rad2Deg * (Rotation - Rotation1.Value) * Plane.transform.forward, Space.World);
                                        }
                                        Rotation1 = Rotation;
                                        break;
                                    case 1: // Pitch
                                        Point = mult * new Vector2(Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1/Mathf.Pow(YawPitchScale, 2)+Mathf.Pow(NormPos.y/NormPos.x,2))), Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1+Mathf.Pow(NormPos.x/NormPos.y,2)/Mathf.Pow(YawPitchScale, 2))));
                                        Rotation = Mathf.Atan2(Point.y, Point.x*1/YawPitchScale);
                                        if (Rotation1 != null)
                                        {
                                            TetrisPiece.transform.Rotate(-1 * Mathf.Rad2Deg * (Rotation - Rotation1.Value) * Plane.transform.right, Space.World);
                                        }
                                        Rotation1 = Rotation;
                                        break;
                                    case 2: // Yaw
                                        Point = mult * new Vector2(Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1+Mathf.Pow(NormPos.y/NormPos.x,2)/Mathf.Pow(YawPitchScale, 2))), Mathf.Sqrt(Mathf.Pow(OrientationDiameter/2 * Screen.width, 2)/(1/Mathf.Pow(YawPitchScale, 2)+Mathf.Pow(NormPos.x/NormPos.y,2))));
                                        Rotation = Mathf.Atan2(Point.y*1/YawPitchScale, Point.x);
                                        if (Rotation1 != null)
                                        {
                                            TetrisPiece.transform.Rotate(-1 * Mathf.Rad2Deg * (Rotation - Rotation1.Value) * Plane.transform.up, Space.World);
                                        }
                                        Rotation1 = Rotation;
                                        break;
                                }
                            }

                            break;
                    }
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    if (!Moved && Time.time - TimeStart < TapTime)
                    {
                        State = 1 - State;
                        AxesCanvas.SetActive(State == 1 ? true : false);
                    }
                }
            }
            else if (Mode == 1)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    Pos1 = touch.position;
                    TimeStart = Time.time;
                    TetrisManager.GetComponent<TetrisMainLoop>().SoftFalling = false;

                    if (HardFallStartY - touch.position.y > HardFallYThresh * (Screen.height/2 - Screen.width*HudScale))
                    {
                        RaycastHit hit;

                        foreach(GameObject go in TetrisPiece.transform.GetChild(0).GetComponent<Riggedbody>()._bones)
                        {
                            for (float i = -1f; i <= 1; i += 0.2f)
                            {
                                for (float j = -1f; j<= 1; j += 0.2f)
                                {
                                    if (Physics.Raycast(go.transform.position + new Vector3(i, -5, j), transform.TransformDirection(-Plane.transform.up), out hit, TetrisManager.GetComponent<TetrisMainLoop>().Height+5f))
                                    {
                                        if (HardFallDist == null){
                                            HardFallDist = hit.distance;
                                        }
                                        else
                                        {
                                            HardFallDist = Math.Min(HardFallDist.Value, hit.distance);
                                        }
                                    }
                                }
                            }
                        }

                        if (HardFallDist != null)
                        {
                            TetrisPiece.transform.position += (HardFallDist.Value + 2) * Vector3.down;
                            HardFallStartY = 0;
                            HardFallDist = null;
                        }
                    }
                }
                else if (touch.phase == TouchPhase.Stationary && Time.time - TimeStart > SoftFallStartTime)
                {
                    TetrisManager.GetComponent<TetrisMainLoop>().SoftFalling = true;
                }
            }
        }
    }
}
