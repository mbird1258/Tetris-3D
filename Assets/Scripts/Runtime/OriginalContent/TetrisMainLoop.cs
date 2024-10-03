using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation.Samples;
using System.Linq;
using Unity.XR.CoreUtils;
using System.Data;
using UnityEngine.XR.ARFoundation;

public class TetrisMainLoop : MonoBehaviour
{
    // 2 --> Game block dimension
    // 10*2 --> 20 --> Width
    // 2 *2 --> 4  --> Length
    // 20*2 --> 40 --> Height

    public GameObject[] Prefabs;
    public GameObject BasePrefab;
    public int QueueSize = 6;

    [HideInInspector]
    public int State = 0; // 0: Waiting for next; 1: Object falling; 2: At bottom
    [HideInInspector]
    public float LockInitialTime = -100f;
    public float LockDelay = 0f;
    [HideInInspector]
    private float EntryInitialTime = -100f;
    public float EntryDelay = 0.5f;

    public float Height = 40f;

    public float GravityMult = 0.5f;
    public float SoftFallGravityMult = 5f;
    [HideInInspector]
    public bool SoftFalling = false;
    private float Gravity = -9.8f; // velocity in unityunit/sec
    float MaxSpeed = 2f;
    float SpeedScaleFactor = 1/50;

    private GameObject[] Queue;
    // private GameObject[] Hold;
    public GameObject FallingObject;
    private GameObject Armature;
    public GameObject MainCamera;
    public GameObject PlayField;
    public GameObject Camera;
    public GameObject Field;
    public GameObject Plane;
    public Material GhostMaterial;
    public GameObject PlaneCloneManager;
    
    private GameObject Ghost = null;
    [HideInInspector]
    public GameObject LastPrefab;

    private ARPlaneManager PlaneManager;

    // Start is called before the first frame update
    void Start()
    {
        // Setup tetris piece queue
        Queue = new GameObject[QueueSize];

        for (int i = 0; i < QueueSize; i++)
        {
            Queue[i] = Prefabs[Random.Range(0, Prefabs.Length)];
        }

        PlaneManager = GameObject.Find("XR Origin").transform.GetComponent<ARPlaneManager>();

        InvokeRepeating("CopyPlanes", 1, 1);
        InvokeRepeating("CheckLines", 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        // Position Cam
        Camera.transform.position = Plane.transform.position + 2/PlayField.transform.localScale.x * (MainCamera.transform.position - PlayField.transform.position);
        Camera.transform.rotation = MainCamera.transform.rotation;
        Field.transform.rotation = PlayField.transform.rotation;
        transform.GetChild(3).position = Plane.transform.position - PlayField.transform.position*2/PlayField.transform.localScale.x;
        
        // Delete OOB tetris pieces
        for(int childIndex = transform.GetChild(1).childCount-1 ; childIndex > 0 ; childIndex--)
        {
            if (transform.GetChild(1).GetChild(childIndex).localPosition.y + transform.GetChild(1).GetChild(childIndex).GetChild(0).localPosition.y < -1)
            {
                if (childIndex == transform.GetChild(1).childCount-1)
                {
                    State = 0;
                    Destroy(Ghost);
                }

                Destroy(transform.GetChild(1).GetChild(childIndex).gameObject);
                // animation WIP //
                // punish    WIP //
            }
        }

        // Handle Tetris Pieces
        switch (State)
        {
            case 0:
                if (Time.fixedTime >= EntryInitialTime + EntryDelay)
                {
                    // Instantiate tetris piece
                    LastPrefab = Queue[0];
                    FallingObject = Instantiate(Queue[0], 
                                                transform.localPosition + (Height + 5) * Plane.transform.up, 
                                                Field.transform.rotation * new Quaternion(0,0.707106829f,0,0.707106829f), 
                                                Field.transform);

                    FallingObject.layer = gameObject.layer;
                    foreach(Transform child in FallingObject.GetComponentsInChildren<Transform>(true))
                    {
                        child.gameObject.layer = gameObject.layer;
                    }

                    Armature = FallingObject.transform.GetChild(0).gameObject;

                    // Iterate queue
                    for (int i = 0; i < QueueSize - 1; i++)
                    {
                        Queue[i] = Queue[i+1];
                    }

                    Queue[QueueSize-1] = Prefabs[Random.Range(0, Prefabs.Length)];
                    
                    State = 1;
                }
                break;           
            case 1:
                float Dist;
                if (SoftFalling)
                {
                    Dist = Gravity * Time.deltaTime * GravityMult * SoftFallGravityMult;
                }
                else
                {
                    Dist = Gravity * Time.deltaTime * GravityMult;
                }

                RaycastHit[] hits;
                float GhostDist = -1 * Height - 100;

                foreach(GameObject go in FallingObject.transform.GetChild(0).GetComponent<Riggedbody>()._bones)
                {
                for (float i = -1f; i <= 1; i += 0.2f)
                {
                for (float j = -1f; j<= 1; j += 0.2f)
                {
                    hits = Physics.RaycastAll(go.transform.position + new Vector3(i, 0, j), transform.TransformDirection(-Plane.transform.up), Height + 5);
                    for (int k = 0; k < hits.Length; k++)
                    {
                        if (FallingObject.transform.GetChild(0).GetComponent<Riggedbody>()._bones.Contains(hits[k].collider.gameObject))
                        {
                            continue;
                        };

                        Dist = Mathf.Max(Dist, -1 * hits[k].distance + 0.4f);
                        GhostDist = Mathf.Max(GhostDist, -1 * hits[k].distance + 0.4f);
                    }
                }
                }
                }
                FallingObject.transform.position += Mathf.Min(Dist, -0.1f) * Plane.transform.up;
                
                if (Ghost == null)
                {
                    Ghost = Instantiate(LastPrefab, FallingObject.transform.position + GhostDist * Plane.transform.up, FallingObject.transform.rotation);
                    Ghost.transform.GetChild(0).gameObject.SetActive(false);
                    Ghost.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material = GhostMaterial;
                    Ghost.layer = gameObject.layer;
                    foreach(Transform child in Ghost.GetComponentsInChildren<Transform>(true))
                    {
                        child.gameObject.layer = gameObject.layer;
                    }
                }
                else
                {
                    Ghost.transform.position = FallingObject.transform.position + GhostDist * Plane.transform.up;
                    Ghost.transform.rotation = FallingObject.transform.rotation;
                }
                break;
            case 2:
                CheckLines();

                if (Time.fixedTime >= LockInitialTime + LockDelay)
                {
                    Armature.GetComponent<Riggedbody>().gravityToggle = true;
                    foreach(GameObject go in Armature.GetComponent<Riggedbody>()._bones)
                    {
                        Rigidbody rb = go.GetComponent<Rigidbody>();
                        rb.velocity = Gravity * GravityMult * Plane.transform.up;
                    }

                    EntryInitialTime = Time.fixedTime;
                    State = 0;
                    GravityMult += (MaxSpeed-GravityMult)*SpeedScaleFactor;
                    Destroy(Ghost);
                    Ghost = null;
                }
                break;
        }

        // Disable OOB Mesh Colliders to effectively crop planes outside of playfield
        foreach(SphereCollider child in Field.transform.GetComponentsInChildren<SphereCollider>())
        {
            Vector3 FieldLocalPos = Field.transform.InverseTransformPoint(child.transform.position);
            if (Mathf.Abs(FieldLocalPos.x) < 20 && Mathf.Abs(FieldLocalPos.z) < 12)
            {
                child.enabled = true;
            }
            else
            {
                child.enabled = false;
            }
        }
    }

    void CopyPlanes()
    {
        foreach (Transform clone in transform.GetChild(3))
        {
            Destroy(clone.gameObject);
        }

        foreach (ARPlane plane in PlaneManager.trackables)
        {
            GameObject clone = Instantiate(plane.gameObject, Plane.transform.position + (plane.transform.position - PlayField.transform.position)*2/PlayField.transform.localScale.x, plane.transform.rotation, transform.GetChild(3));
            clone.transform.localScale = new Vector3 (1, 1, 1)*2/PlayField.transform.localScale.x;
        }

        foreach (Transform child in transform.GetChild(3).GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = gameObject.layer;
        }
    }

    void CheckLines()
    {
        bool Success = false;
        GameObject[] gameobjects;

        for (float y = 1f; y < 2; y++)
        {
            gameobjects = new GameObject[0] {};
        for (float z = -1; z < 2; z += 2)
        {
        for (float x = -9f; x < 10; x += 2)
        {
            Success = false;

            foreach (SphereCollider child in Field.transform.GetComponentsInChildren<SphereCollider>())
            {
            if (child.bounds.Contains(Plane.transform.position + new Vector3 (x, y, z)))
            {
                Success = true;
                
                Transform Shape = child.transform.parent;
                while (true)
                {
                    if (Shape.name == "Armature")
                    {
                        Shape = Shape.parent;
                        break;
                    }
                    Shape = Shape.parent;
                }
                
                System.Array.Resize(ref gameobjects, gameobjects.Length + 1);
                gameobjects[gameobjects.GetUpperBound(0)] = Shape.gameObject;
                break;
            }
            }

            if (!Success)
            {
            foreach (SphereCollider child in Field.transform.GetComponentsInChildren<SphereCollider>())
            {
            if (child.bounds.Contains(Plane.transform.position + new Vector3 (x - 0.5f, y, z)) && child.bounds.Contains(Plane.transform.position + new Vector3 (x + 0.5f, y, z)))
            {
                Success = true;
                System.Array.Resize(ref gameobjects, gameobjects.Length + 1);
                gameobjects[gameobjects.GetUpperBound(0)] = child.gameObject;
                break;
            }
            }
            }

            if (!Success)
            {
                break;
            }
        }
            if (!Success)
            {
                break;
            }
        }
            Success = true;
            if (Success)
            {
                foreach (GameObject go in gameobjects.Distinct().ToArray())
                {
                    foreach (Vector2 pos in go.transform.GetComponent<ParentFlagScript>().ConstituantBlockPositions)
                    {
                        Vector3 BaseCubeWorldPos = go.transform.parent.TransformPoint(go.transform.localPosition + go.transform.GetChild(0).localPosition + new Vector3 (pos.x, pos.y, 0));

                        if (Mathf.Abs(transform.InverseTransformPoint(BaseCubeWorldPos).y - y) < 1)
                        {
                            continue;
                        }
                        
                        GameObject obj = Instantiate(BasePrefab, BaseCubeWorldPos, go.transform.rotation, go.transform.parent);
                        obj.transform.GetChild(0).GetComponent<Riggedbody>().gravityToggle = true;
                        obj.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material = go.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material;
                        obj.transform.GetComponent<ParentFlagScript>().Collided = true;
                        
                        obj.layer = gameObject.layer;
                        foreach(Transform child in obj.GetComponentsInChildren<Transform>(true))
                        {
                            child.gameObject.layer = gameObject.layer;
                        }
                    }

                    Destroy(go.transform.gameObject);
                }
            }
            Success = false;
        }
    }
}