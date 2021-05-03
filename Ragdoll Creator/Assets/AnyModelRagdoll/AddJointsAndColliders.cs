using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class AddJointsAndColliders : MonoBehaviour
{
    #region ColliderSizes
    // Size of Colliders
    float LengthOverLap = 0.1f;
    [Range(0.0f, 1.0f)]
    public float HipColliderSize;
    [Range(0.0f, 1.0f)]
    public float NeckColliderSize;
    [Range(0.0f, 1.0f)]
    public float HeadColliderSize;
    [Range(0.0f, 1.0f)]
    public float ChestColliderSize;
    [Range(0.0f, 1.0f)]
    public float SpineColliderSize;
    [Range(0.0f, 1.0f)]
    public float ColliderThickness;
    [Range(0.0f, 1.0f)]
    public float ArmColliderWidth;
    [Range(0.0f, 1.0f)]
    public float LegColliderWidth;
    #endregion
    #region RigidBody Masses
    // Rigid body mass
    [Range(0.0f, 10.0f)]
    public float HipsMass;
    [Range(0.0f, 10.0f)]
    public float SpineMass;
    [Range(0.0f, 10.0f)]
    public float ChestMass;
    [Range(0.0f, 10.0f)]
    public float NeckMass;
    [Range(0.0f, 10.0f)]
    public float HeadMass;
    [Range(0.0f, 10.0f)]
    public float lUpperArmMass;
    [Range(0.0f, 10.0f)]
    public float lLowerArmMass;
    [Range(0.0f, 10.0f)]
    public float lHandMass;
    [Range(0.0f, 10.0f)]
    public float rUpperArmMass;
    [Range(0.0f, 10.0f)]
    public float rLowerArmMass;
    [Range(0.0f, 10.0f)]
    public float rHandMass;
    [Range(0.0f, 10.0f)]
    public float lUpperLegMass;
    [Range(0.0f, 10.0f)]
    public float lLowerLegMass;
    [Range(0.0f, 10.0f)]
    public float lFootMass;
    [Range(0.0f, 10.0f)]
    public float rUpperLegMass;
    [Range(0.0f, 10.0f)]
    public float rLowerLegMass;
    [Range(0.0f, 10.0f)]
    public float rFootMass;
    #endregion
    #region Body Transforms
    // All transforms
    public Transform Hip;
    public Transform Spine;
    public Transform Chest;
    public Transform UpperChest;
    public Transform lShoulder;
    public Transform lUpperArm;
    public Transform lLowerArm;
    public Transform lHand;
    public Transform rShoulder;
    public Transform rUpperArm;
    public Transform rLowerArm;
    public Transform rHand;
    public Transform lUpperLeg;
    public Transform lLowerLeg;
    public Transform lFoot;
    public Transform lToes;
    public Transform rUpperLeg;
    public Transform rLowerLeg;
    public Transform rFoot;
    public Transform rToes;
    public Transform Neck;
    public Transform Head;
    public Transform lFingers;
    public Transform rFingers;
    #endregion

    void Awake()//Done is awake so the configurablejoints script knows there are rigid bodiex attatched
    {
        AddColliders();
        AddingJoints();
    }

    // Update is called once per frame
    void Update()
    {
        AdjustRidigBodies();
    }
    void AddColliders()
    {
        //Spine to hip position
        Vector3 upperArmToHipCentroid = SpineToHip();
        Vector3 shoulderDirection = rUpperArm.position - lUpperArm.position;
        float torsoWidth = shoulderDirection.magnitude - 1;
        float torsoProportionAspect = ColliderThickness;
        //Hips Collider
        Vector3 hipsStartPoint = Hip.position;
        Vector3 lastEndPoint = upperArmToHipCentroid;
        hipsStartPoint += (hipsStartPoint - upperArmToHipCentroid) * 0.1f;
        float hipsWidth = torsoWidth * 0.8f;
        CreateCollider(Hip,
            hipsStartPoint,
            lastEndPoint,
            LengthOverLap,
            hipsWidth,
            torsoProportionAspect,
            shoulderDirection
            );
        //Spine Collider
        Vector3 spineStartPoint = lastEndPoint;
        lastEndPoint = ChestToSpineCentre();
        float spineWidth = torsoWidth * 0.75f;
        CreateCollider(Spine,
            spineStartPoint,
            lastEndPoint,
            LengthOverLap,
            spineWidth,
            torsoProportionAspect,
            shoulderDirection);
        Vector3 chestStartPoint = lastEndPoint;
        lastEndPoint = ChestToNeckCentre();
        //Chest Collider
        CreateCollider(Chest,
            chestStartPoint,
            lastEndPoint,
            LengthOverLap,
            torsoWidth,
            torsoProportionAspect,
            shoulderDirection);
        //NecK
        Vector3 NeckStartPoint = lastEndPoint;
        lastEndPoint = HeadToNeckeCentre();
        float NeckWidth = torsoWidth * NeckColliderSize;
        CreateCollider(Neck,
            NeckStartPoint,
            lastEndPoint,
            LengthOverLap,
            NeckWidth,
            torsoProportionAspect,
            shoulderDirection);
        // Head collider
        Vector3 headStartPoint = lastEndPoint;
        Vector3 headEndPoint = headStartPoint + (headStartPoint - hipsStartPoint) * HeadColliderSize;
        Vector3 Headaxis = Head.TransformVector(GetAxisVectorToDirection(Head, headEndPoint - headStartPoint));
        headEndPoint = headStartPoint + Vector3.Project(headEndPoint - headStartPoint, Headaxis).normalized * (headEndPoint - headStartPoint).magnitude;
        CreateCollider(Head,
            headStartPoint,
            headEndPoint,
            LengthOverLap,
            Vector3.Distance(headStartPoint, headEndPoint) * 0.8f);

        // Arms colliders
        float leftArmWidth = Vector3.Distance(lUpperArm.position, lLowerArm.position) * ArmColliderWidth;
        CreateCollider(lUpperArm, lUpperArm.position, lLowerArm.position, LengthOverLap, leftArmWidth);
        CreateCollider(lLowerArm, lLowerArm.position, lHand.position, LengthOverLap, leftArmWidth * 0.9f);
        float rightArmWidth = Vector3.Distance(rUpperArm.position, rLowerArm.position) * ArmColliderWidth;
        CreateCollider(rUpperArm, rUpperArm.position, rLowerArm.position, LengthOverLap, rightArmWidth);
        CreateCollider(rLowerArm, rLowerArm.position, rHand.position, LengthOverLap, rightArmWidth * 0.9f);
        //Hands colliders
        CreateCollider(lHand, lHand.position, lFingers.position, LengthOverLap, leftArmWidth);
        CreateCollider(rHand, rHand.position, rFingers.position, LengthOverLap, rightArmWidth);
        //Legs colliders
        float leftLegWidth = Vector3.Distance(lUpperLeg.position, lLowerLeg.position) * LegColliderWidth;
        CreateCollider(lUpperLeg, lUpperLeg.position, lLowerLeg.position, LengthOverLap, leftLegWidth);
        CreateCollider(lLowerLeg, lLowerLeg.position, lFoot.position, LengthOverLap, leftLegWidth * 0.9f);
        float rightLegWidth = Vector3.Distance(rUpperLeg.position, rLowerLeg.position) * LegColliderWidth;
        CreateCollider(rUpperLeg, rUpperLeg.position, rLowerLeg.position, LengthOverLap, rightLegWidth);
        CreateCollider(rLowerLeg, rLowerLeg.position, rFoot.position, LengthOverLap, rightLegWidth * 0.9f);
        //Feet colliders
        CreateCollider(lFoot, lFoot.position, lToes.position, LengthOverLap, leftLegWidth);
        CreateCollider(rFoot, rFoot.position, rToes.position, LengthOverLap, rightLegWidth);
    }

    void AdjustRidigBodies()
    {
        // sets the mass of rigid bodies
        Hip.gameObject.GetComponent<Rigidbody>().mass = HipsMass;
        Spine.gameObject.GetComponent<Rigidbody>().mass = SpineMass;
        Chest.gameObject.GetComponent<Rigidbody>().mass = ChestMass;
        Neck.gameObject.GetComponent<Rigidbody>().mass = NeckMass;
        Head.gameObject.GetComponent<Rigidbody>().mass = HeadMass;
        lUpperArm.gameObject.GetComponent<Rigidbody>().mass = lUpperArmMass;
        lLowerArm.gameObject.GetComponent<Rigidbody>().mass = lLowerArmMass;
        lHand.gameObject.GetComponent<Rigidbody>().mass = lHandMass;
        rUpperArm.gameObject.GetComponent<Rigidbody>().mass = rUpperArmMass;
        rLowerArm.gameObject.GetComponent<Rigidbody>().mass = rLowerArmMass;
        rHand.gameObject.GetComponent<Rigidbody>().mass = rHandMass;
        lUpperLeg.gameObject.GetComponent<Rigidbody>().mass = lUpperLegMass;
        lLowerLeg.gameObject.GetComponent<Rigidbody>().mass = lLowerLegMass;
        lFoot.gameObject.GetComponent<Rigidbody>().mass = lFootMass;
        rUpperLeg.gameObject.GetComponent<Rigidbody>().mass = rUpperLegMass;
        rLowerLeg.gameObject.GetComponent<Rigidbody>().mass = rLowerLegMass;
        rFoot.gameObject.GetComponent<Rigidbody>().mass = rFootMass;
    }

    void AddConfigurableJoints(Rigidbody rb, Rigidbody ConnectedBody)
    {
        // Adds joints to the ragdoll
        // Makes a character joint
        // copies the componenets to the configurable joint
        // Deletes the character joint      
        CharacterJoint charJoint = rb.gameObject.AddComponent<CharacterJoint>();
        ConfigurableJoint cj = rb.gameObject.AddComponent<ConfigurableJoint>();

        cj.connectedBody = ConnectedBody;
        cj.anchor = charJoint.anchor;
        cj.axis = charJoint.axis;
        cj.secondaryAxis = charJoint.swingAxis;

        cj.xMotion = ConfigurableJointMotion.Locked;
        cj.yMotion = ConfigurableJointMotion.Locked;
        cj.zMotion = ConfigurableJointMotion.Locked;

        cj.angularXMotion = ConfigurableJointMotion.Limited;
        cj.angularYMotion = ConfigurableJointMotion.Limited;
        cj.angularZMotion = ConfigurableJointMotion.Limited;

        cj.lowAngularXLimit = charJoint.lowTwistLimit;
        cj.highAngularXLimit = charJoint.highTwistLimit;
        cj.angularYLimit = charJoint.swing1Limit;
        cj.angularZLimit = charJoint.swing2Limit;
        cj.rotationDriveMode = RotationDriveMode.Slerp;

        // CHANGES THE LIMITS ON THE JOINTS FOR THESE SPECIFIC BODY PARTS
        if(cj.gameObject == lUpperArm.gameObject) ConvertSoftLimits(lUpperArm.gameObject, -177f, 177f, 90f);
        if (cj.gameObject == rUpperArm.gameObject) ConvertSoftLimits(rUpperArm.gameObject, -177f, 177f, 90f);
        if (cj.gameObject == lFoot.gameObject) ConvertSoftLimits(lFoot.gameObject, -177f, 70f, 40f);
        if (cj.gameObject == rFoot.gameObject) ConvertSoftLimits(rFoot.gameObject, -177, 70f, 40f);
        if (cj.gameObject == lUpperLeg.gameObject) ConvertSoftLimits(lUpperLeg.gameObject, -177f, 70f, 40f);
        if (cj.gameObject == rUpperLeg.gameObject) ConvertSoftLimits(rUpperLeg.gameObject, -177f, 70f, 40f);
        Destroy(charJoint);
    }
    //Returns vectors for positioning of the colliders
    private Vector3 ChestToSpineCentre()
    {
        return Vector3.Lerp(UpperArmCentre(), Spine.transform.position, SpineColliderSize);
    }
    private Vector3 HeadToNeckeCentre()
    {
        return Vector3.Lerp(Neck.transform.position, Head.transform.position, SpineColliderSize);
    }
    private Vector3 ChestToNeckCentre()
    {
        return Vector3.Lerp(UpperArmCentre(), Neck.transform.position, SpineColliderSize);
    }
    private Vector3 SpineToHip()
    {
        return Vector3.Lerp(Spine.transform.position, Hip.transform.position, SpineColliderSize);
    }
    private Vector3 UpperArmCentre()
    {
        return Vector3.Lerp(lUpperArm.transform.position, rUpperArm.transform.position, 0.5f);
    }
    // Creates capsule collider
    protected static void CreateCollider(Transform t,
                                         Vector3 startPoint,
                                         Vector3 endPoint,
                                         float lengthOverLap,
                                         float width)

    {
        Vector3 direction = endPoint - startPoint;
        float height = direction.magnitude * (1f + lengthOverLap);
        Vector3 heightAxis = GetAxisVectorToDirection(t, direction);
        if (!t.gameObject.GetComponent<Rigidbody>())
        {
            t.gameObject.AddComponent<Rigidbody>();
        }
        float scaleF = GetScaleF(t);
        CapsuleCollider capsule = t.gameObject.AddComponent<CapsuleCollider>();
        capsule.height = Mathf.Abs(height / scaleF);
        capsule.radius = Mathf.Abs((width * 0.75f) / scaleF);
        capsule.direction = DirectionVector3ToInt(heightAxis);
        capsule.center = t.InverseTransformPoint(Vector3.Lerp(startPoint, endPoint, 0.5f));
        if (t.gameObject.name.Contains("Hand"))
        {

            capsule.isTrigger = true;
        }
    }
    // Creates box collider
    protected static void CreateCollider(Transform t,
                                        Vector3 startPoint,
                                        Vector3 endPoint,
                                        float lengthOverlap,
                                        float width,
                                        float proportionAspect,
                                        Vector3 widthDirection)
    {
        Vector3 direction = endPoint - startPoint;
        float height = direction.magnitude * (1f + lengthOverlap);
        Vector3 heightAxis = GetAxisVectorToDirection(t, direction);
        Vector3 widthAxis = GetAxisVectorToDirection(t, widthDirection);
        if (widthAxis == heightAxis)
        {
            Debug.LogWarning("Width axis = height axis on " + t.name, t);
            widthAxis = new Vector3(heightAxis.y, heightAxis.z, heightAxis.x);
        }
        t.gameObject.AddComponent<Rigidbody>();
        Vector3 heightAdd = Vector3.Scale(heightAxis, new Vector3(height, height, height));
        Vector3 widthAdd = Vector3.Scale(widthAxis, new Vector3(width, width, width));
        Vector3 size = heightAdd + widthAdd;
        if (size.x == 0f) size.x = width * proportionAspect;
        if (size.y == 0f) size.y = width * proportionAspect;
        if (size.z == 0f) size.z = width * proportionAspect;
        BoxCollider box = t.gameObject.AddComponent<BoxCollider>();
        box.size = size / GetScaleF(t);
        box.center = t.InverseTransformPoint(Vector3.Lerp(startPoint, endPoint, 0.5f));
    }
    void AddingJoints()
    {
        // Torso
        if (Spine)
        {
            AddConfigurableJoints(Spine.GetComponent<Rigidbody>(),
                Hip.GetComponent<Rigidbody>());
        }

        if (Chest)
        {
            AddConfigurableJoints(Chest.GetComponent<Rigidbody>(),
                (Spine.GetComponent<Rigidbody>()));
        }
        if (UpperChest != Chest)
        {
            AddConfigurableJoints(UpperChest.GetComponent<Rigidbody>(),
                (Chest.GetComponent<Rigidbody>()));
        }
        // Neck
        AddConfigurableJoints(Neck.GetComponent<Rigidbody>(),
        UpperChest.GetComponent<Rigidbody>());
        // Head
        AddConfigurableJoints(Head.GetComponent<Rigidbody>(),
        Neck.GetComponent<Rigidbody>());
        // Arms
        // Right arm
        AddConfigurableJoints(rUpperArm.GetComponent<Rigidbody>(),
        (UpperChest.GetComponent<Rigidbody>()));
        AddConfigurableJoints(rLowerArm.GetComponent<Rigidbody>(),
        (rUpperArm.GetComponent<Rigidbody>()));
        AddConfigurableJoints(rHand.GetComponent<Rigidbody>(),
        (rLowerArm.GetComponent<Rigidbody>()));
        // Left arm
        AddConfigurableJoints(lUpperArm.GetComponent<Rigidbody>(),
        (UpperChest.GetComponent<Rigidbody>()));
        AddConfigurableJoints(lLowerArm.GetComponent<Rigidbody>(),
        (lUpperArm.GetComponent<Rigidbody>()));
        AddConfigurableJoints(lHand.GetComponent<Rigidbody>(),
        (lLowerArm.GetComponent<Rigidbody>()));
        // LEGS
        // Right Leg
        AddConfigurableJoints(rUpperLeg.GetComponent<Rigidbody>(),
        Hip.GetComponent<Rigidbody>());
        AddConfigurableJoints(rLowerLeg.GetComponent<Rigidbody>(),
        (rUpperLeg.GetComponent<Rigidbody>()));
        AddConfigurableJoints(rFoot.GetComponent<Rigidbody>(),
        (rLowerLeg.GetComponent<Rigidbody>()));
        // Left leg
        AddConfigurableJoints(lUpperLeg.GetComponent<Rigidbody>(),
        Hip.GetComponent<Rigidbody>());
        AddConfigurableJoints(lLowerLeg.GetComponent<Rigidbody>(),
        (lUpperLeg.GetComponent<Rigidbody>()));
        AddConfigurableJoints(lFoot.GetComponent<Rigidbody>(),
        (lLowerLeg.GetComponent<Rigidbody>()));
    }
    protected static float GetScaleF(Transform t)
    {
        Vector3 scale = t.lossyScale;
        return (scale.x + scale.y + scale.z) / 3f;
    }
    protected static int DirectionVector3ToInt(Vector3 dir)
    {
        float dotX = Vector3.Dot(dir, Vector3.right);
        float dotY = Vector3.Dot(dir, Vector3.up);
        float dotZ = Vector3.Dot(dir, Vector3.forward);

        float absDotX = Mathf.Abs(dotX);
        float absDotY = Mathf.Abs(dotY);
        float absDotZ = Mathf.Abs(dotZ);

        int rotatedDirection = 0;
        if (absDotY > absDotX && absDotY > absDotZ) rotatedDirection = 1;
        if (absDotZ > absDotX && absDotZ > absDotY) rotatedDirection = 2;
        return rotatedDirection;
    }
    // Returns the local axis of the Transform that aligns the most with a direction.
    public static Vector3 GetAxisVectorToDirection(Transform t, Vector3 direction)
    {
        return GetAxisVectorToDirection(t.rotation, direction);
    }
    public static Vector3 GetAxisVectorToDirection(Quaternion r, Vector3 direction)
    {
        direction = direction.normalized;
        Vector3 axis = Vector3.right;
        float dotX = Mathf.Abs(Vector3.Dot(Vector3.Normalize(r * Vector3.right), direction));
        float dotY = Mathf.Abs(Vector3.Dot(Vector3.Normalize(r * Vector3.up), direction));
        if (dotY > dotX) axis = Vector3.up;
        float dotZ = Mathf.Abs(Vector3.Dot(Vector3.Normalize(r * Vector3.forward), direction));
        if (dotZ > dotX && dotZ > dotY) axis = Vector3.forward;
        return axis;
    }
    void ConvertSoftLimits(GameObject BodyPart, float LowAngularXlimit, float HighAngularXlimit, float AngularYZLimit)
    {
        // soft joint limits for the shoulders
        // there was alot of stiffness when just converting the character joints to configurable joints
        SoftJointLimit LowAgnularXLimit = new SoftJointLimit();
        LowAgnularXLimit.limit = LowAngularXlimit;
        LowAgnularXLimit.bounciness = 0.0f;
        LowAgnularXLimit.contactDistance = 0.0f;

        SoftJointLimit HighAgnularXLimit = new SoftJointLimit();
        HighAgnularXLimit.limit = HighAngularXlimit;
        HighAgnularXLimit.bounciness = 0.0f;
        HighAgnularXLimit.contactDistance = 0.0f;

        SoftJointLimit AngularYLimit = new SoftJointLimit();
        AngularYLimit.limit = AngularYZLimit;
        AngularYLimit.bounciness = 0.0f;
        AngularYLimit.contactDistance = 0.0f;

        SoftJointLimit AngularZLimit = new SoftJointLimit();
        AngularZLimit.limit = AngularYZLimit;
        AngularZLimit.bounciness = 0.0f;
        AngularZLimit.contactDistance = 0.0f;

        BodyPart.GetComponent<ConfigurableJoint>().lowAngularXLimit = LowAgnularXLimit;
        BodyPart.GetComponent<ConfigurableJoint>().highAngularXLimit = HighAgnularXLimit;
        BodyPart.GetComponent<ConfigurableJoint>().angularYLimit = AngularYLimit;
        BodyPart.GetComponent<ConfigurableJoint>().angularZLimit = AngularZLimit;
        BodyPart.GetComponent<ConfigurableJoint>().rotationDriveMode = RotationDriveMode.Slerp;
    }
}
