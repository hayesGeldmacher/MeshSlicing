using UnityEngine;
using System.Collections.Generic;

public class CamController : MonoBehaviour
{

    [Header("Look Fields")]
    [SerializeField] private float sensitivityX;
    [SerializeField] private float sensitivityY;
    private bool movingLook = false;
    private bool canLook = true;

    private float rotationX, rotationZ = 0;
    private float rotationForward; 
    private float moveX, moveY;
    [Header("Sway Fields")]
    [SerializeField] private float swayMaxX = 1.0f;
    [SerializeField] private float swayMaxZ = 1.0f;
    [SerializeField] private float swaySpeedX = 1.0f;
    [SerializeField] private float swaySpeedZ = 1.0f;
    private float targetZ, targetX = 0.0f;
    public float tz = 1.0f, tx = 1.0f;
    private float oldTargetZ, oldTargetX = 0.0f;
    private float targetLastFrameZ, targetLastFrameX = 0.0f;


    [Header("Assign Fields")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private PController playerController;
    [SerializeField] private Transform cameraHolder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //set cursor to invisible and locked at game start
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LookUpdate(float mouseX, float mouseY)
    {
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        //This rotates the player's body side to side when aiming with the camera
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void ChangeSway(bool isX, float target)
    {
        if (isX)
        {
            tx = 0.0f;
            oldTargetX = target;
        }
        else
        {
            tz = 0.0f;
            oldTargetZ = target;
        }
    }

    private void XSwayUpdate() {

        targetX = 0.0f;
        float inputZ = playerController.GetZMovement();

        if (inputZ >= 0.1f) { targetX = swayMaxX; }
        else if (inputZ <= -0.1f) { targetX = -swayMaxX; }

        if (targetX != targetLastFrameX) { ChangeSway(true, rotationForward); }
        if (tx <= 1.0f)
        {
            float speed = swaySpeedX;
            float distance = Mathf.Abs(targetX - rotationX);
            if (targetX == 0.0f) { speed *= 0.7f; }
            tx += Time.deltaTime * speed;

            rotationForward = Mathf.Lerp(oldTargetX, targetX, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, tx)));
        }

        targetLastFrameX = targetX;

    }

    private void ZSwayUpdate()
    {
        targetZ = 0.0f;
        float inputX = playerController.GetXMovement();

        if(inputX >= 0.1f) { targetZ = -swayMaxZ; }
        else if(inputX <= -0.1f) { targetZ = swayMaxZ; }

        if(targetZ != targetLastFrameZ) { ChangeSway( false, rotationZ); }
        if(tz <= 1.0f)
        {
            float speed = swaySpeedZ;
            if(targetZ == 0.0f) { speed *= 0.7f; }
            tz += Time.deltaTime * speed;
           
            rotationZ = Mathf.Lerp(oldTargetZ, targetZ, Mathf.SmoothStep(0.0f, 1.0f, Mathf.SmoothStep(0.0f, 1.0f, tz)));
        }

        targetLastFrameZ = targetZ;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("ControllerX") * sensitivityX;
        float mouseY = Input.GetAxis("ControllerY") * sensitivityY;

        movingLook = (Mathf.Abs(mouseX) + Mathf.Abs(mouseY) >= 0.1f) ? true : false;

        if(movingLook && canLook) { LookUpdate(mouseX, mouseY); }
        ZSwayUpdate();
        XSwayUpdate();


        transform.localRotation = Quaternion.Euler(rotationX, 0, rotationZ);

        //This rotates the camera holder up and down when the player walks forward or backward
        cameraHolder.localRotation = Quaternion.Euler(rotationForward, 0f, 0f);

    }
}
