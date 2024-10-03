using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionReporter : MonoBehaviour
{   
    private TetrisMainLoop MainLoop;
    [HideInInspector]
    public ParentFlagScript Parent;
    private Transform temp;

    // Start is called before the first frame update
    void Start()
    {
        MainLoop = GameObject.Find("Tetris Manager").GetComponent<TetrisMainLoop>();
        
        temp = transform.parent;
        while (true)
        {
            if (temp.name == "Armature")
            {
                temp = temp.parent;
                break;
            }
            temp = temp.parent;
        }
        Parent = temp.gameObject.GetComponent<ParentFlagScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision col)
    {
        if (Parent.Collided)
        {
            return;
        }

        if (MainLoop.State == 1)
        {
            MainLoop.LockInitialTime = Time.fixedTime;
            MainLoop.State = 2;

            Parent.Collided = true;
        }
    }
}
