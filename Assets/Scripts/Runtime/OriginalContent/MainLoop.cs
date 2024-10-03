using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation.Samples;
using System;

namespace UnityEngine.XR.ARFoundation.Samples
{
    public class MainLoop : MonoBehaviour
    {
        Vector3 mainPos;
        Vector3 mainNorm;
        float mainRot = 361;

        List<Vector3> secondaryPos = new List<Vector3>();
        List<Vector3> secondaryNorm = new List<Vector3>();
        List<float> secondaryRot = new List<float>();

        GameObject PlayField;

        // Update is called once per frame
        void Update()
        {
            if (ARPlaceAnchor.flag)
            {
                if (mainRot == 361) // replace with a place gamearea flag
                {
                    mainPos = ARPlaceAnchor.pos;
                    mainNorm = ARPlaceAnchor.rot;

                    // Enable play field
                }

                else
                {
                    secondaryPos.Add(ARPlaceAnchor.pos);
                    secondaryNorm.Add(ARPlaceAnchor.rot);
                }

                PlayField = transform.Find("Play Field").gameObject;
                PlayField.SetActive(true);
                PlayField.transform.rotation = Quaternion.LookRotation(ARPlaceAnchor.rot);
                PlayField.transform.Rotate(90, 0, 0);
                PlayField.transform.position = ARPlaceAnchor.pos;

                ARPlaceAnchor.obj.SetActive(false);

                ARPlaceAnchor.flag = false;
            }

            if (PlayField == transform.Find("Play Field").gameObject) //
            { //
                // PlayField.transform.RotateAround(ARPlaceAnchor.pos, ARPlaceAnchor.rot, 1); //
            } //
        }
    }
}


/*
To do:
=========
Slider for gameplay scale size
Rotation slider
*/