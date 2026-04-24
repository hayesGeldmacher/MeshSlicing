using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PController : MonoBehaviour
{

    [Header("Player Movement")]
    
    //movement speeds
    [SerializeField] private float walkSpeed;
    [SerializeField] private float runSpeed;
    private List<float> moveSpeeds = new List<float>(); //list of speeds, list for quick access

    //player input fields, not visible to inspector
    [SerializeField] private float inputX; //receives player input
    [SerializeField] private float inputZ; //receives player input
    [SerializeField] private bool isMovingInput = false; //is player moving

    [HideInInspector] public enum enMoveState {STILL, WALKING, RUNNING}
    public enMoveState moveState; //tracks current player state

    private CharacterController controller; //private movement component
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = transform.GetComponent<CharacterController>();
        moveSpeeds.Add(0);
        moveSpeeds.Add(walkSpeed);
        moveSpeeds.Add(runSpeed);
    }

    //standard movement update
    private void MoveUpdate()
    {

        float speed = moveSpeeds[(int)moveState];

        Vector3 moveDirection = (transform.right * inputX + transform.forward * inputZ);

        //controller.move is how the character actually moves - always multiply by Time.deltaTime so physics work correctly!
        controller.Move(moveDirection * speed * Time.deltaTime);
        //_controller.Move(_velocity * Time.deltaTime);

    }

    private void CheckMoveState()
    {
        inputX = Input.GetAxis("Horizontal");
        inputZ = Input.GetAxis("Vertical");

        isMovingInput = ((Mathf.Abs(inputX + inputZ)) >= 0.1f) ? true : false;
        if (isMovingInput)
        {
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
            case enMoveState.WALKING:
                //player is moving
                MoveUpdate(); break;
            case enMoveState.RUNNING:
                //player is runnign
                MoveUpdate(); break;
        }
    }
}
