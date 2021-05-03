using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewSimpleIK : MonoBehaviour
{
    public Animator anim;
    private Vector3 rightFootPosition, leftFootPosition, leftFootIkPosition, rightFootIkPosition;
    private Quaternion leftFootIkRotation, rightFootIkRotation;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;
    RaycastHit feetOutHit;
    [Header("Feet Grounder")]
    public bool enableFeetIk = true;
    [Range(-5, 30)] [SerializeField] private float heightFromGroundRaycast = 20f;
    [Range(-5, 30)] [SerializeField] private float raycastDownDistance = 20f;
    [SerializeField] private LayerMask Ground;
    [SerializeField] private float pelvisOffset = 0f;
    [Range(0, 1)] [SerializeField] private float pelvisUpAndDownSpeed = 0.28f;
    [Range(0, 1)] [SerializeField] private float feetToIkPositionSpeed = 0.5f;
    // Naming in animator curves
    public string leftFootAnimVariableName = "LeftFootCurve";
    public string rightFootAnimVariableName = "RightFootCurve";
    // Bools
    public bool useProIkFeature = false;
    public bool showSolverDebug = true;
    private void Start()
    {
        Ground = LayerMask.GetMask("Ground");
    }
    private void FixedUpdate()
    {   // return if false or null   
        if (enableFeetIk == false) { return; }
        if (anim == null) { return; }
        // feet posiitoning
        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);
        //find and raycast to the ground to find positions
        FeetPositionSolver(rightFootPosition, ref rightFootIkPosition, ref rightFootIkRotation); // handle the solver for right foot
        FeetPositionSolver(leftFootPosition, ref leftFootIkPosition, ref leftFootIkRotation); //handle the solver for the left foot
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (enableFeetIk == false) { return; }
        if (anim == null) { return; }
        MovePelvisHeight();
        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        if (useProIkFeature)
        {
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnimVariableName));
        }
        MoveFeetToIkPoint(AvatarIKGoal.RightFoot,
            rightFootIkPosition,
            rightFootIkRotation,
            ref lastRightFootPositionY);
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

        if (useProIkFeature)
        {
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName));
        }
        MoveFeetToIkPoint(AvatarIKGoal.LeftFoot,
            leftFootIkPosition,
            leftFootIkRotation,
            ref lastLeftFootPositionY);
    }
    void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPositionY)
    {
        Vector3 targetIkPosition = anim.GetIKPosition(foot);
        if (positionIkHolder != Vector3.zero)
        {
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);
            float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, feetToIkPositionSpeed);
            targetIkPosition.y += yVariable;
            lastFootPositionY = yVariable;
            targetIkPosition = transform.TransformPoint(targetIkPosition);
            anim.SetIKRotation(foot, rotationIkHolder);
        }
        anim.SetIKPosition(foot, targetIkPosition);
    }
    private void MovePelvisHeight()
    {
        if (rightFootIkPosition == Vector3.zero || leftFootIkPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = anim.bodyPosition.y;
            return;
        }
        float lOffsetPosition = (leftFootIkPosition.y) - transform.position.y;
        float rOffsetPosition = (rightFootIkPosition.y) - transform.position.y;
        float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;
        Vector3 newPelvisPosition = anim.bodyPosition + Vector3.up * totalOffset * .2f;
        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY,
        newPelvisPosition.y, pelvisUpAndDownSpeed);
        anim.bodyPosition = newPelvisPosition;
        lastPelvisPositionY = anim.bodyPosition.y;
    }
    private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPositions, ref Quaternion feetIkRotations)
    {
        //raycast handling section 
        if (showSolverDebug)
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow);
        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDownDistance + heightFromGroundRaycast, Ground))
        {
            //finding our feet ik positions from the sky position
            feetIkPositions = fromSkyPosition;
            feetIkPositions.y = feetOutHit.point.y + pelvisOffset;
            feetIkRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;
            return;
        }
        feetIkPositions = Vector3.zero; //it didn't work
    }
    private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = anim.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + heightFromGroundRaycast;
    }
}
