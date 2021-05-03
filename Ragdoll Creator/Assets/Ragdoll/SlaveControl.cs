using UnityEngine;
using System;


public enum RagdollState
{
    FOLLOWING_ANIMATION,
    LOOSING_STRENGTH,
    GAINING_STRENGTH,
    DEAD,
    FALLINGSTANDING
}


public class SlaveControl : MonoBehaviour
{
    // RAGDOLL STATE
    public RagdollState state;

    // PARAMETERS
    [SerializeField]
    private float looseStrengthLerp = 1.0f;
    [SerializeField]
    private float gainStrengthLerp = 0.00005f;
    [SerializeField]
    private float minContactForce = 0.1f;
    [SerializeField]
    private float minContactTorque = 0.1f;
    [SerializeField]
    private float deadTime = 4.0f;

    //VARIABLES
    public ConfigurableJoints animFollow;
    private float maxTorqueCoefficient;
    private float maxForceCoefficient;
    [SerializeField]
    private float currentDeadStep;
    private float currentStrength;
    private GameObject Master;
    [NonSerialized] public int currentNumberOfCollisions;
    // Start is called before the first frame update.
    void Start()
    {
        HumanoidSetUp setUp = this.GetComponentInParent<HumanoidSetUp>();
        animFollow = setUp.animFollow;
        maxForceCoefficient = animFollow.forceCoefficient;
        maxTorqueCoefficient = animFollow.torqueCoefficient;
        currentNumberOfCollisions = 0;
        currentDeadStep = deadTime;
        currentStrength = 1.0f;
        Master = GameObject.Find("Master");
    }
    // Unity method for physics update.
    void FixedUpdate()
    {
        // Apply animation following
        animFollow.FollowAnimation();
        state = GetRagdollState();
        switch (state)
        {
            case RagdollState.DEAD:
                currentDeadStep += Time.fixedDeltaTime;
                if (currentDeadStep >= deadTime)
                    ComeAlive();
                break;
            case RagdollState.FALLINGSTANDING:
                currentDeadStep += Time.fixedDeltaTime;
                break;
            case RagdollState.LOOSING_STRENGTH:
                LooseStrength();
                break;

            case RagdollState.GAINING_STRENGTH:
                GainStrength();
                break;

            case RagdollState.FOLLOWING_ANIMATION:
                break;

            default:
                break;
        }
    }
    private RagdollState GetRagdollState()
    {
        if (!animFollow.isAlive)
        {
            return RagdollState.DEAD;
        }
        else if(animFollow.falling)
        {
            return RagdollState.FALLINGSTANDING;
        }
        else if (currentNumberOfCollisions != 0)
        {
            return RagdollState.LOOSING_STRENGTH;
        }
        else if (currentStrength < 1)
        {
            return RagdollState.GAINING_STRENGTH;
        }
        else
        {
            return RagdollState.FOLLOWING_ANIMATION;
        }
    }

    private void LooseStrength()
    {
        currentStrength -= looseStrengthLerp * Time.fixedDeltaTime;
        currentStrength = Mathf.Clamp(currentStrength, 0, 1);
        InterpolateStrength(currentStrength);
    }

    private void GainStrength()
    {
        currentStrength += gainStrengthLerp *  Time.fixedDeltaTime;
        currentStrength = Mathf.Clamp(currentStrength, 0, 1);
        InterpolateStrength(currentStrength);
    }

    private void InterpolateStrength(float ratio)
    {
        animFollow.forceCoefficient = Mathf.Lerp(minContactForce, maxForceCoefficient, ratio);
        animFollow.torqueCoefficient = Mathf.Lerp(minContactTorque, maxTorqueCoefficient, ratio);
    }

    [ContextMenu("Die")]
    public void Die()
    {
        animFollow.isAlive = false;
        currentDeadStep = 0;       
        ResetForces();
    }

    [ContextMenu("Come alive")]
    public void ComeAlive()
    {
        animFollow.falling = false;
        animFollow.isAlive = true;
        Master.GetComponent<PlayerController>().WalkEnabled = true;
    }
    [ContextMenu("Fall")]
    public void Falling()
    {  
        animFollow.falling = true;
        ResetForces();
    }
    [ContextMenu("Stand Up")]
    public void StandUp()
    {
        animFollow.falling = false;
        animFollow.isAlive = true;
    }
    // Sets animation following forces to zero. After calling this method, ragdoll will gradually regain strength.
    [ContextMenu("Reset forces")]
    private void ResetForces()
    {
        animFollow.forceCoefficient = 0f;
        animFollow.torqueCoefficient = 0f;
        currentStrength = 0;
    }
}

