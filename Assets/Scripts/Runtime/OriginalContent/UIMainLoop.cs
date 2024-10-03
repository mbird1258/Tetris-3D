using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.ARFoundation.Samples
{
    public class UIMainLoop : MonoBehaviour
    {
        // [HideInInspector]
        public UI.Button ButtonFrom0;
        public UI.Button ButtonFrom1;
        public UI.Button ButtonFromStart;
        public UI.Button ButtonToggleSnap;
        public UI.Scrollbar RotationScrollBar;
        public UI.Scrollbar ScaleScrollBar;
        private GameObject PlayField;
        private Transform UIManager;

        // Start is called before the first frame update
        void Start()
        {
            Transform Roll = transform.GetChild(0).GetChild(0);
            Transform Pitch = transform.GetChild(0).GetChild(1);
            Transform Yaw = transform.GetChild(0).GetChild(2);
            Transform InputManager = transform.parent.GetChild(2);

            Roll.localScale = InputManager.GetComponent<InputMainLoop>().OrientationDiameter * Screen.width * new Vector3(1,1,1);
            Pitch.localScale = InputManager.GetComponent<InputMainLoop>().OrientationDiameter * Screen.width * new Vector3(InputManager.GetComponent<InputMainLoop>().YawPitchScale,1,1);
            Yaw.localScale = InputManager.GetComponent<InputMainLoop>().OrientationDiameter * Screen.width * new Vector3(1,InputManager.GetComponent<InputMainLoop>().YawPitchScale,1);

            PlayField = transform.parent.GetChild(0).gameObject;
            UIManager = transform.parent.GetChild(2);

            // Bind buttons
            ButtonFrom0.onClick.AddListener(From0);
            ButtonFrom1.onClick.AddListener(From1);
            ButtonFromStart.onClick.AddListener(FromStart);
            ButtonToggleSnap.onClick.AddListener(ToggleSnap);
            RotationScrollBar.onValueChanged.AddListener(ChangeRotation);
            ScaleScrollBar.onValueChanged.AddListener(ChangeScale);
        }

        // Update is called once per frame
        void Update()
        {
            transform.GetChild(3).GetChild(0).Rotate(0, 90 * Time.deltaTime, 0, Space.World);
        }

        void From0()
        {
            transform.parent.GetChild(1).gameObject.SetActive(true);
            ButtonFrom0.transform.parent.gameObject.SetActive(false);
            ButtonFrom1.transform.parent.gameObject.SetActive(true);
            GameObject.Find("XR Origin").GetComponent<ARPlaceAnchor>().enabled = false;
            transform.parent.GetChild(2).gameObject.SetActive(true);
        }

        void From1()
        {
            UIManager.GetComponent<InputMainLoop>().State = 0;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.parent.GetChild(1).gameObject.SetActive(false);
            ButtonFrom0.transform.parent.gameObject.SetActive(true);
            ButtonFrom1.transform.parent.gameObject.SetActive(false);
            GameObject.Find("XR Origin").GetComponent<ARPlaceAnchor>().enabled = true;
            transform.parent.GetChild(2).gameObject.SetActive(false);
        }

        void FromStart()
        {
            transform.GetChild(2).gameObject.SetActive(false);
            ButtonFrom0.transform.parent.gameObject.SetActive(true);
            ButtonFromStart.transform.parent.gameObject.SetActive(false);
            GameObject.Find("XR Origin").GetComponent<ARPlaceAnchor>().enabled = true;
            GameObject.Find("Main Camera").GetComponent<Camera>().enabled = true;
        }

        void ToggleSnap()
        {
            if (UIManager.GetComponent<InputMainLoop>().Snap)
            {
                UIManager.GetComponent<InputMainLoop>().Snap = false;
                ButtonToggleSnap.transform.GetChild(0).gameObject.SetActive(false);
                ButtonToggleSnap.transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                UIManager.GetComponent<InputMainLoop>().Snap = true;
                ButtonToggleSnap.transform.GetChild(0).gameObject.SetActive(true);
                ButtonToggleSnap.transform.GetChild(1).gameObject.SetActive(false);
            }
        }

        void ChangeRotation(float Value)
        {
            PlayField.transform.eulerAngles = new Vector3(PlayField.transform.rotation.x, (Value - 0.5f)*360, PlayField.transform.rotation.z);
        }

        void ChangeScale(float Value)
        {
            PlayField.transform.localScale =  Mathf.Pow(Value, 2) * Vector3.one;
        }
    }
}
