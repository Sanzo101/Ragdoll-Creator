using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Grabbing : MonoBehaviour
{
    [System.Serializable]
    enum Hand
    {
        Left,
        Right
    }
    GameObject grabbedOBJ;
    [SerializeField]
    GameObject RightRig, LeftRig;
    GameObject Rig;
    int MouseButton;
    [SerializeField]
    Hand WhatHand;
    [SerializeField]
    float BreakForce = 9000f;

    // Start is called before the first frame update
    void Start()
    {
        // Choose what hand in the inspector
        // Changes the mousebutton pressed and what rig to be used
        switch (WhatHand)
        {
            case Hand.Left:
                MouseButton = 0;
                Rig = LeftRig;
                break;
            case Hand.Right:
                MouseButton = 1;
                Rig = RightRig;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Checks what button was pressed
        // Sets the rig weight to 1
        // creates a fixed joint connected to the item
        if (Input.GetMouseButton(MouseButton))
        {
            Rig.GetComponent<Rig>().weight = 1;
            if (grabbedOBJ != null)
            {
                if (!gameObject.GetComponent<FixedJoint>())
                {
                    FixedJoint fj = gameObject.AddComponent<FixedJoint>();
                    fj.connectedBody = grabbedOBJ.GetComponent<Rigidbody>();
                    fj.breakForce = BreakForce;
                }
            }
        }
        // Checks if mouse button is up
        // Sets rig weight to 0
        // Destroys the fixed joint on the hand
        else if (Input.GetMouseButtonUp(MouseButton))
        {
            Rig.GetComponent<Rig>().weight = 0;
            if (grabbedOBJ != null)
            {
                Destroy(GetComponent<FixedJoint>());
            }
            grabbedOBJ = null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        // Objects that want to be picked up need to have the tag "item"
        // Set the layer to whatever
        // note: the layer might make the player bounce around
        // if you dont want this to happen, set this layer as a "dontloosestrenghtlayer", in humanoid setup script, in inspector
        if (other.gameObject.tag.Contains("item"))
        {
            grabbedOBJ = other.gameObject;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Contains("item"))
        {
            grabbedOBJ = null;
        }
    }
}
