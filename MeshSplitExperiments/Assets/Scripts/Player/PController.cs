using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PController : MonoBehaviour
{
    [Header("Player Movement")]
    
    //movement speeds
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    [SerializeField] private float aimSpeed;
    private List<float> moveSpeeds = new List<float>(); //list of speeds, list for quick access

    //player input fields, not visible to inspector
    [SerializeField] private float inputX; //receives player input
    [SerializeField] private float inputZ; //receives player input
    [SerializeField] private bool isMovingInput = false; //is player moving
    [SerializeField] private float storedSpeed; //stored movement from last frame
    [SerializeField] private float storedDecayRate; //how fast does stored movement decay

    [HideInInspector] public enum enMoveState {STILL, AIMING, WALKING, RUNNING}
    public enMoveState moveState; //tracks current player state

    private CharacterController controller; //private movement component

    [Header("Jumping")]
    [SerializeField] private Transform groundCheck;
    private float groundCheckRange = 0.7f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private bool isGrounded = true;
    [SerializeField] private float jumpTimer = 0.4f;
    [SerializeField] private float jumpCoolDown = 0.0f;

    [SerializeField] private float jumpStrength;
    [SerializeField] private float gravityStrength;
    [SerializeField] private float velocity;
    [SerializeField] private float minVelocity; //the strongest it can pull down

    private bool aimFrozen = false;

    #region Singleton

    public static PController instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of playercontroller present in scene");
            return;
        }

        instance = this;
    }

    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = transform.GetComponent<CharacterController>();
        moveSpeeds.Add(0);
        moveSpeeds.Add(aimSpeed);
        moveSpeeds.Add(walkSpeed);
        moveSpeeds.Add(runSpeed);
    }

    private void GroundedUpdate()
    {
        RaycastHit hit;
        isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, out hit, groundCheckRange, groundMask);
    }

    private void GravityUpdate()
    {
        velocity -= gravityStrength * Time.deltaTime;
        if (velocity < minVelocity) { velocity = minVelocity; }
        Vector3 downMovement = Vector3.up * velocity;
        controller.Move(downMovement * Time.deltaTime);

        jumpTimer -= Time.deltaTime;
    }

    private void Jump()
    {
        velocity = jumpStrength;
        jumpCoolDown = jumpTimer;
    }

    private void JumpUpdate()
    {
        if (Input.GetButtonDown("Jump") && jumpCoolDown <= 0.0f) { Jump(); }
    }

    //standard movement update
    private void MoveUpdate()
    {
        if (isGrounded)
        {
            storedSpeed = moveSpeeds[(int)moveState];
        }
        else
        {
            storedSpeed -= storedDecayRate * Time.deltaTime;
            if (storedSpeed < 0.0f) { storedSpeed = 0.0f; }
        }

        

        Vector3 moveDirection = (transform.right * inputX + transform.forward * inputZ);
        moveDirection.Normalize();
        //controller.move is how the character actually moves - always multiply by Time.deltaTime so physics work correctly!
        controller.Move(moveDirection * storedSpeed * Time.deltaTime);
    }

    private void CheckMoveState()
    {
      
         inputX = Input.GetAxisRaw("Horizontal");
         inputZ = Input.GetAxisRaw("Vertical");

        isMovingInput = ((Mathf.Abs(inputX) + Mathf.Abs(inputZ)) >= 0.1f) ? true : false;
        if (isMovingInput)
        {
            if (aimFrozen) { moveState = enMoveState.AIMING; return; }

            bool isHoldingRun = (Input.GetButton("run")) ? true : false;
            if (isHoldingRun) { moveState = enMoveState.RUNNING; }
            else { moveState = enMoveState.WALKING; }
        }
        else { moveState = enMoveState.STILL; }
    }

    // Update is called once per frame
    void Update()
    {
        //get current state of movement
        CheckMoveState();

        switch (moveState)
        {
            case enMoveState.STILL:
                //player is still
                break;
            case enMoveState.AIMING:
                MoveUpdate(); break;
            case enMoveState.WALKING:
                //player is moving
                MoveUpdate(); break;
            case enMoveState.RUNNING:
                //player is runnign
                MoveUpdate(); break;
        }

        GroundedUpdate();
        if (isGrounded) { JumpUpdate(); }
        GravityUpdate();
    }

    public float GetXMovement()
    {
        return inputX;
    }
    
    public float GetZMovement()
    {
        return inputZ;
    }

    public void SetAimFrozen(bool frozen)
    {
        aimFrozen = frozen;
    }

    public bool GetGrounded()
    {
        return isGrounded;
    }

    //rigidbody collisions
    void OnControllerColliderHit(ControllerColliderHit hit)
    {


        Rigidbody body = hit.collider.attachedRigidbody;

        //no rigidbody 
        if(body == null || body.isKinematic) { return; }

        Debug.Log("COllided");
        Vector3 direction = transform.position - hit.gameObject.transform.position;
        //dont' wanna push objects below us
        //if(hit.moveDirection.y < 0.8f) { Debug.Log("Hit direction too low!");  return; }

        //calculate push direction from move direction
        //only push obects to the sides, never up and down
        var pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        body.linearVelocity = pushDir * 5;
    }
}
