using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HandIK : MonoBehaviour
{
    //Animator of master
    Animator Anim;
    //Positions for hands and shoulders
    private Vector3 RHandPos, LHandPos;
    private Vector3 RShoulderPos, LShoulderPos;
    //ray info
    RaycastHit RightRayInfo, LeftRayInfo;
    //Distance to activate the handIK
    [Range(0, 10.0f)]
    [SerializeField]
    float WallDetectionDistance = 2.0f;
    //What layer mask do you want the HandIk to work with
    private LayerMask Wall;
    //bools to set the IK, Pro feature is for hand rotation
    public bool Right_HandIK_enabled = false, Left_HandIK_enabled = false, ProFeatures = false;
    //How fast the Hand moves to the surface
    [Range(0, 10.0f)]
    [SerializeField]
    public float HandIKSpeed;
    float MaxHandSpeed = 20.0f;
    //Distance between the hand and surface
    [Range(0, 1.0f)]
    [SerializeField]
    public float HandDistFromSurface = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        Anim = GetComponent<Animator>();
        Wall = LayerMask.GetMask("Wall");
    }
    // Update is called once per frame
    void Update()
    {
        //Gets current hand positions
        RHandPos = Anim.GetBoneTransform(HumanBodyBones.RightHand).position;
        LHandPos = Anim.GetBoneTransform(HumanBodyBones.LeftHand).position;
        //Draws rays
        MakeRightRay();
        MakeLeftRay();      
    }
    private void OnAnimatorIK()
    {
        if (Anim)
        {
            if (Right_HandIK_enabled)
            {
                //Right Hand IK              
                Anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                Vector3 newHandPos = RightRayInfo.point - RShoulderPos;
                Vector3 Line = RightRayInfo.point - HandDistFromSurface * newHandPos; // equation of line  so that hand does not land exactly on the wall
                Anim.SetIKPosition(AvatarIKGoal.RightHand, Vector3.Slerp(RHandPos, Line, Mathf.Lerp(0, HandIKSpeed, Time.deltaTime)));
                if (ProFeatures)
                {
                    //Do rotations here
                    Anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    Anim.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(transform.up, transform.right));
                    //Sets max hand speed after 1 second
                    StartCoroutine(WaitSecond());
                }
            }
            if (Left_HandIK_enabled)
            {
                //Left hand ik
                Anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                Vector3 newHandPos = LeftRayInfo.point - LShoulderPos;
                Vector3 Line = LeftRayInfo.point - HandDistFromSurface * newHandPos;
                //Tries to smoothly move hand position to wall
                //Only works if player is not moving
                //If player walks as they intially touch, then the hand strays behind for a split second
                Anim.SetIKPosition(AvatarIKGoal.LeftHand, Vector3.Slerp(LHandPos, Line, HandIKSpeed));
                if (ProFeatures)
                {
                    //Do rotations here
                    Anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    Anim.SetIKRotation(AvatarIKGoal.LeftHand, Quaternion.LookRotation(transform.up, -transform.right));
                   StartCoroutine(WaitSecond());

                }
            }
        }
    }
    private void MakeRightRay()
    {
        //Right ray
        RShoulderPos = Anim.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
        Vector3 RayOut = (transform.forward + transform.right) / 2; //get average of the two vectors (mid point)
        if (Physics.Raycast(RShoulderPos, RayOut, out RightRayInfo, WallDetectionDistance, Wall))
        {
            Right_HandIK_enabled = true;
            //rightelbowmover is the name of the gameobject for the rig on the elbow. This becomes active to keep the elbow form moving too much when the hand ik is working
           GameObject.Find("RightElbowMover").GetComponent<RigWeightChanger>().RightElbowWeight = 1;
        }
        else if (!Physics.Raycast(RShoulderPos, RayOut, out RightRayInfo, WallDetectionDistance, Wall))
        {
            Right_HandIK_enabled = false;
           GameObject.Find("RightElbowMover").GetComponent<RigWeightChanger>().RightElbowWeight = 0;
            Debug.DrawLine(RShoulderPos, RShoulderPos + RayOut * WallDetectionDistance, Color.red);
            HandIKSpeed = MaxHandSpeed;
        }
    }
    private void MakeLeftRay()
    {
        //Left ray
        LShoulderPos = Anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position;
        Vector3 RayOut = (transform.forward + -transform.right) / 2; //get average of the two vectors (mid point)
        if (Physics.Raycast(LShoulderPos, RayOut, out LeftRayInfo, WallDetectionDistance, Wall))
        {
            Left_HandIK_enabled = true;
            GameObject.Find("LeftElbowMover").GetComponent<RigWeightChanger>().LeftElbowWeight = 1;
        }
        else if (!Physics.Raycast(LShoulderPos, RayOut, out LeftRayInfo, WallDetectionDistance, Wall))
        {
            Left_HandIK_enabled = false;
            GameObject.Find("LeftElbowMover").GetComponent<RigWeightChanger>().LeftElbowWeight = 0;
            Debug.DrawLine(LShoulderPos, LShoulderPos + RayOut * WallDetectionDistance, Color.red);
           HandIKSpeed = MaxHandSpeed;
        }
    }

    IEnumerator WaitSecond()
    {
        //resets max hand speed along the wall so the hand does not drag behind the player
        yield return new WaitForSeconds(0.2f);
        HandIKSpeed = MaxHandSpeed;

    }
}
