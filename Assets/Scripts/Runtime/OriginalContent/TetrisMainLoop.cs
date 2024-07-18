using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.ARFoundation.Samples
{
    public class TetrisMainLoop : MonoBehaviour
    {
        // 2 --> Game block dimension
        // 10*2 --> 20 --> Width
        // 2 *2 --> 4  --> Length
        // 20*2 --> 40 --> Height

        public GameObject[] Prefabs;
        public int QueueSize = 6;

        private int State = 0; // 0: Waiting for next; 1: Object falling; 2: At bottom
        private float LockInitialTime = -100;
        public float LockDelay = 0.5f;
        private float EntryInitialTime = -100;
        public float EntryDelay = 0.5f;

        public float GravityMult = 1f;
        private float Gravity = -9.8f; // velocity in unityunit/sec

        private GameObject[] Queue;
        // private GameObject[] Hold;
        private GameObject FallingObject;
        private GameObject Armature;

        // Start is called before the first frame update
        void Start()
        {
            Queue = new GameObject[QueueSize];

            for (int i = 0; i < QueueSize; i++)
            {
                Queue[i] = Prefabs[Random.Range(0, Prefabs.Length)];
            }

            // foreach (GameObject a in Queue)
            // {
            //     Debug.Log(a);
            // }
        }

        // Update is called once per frame
        void Update()
        {
            switch (State)
            {
                case 0:
                    if (Time.fixedTime >= EntryInitialTime + EntryDelay)
                    {
                        FallingObject = Instantiate(Queue[0], transform.localPosition + new Vector3(0, 5, 0), new Quaternion(0,0.707106829f,0,0.707106829f), this.transform);
                        Armature = FallingObject.transform.GetChild(0).gameObject;

                        for (int i = 0; i < QueueSize - 1; i++)
                        {
                            Queue[i] = Queue[i+1];
                        }

                        Queue[QueueSize-1] = Prefabs[Random.Range(0, Prefabs.Length)];
                        
                        State = 1;
                    }
                    break;
                
                case 1:
                	FallingObject.transform.position += new Vector3(0, Gravity * Time.deltaTime * GravityMult, 0);

                    if (true)
                    {
                        LockInitialTime = Time.fixedTime;
                        State = 2;
                    }
                    break;

                case 2:
                    if (Time.fixedTime >= LockInitialTime + LockDelay)
                    {
                 	    Armature.GetComponent<Riggedbody>().gravityToggle = true;
                        EntryInitialTime = Time.fixedTime;
                        State = 0;
                    }
                    break;
            }
        }
    }
}
