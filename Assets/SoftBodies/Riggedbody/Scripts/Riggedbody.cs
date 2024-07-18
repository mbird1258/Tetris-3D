using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adds rigidbodies and springs for all objects in _bones array. Use it for rigged model so it will behave like a soft body.
/// </summary>
public class Riggedbody : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _bones;
    [SerializeField]
    private float _mass;
    [SerializeField]
    private float _drag;
    [SerializeField]
    private float _angularDrag;
    [SerializeField]
    private float _damper;
    [SerializeField]
    private float _spring;
    [SerializeField]
    private float gravityScale = 1.0f;

    public bool gravityToggle = false;

    public static float globalGravity = -9.81f;

    void Start()
    {
        InitRigidbodies();
        InitSprings();
    }

    void Update()
    {
        if (gravityToggle)
        {
            foreach(GameObject go in _bones)
            {
                Rigidbody rb = go.GetComponent<Rigidbody>();
                
                // Custom Gravity
                Vector3 gravity = globalGravity * gravityScale * Vector3.up;
                rb.AddForce(gravity, ForceMode.Acceleration);

                rb.constraints = RigidbodyConstraints.FreezeRotationY;
            }
        }
    }

    void InitRigidbodies()
    {
        foreach(GameObject go in _bones)
        {
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = _mass;
            rb.drag = _drag;
            rb.angularDrag = _angularDrag;

            // Custom Gravity
            rb.useGravity = false;

            rb.constraints = RigidbodyConstraints.FreezeRotationY;
        }
    }

    void InitSprings()
    {
        foreach (GameObject goA in _bones)
        {
            foreach(GameObject goB in _bones)
            {
                if(goA != goB)
                {
                    SpringJoint sj = goA.AddComponent<SpringJoint>();
                    sj.spring = _spring;
                    sj.damper = _damper;
                    sj.connectedBody = goB.GetComponent<Rigidbody>();
                }
            }
        }
    }
}
