using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

//Diferent states player can be in
public enum CharacterState
{
    IDLE,
    WALKING,
    RUNNING,
    FALLING
}
public class PlayerController : MonoBehaviour
{
    //Different state of player
    public CharacterState state;
    Rigidbody rb;
    SlaveControl slavecontroller;
    Camera Charcamera;
    Transform Master;
    Transform Slave;
    Animator Animation;
    //show debug lines
    public bool debug;
    //used to get the origin transform of the character
    public GameObject Armature, SlaveArmature;
    //Walk and Run speed//
    [SerializeField]
    float MovementSpeed = 75.0f;
    //How quick the rotation of the player is
    [SerializeField]
    float TurnTime = 0.40f;
    //How far from the ground the master is
    [Range(0.0f, 10.0f)]
    [SerializeField]
    float GroundDis = 0.1f;
    //How fast the master returns to the ragdoll
    [SerializeField]
    float Pushbackspeed = 7.0f;
    //How far the master skeleton can go from the ragdoll when collinding with something
    [SerializeField]
    float MaxDisFromRagdoll = 2.0f;
    //Accelerates player in animator
    [SerializeField]
    float Acceleration = 0.1f;
    //Decelerates player in animator
    [SerializeField]
    float Deceleration = 0.5f;
    //To avoid jitteryness when repositioning the player 
    [SerializeField]
    float heightPadding = 0.05f;
    //increase or decrease depnding on how high an angle you want the player to walk up
    [SerializeField]
    float maxGroundAngle = 120;
    //How far beneath the player the ground is before they enter falling state
    [Range(0.0f, 20.0f)]
    [SerializeField]
    float FallingDist = 0.1f;
    [SerializeField]
    float StandUpTime = 0.0f;
    [SerializeField]
    float TimeToStandUp = 6.0f;
    //Closest distance player is allowed to walls
    [Range(0.0f, 20.0f)]
    [SerializeField]
    float ClosestWallDistance = 2.0f;
    //Layermasks
    [SerializeField]
    public LayerMask GroundMask, WallMask;
    //Vectors and rotations
    Vector3 MoveDir;
    Quaternion target;
    RaycastHit HitInfo;
    //floats 
    float CamAngle;
    float tiltAroundZ;
    float tiltAroundX;
    float CurrentTurnVelocity;
    float Velocity = 0.0f;
    float tiltAngle = 30.0f;
    float groundAngle;
    float WalkingTilt = 80.0f;
    float RunningTilt = 100.0f;
    [SerializeField]
    float TimeToStartWalking = 15.0f;
    float EndStandUpTime = 0.0f;
    //Bools for when grounded and when falling
    [System.NonSerialized]
    public bool isGrounded;
    [System.NonSerialized]
    public bool WalkEnabled = true;
    bool Falling = false;
    bool wasFalling = false;
    bool AlreadyPlayedAnimation = false;
    //Animator velocity
    int VelocityHash;
    // Start is called before the first frame update
    void Start()
    {
        //Increase performance
        VelocityHash = Animator.StringToHash("Velocity");
        //Setsup the Player
        HumanoidSetUp setUp = this.GetComponentInParent<HumanoidSetUp>();
        Charcamera = setUp.characterCamera;
        Master = setUp.masterRoot;
        Slave = setUp.slaveRoot;
        Animation = setUp.anim;      
        slavecontroller = setUp.slaveController;
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }
    // Called every fixed framerate frame
    private void FixedUpdate()
    {
        state = GetState();
        Vector3 Dir = GetDirectionInput();
        CalculateForward();
        CalculateGroundAngle();
        FallingCheck();
        CheckGround();
        //Checks for wall or surface and slows down the player
        WallCheck(MoveDir);
        WallCheck((MoveDir + transform.right) / 2);
        WallCheck((MoveDir + -transform.right) / 2);
        ApplyGravity();
        DrawDebugLines();
        float CamY = Charcamera.transform.eulerAngles.y;
        CamAngle = CalculateCharAngle(Dir, CamY);
        SetCharacterRotation(CamAngle);
        SetAnimation();
        MoveCharacter(MoveDir);
    }
    // Gets the state of the character
    private CharacterState GetState()
    {
        bool WSAD = Input.GetKey(KeyCode.W) ||
            Input.GetKey(KeyCode.S) ||
            Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.D);
        //Falling state
        if (!isGrounded && Falling && slavecontroller.animFollow.isAlive) // Player has to be alive to play falling and standing up animation
        {                                                                 // when the player is dead it starts the death timer which can be changed in the inspector under the SlaveControl script
            this.GetComponent<NewSimpleIK>().enabled = false;
            this.GetComponent<HandIK>().enabled = true;
            wasFalling = true;
            WalkEnabled = false;
            AlreadyPlayedAnimation = false;
            return CharacterState.FALLING;
        }
        //Running state
        else if (WSAD && Input.GetKey(KeyCode.LeftShift) && WalkEnabled)
        {
            //Sets Hand ik to false and weight of the rig to zero when running
            this.GetComponent<NewSimpleIK>().enabled = true;
            this.GetComponent<HandIK>().enabled = false;
           //GameObject.Find("RightElbowMover").GetComponent<RigWeightChanger>().RightElbowWeight = 0;
            //GameObject.Find("LeftElbowMover").GetComponent<RigWeightChanger>().LeftElbowWeight = 0;
            return CharacterState.RUNNING;
        }
        //Walking state
        //Checks to see if the player is Upright
        else if (WSAD&&WalkEnabled)
        {
            this.GetComponent<HandIK>().enabled = true;
            this.GetComponent<NewSimpleIK>().enabled = true;
            return CharacterState.WALKING;
        }

        else
        {
            //Checks if the player was just in a falling state
            //Calculates time for stand up animation to take place
            //Can be changed in the inspector
            if (wasFalling)
            {                
                //Begins the standing up animation           
                StandUpTime += Time.deltaTime;             
                if (StandUpTime > TimeToStandUp)
                {
                     if (!AlreadyPlayedAnimation)
                    {
                        AlreadyPlayedAnimation = true;
                        this.GetComponent<HandIK>().enabled = true;
                        this.GetComponent<NewSimpleIK>().enabled = true;
                        slavecontroller.StandUp();
                        Animation.SetTrigger("StandingUp");
                        StandUpTime = 0.0f;
                    }
                    StandUpTime = 0.0f;
                }
                //When the animation has finished the player is now upright, therefor can start moving again
                EndStandUpTime += Time.deltaTime;
                if (EndStandUpTime > TimeToStartWalking)
                {
                    StandUpTime = 0.0f;
                    EndStandUpTime = 0.0f;
                    WalkEnabled = true;
                    wasFalling = false;
                }
            }           
            return CharacterState.IDLE;
        }
    }
    // Gets the direction Input and camera angle
    private Vector3 GetDirectionInput()
    {
        //Leans the player into the direction they are moving
        tiltAroundZ = Input.GetAxis("Horizontal") * tiltAngle;
        tiltAroundX = Input.GetAxis("Vertical") * tiltAngle;
        //Camera Angle
        target = Quaternion.Euler(tiltAroundX, CamAngle, -tiltAroundZ);
        return new Vector3(tiltAroundZ, 0f, tiltAroundX).normalized;
    }
    // Calculates the characters angle
    private float CalculateCharAngle(Vector3 direction, float cameraAngleY)
    {
        //Calculates the angle of the character
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraAngleY;
        float characterAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref CurrentTurnVelocity, TurnTime);

        return characterAngle;
    }
    // Player movement
    private void MoveCharacter(Vector3 moveDirection)
    {
        //Disable movement if the ground angle is too high
        //Can be changed in inspector
        if (groundAngle >= maxGroundAngle) return;
        //Adds in the pushback movement from the master to the slave
        moveDirection = moveDirection.normalized;
        Vector3 pushBackMovement = CalculatePushBackMovement();   // this vector pushes static animation back towards ragdoll if it's too far away
        Vector3 movement = moveDirection + pushBackMovement;
        //Switches through the different states of the character
        //Movement of the player
        switch (state)
        {
            case CharacterState.FALLING:
                //Master gets pushed to the slave position
                transform.position += pushBackMovement * Pushbackspeed * Time.fixedDeltaTime;
                break;
            case CharacterState.RUNNING:            
                    rb.AddForce(movement * Velocity * MovementSpeed);
                    //This makes the player go into falling state
                    //This is done by design
                    //Animation can be changed if desired
                    BigJump();
                break;
            case CharacterState.WALKING:
                    rb.AddForce(movement * Velocity * MovementSpeed);
                    Jump();              
                break;
            case CharacterState.IDLE:
                //Inhibits jerky movement
                //the push back is applied here for when we are idle after falling state
                //So that the master and slave are alligned for the standing up animation
                pushBackMovement.y = 0;
                rb.AddForce(movement *  Velocity * MovementSpeed);
                transform.position += pushBackMovement * Pushbackspeed * Time.fixedDeltaTime;
                Jump();
                break;
            default:
                break;
        }
    }
    // Player rotation
    private void SetCharacterRotation(float angleY)
    {
        //Character rotation
        transform.rotation = Quaternion.Euler(0, angleY, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 5);

    }
    // Player Animation
    private void SetAnimation()
    {
        //Switches through the different states
        //Sets the Velocity for each state which is a variable within the animator in Unity
        //if n > 11 the player is running
        //if n < 11, the player is walking 
        //0 is idle
        //slavecontroller.falling() sets the tourque of each joint to zero, giving the ragdoll effect when falling
        switch (state)
        {   
            case CharacterState.FALLING:              
                slavecontroller.Falling();
                Velocity = 0.0f;
                break;
            case CharacterState.RUNNING:
                //Tilt in the direction the player is moving
                tiltAngle = RunningTilt;
                float RUNBOOSTER = 2f;
                Velocity += Time.deltaTime * Acceleration * RUNBOOSTER;
                if (Velocity >= 20)
                {
                    Velocity = 20;                   
                }
                break;
            case CharacterState.WALKING:
                tiltAngle = WalkingTilt;              
                if (Velocity <= 9)
                {
                    Velocity += Time.deltaTime * Acceleration;
                }
                else if(Velocity >9)
                {
                    Velocity -= Time.deltaTime * Deceleration;
                }
                break;
            case CharacterState.IDLE:
                Velocity -= Time.deltaTime * Deceleration;
                if (Velocity < 0.0f)
                {
                    Velocity = 0.0f;
                }
                break;
            default:
                break;
        }
        Animation.SetFloat(VelocityHash, Velocity);
    }
    // Calculates the vector that pushes back the master to the slave
    private Vector3 CalculatePushBackMovement()
    {
        //Gets the vector between the slave and master
        //Interpolates smoothly bewteen the two distances
        Vector3 slaveToMasterVector = (Slave.position - Master.position);
        float distance = slaveToMasterVector.magnitude;

        float ratio = Mathf.Clamp(distance / MaxDisFromRagdoll, 0, 1);
        float magnitude = Mathf.SmoothStep(0, 1, ratio);
        Vector3 moveVector = Vector3.ClampMagnitude(slaveToMasterVector, magnitude);
        return moveVector;
    }
    // Calculates the forward vector of the player
    public void CalculateForward()
    {
        if (!isGrounded && Falling)
        {
            MoveDir = transform.forward;
            return;
        }
        //HitInfo is the info from the raycast shooting from the hips to the ground
        MoveDir = Vector3.Cross(HitInfo.normal, -transform.right);       
    }
    //Calculates the ground angle the player is standing on
    public void CalculateGroundAngle()
    {
        if (!isGrounded && Falling)
        {
            groundAngle = 90;
            return;
        }
        //Gets the ground angle
        groundAngle = Vector3.Angle(HitInfo.normal, transform.forward);
    }
    // Gravity
    public void ApplyGravity()
    {
        //Gravity
       if (!isGrounded)
       {
            transform.position += Physics.gravity * Time.deltaTime;
        }
    }
    // Checks if the player is standing on the ground
    public void CheckGround()
    {
        //Shoots ray down from player to the ground
        if (Physics.Raycast(Armature.transform.position, -Vector3.up,
            out HitInfo, GroundDis + heightPadding, GroundMask))
        {   //pushes player back up if distance between ground and player is smaller than the ground distance
            //Height padding is so that the pushback is smooth
            if (Vector3.Distance(Armature.transform.position, HitInfo.point) < GroundDis)
            {
                transform.position = Vector3.Lerp(transform.position,
                   transform.position + Vector3.up * GroundDis, .5f * Time.deltaTime);
            }
            if (HitInfo.transform.tag == "Stairs")
            {
                MoveDir = (transform.forward + transform.up) / 2;
            }
            isGrounded = true;
        }
        else
             isGrounded = false;
    }
    // Debug Lines
    public void DrawDebugLines()
    {
        if (!debug) return;
        //Debug lines
        //Can be turned on and off from the inspector
        Debug.DrawLine(transform.position, transform.position + MoveDir, Color.blue);
        Debug.DrawLine(Armature.transform.position, transform.position - Vector3.up * GroundDis, Color.green);
        Debug.DrawLine(Armature.transform.position + Vector3.up * 2, Armature.transform.position + Vector3.down * FallingDist, Color.red);
    }
    // Checks if we are in a falling state
    public void FallingCheck()
    {
        //Shoots ray down to check if player is to enter into the falling state
        RaycastHit info;
        if (Physics.Raycast(Armature.transform.position, -Vector3.up,
            out info, FallingDist, GroundMask))
        {
            Falling = false;
        }
        else
            Falling = true;
    }
    // Shoots rays to check for walls
    public void WallCheck(Vector3 Direction)
    {
        //Shoots rays to check if the player is close to a wall
        RaycastHit WallHitInfo;
        if (Physics.Raycast(Armature.transform.position, Direction, out WallHitInfo, ClosestWallDistance, WallMask))
        {
            Velocity -= Deceleration;
            if (Velocity <= 0)
            {
                Velocity = 0;
            }
        }
        if (debug)
            Debug.DrawLine(Armature.transform.position, Armature.transform.position + Direction * ClosestWallDistance, Color.red);
    }
    // Jump
    public void Jump()
    {
        // return if walk is still not enabled
        if (!WalkEnabled) return;
        if (!isGrounded) return;
        //Sets jump animation
        if (Input.GetKeyDown(KeyCode.Space))
        {                 
            Animation.SetTrigger("Jumping");
        }
    }
    // Big Jump
    public void BigJump()
    {
        // dont jump if walk is not enabled
        if (!WalkEnabled) return;
        if (!isGrounded) return;
        //Sets BigJump animtion
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Animation.SetTrigger("BigJumping");
        }
    }

}