using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

public class CamController : MonoBehaviour
{

    [Header("Look Fields")]
    [SerializeField] private float sensitivityX;
    [SerializeField] private float sensitivityY;
    [SerializeField] private float aimMult = 0.2f;
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
    [SerializeField] private CinemachineCamera cineCam;
    [SerializeField] private Transform cineCamHolder;
    [SerializeField] private Camera armCam;

    [Header("Aiming Fields")]
    [SerializeField] private float standardFOV;
    [SerializeField] private float aimingFOV;
    [SerializeField] private float FOVchangeSpeed;
    [SerializeField] private float t;
    [SerializeField] private float previousFOV;
    [SerializeField] private float targetFOV;
    [SerializeField] private bool isAiming;
    [SerializeField] private bool isChangingFOV;


    private bool aimFrozen = false;

    #region Singleton

    public static CamController instance;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of camera controller present in scene!");
            return;
        }

        instance = this;
    }

    #endregion

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

        if (aimFrozen) { mouseX *= aimMult; mouseY *= aimMult; }
    
        if (isChangingFOV) { ChangeFOV(); }
        movingLook = (Mathf.Abs(mouseX) + Mathf.Abs(mouseY) >= 0.1f) ? true : false;

        if(movingLook && canLook) { LookUpdate(mouseX, mouseY); }
        ZSwayUpdate();
        XSwayUpdate();


        cineCamHolder.transform.localRotation = Quaternion.Euler(rotationX, 0, rotationZ);

        //This rotates the camera holder up and down when the player walks forward or backward
        cameraHolder.localRotation = Quaternion.Euler(rotationForward, 0f, 0f);

    }


    private void ChangeFOV()
    {
        t += Time.deltaTime * FOVchangeSpeed;
        float currentFOV = Mathf.Lerp(previousFOV, targetFOV, t);
        cineCam.Lens.FieldOfView = currentFOV;
        armCam.fieldOfView = currentFOV;
        float difference = Mathf.Abs(currentFOV - targetFOV);
        if (difference <= 0.1f) { isChangingFOV = false; }
    }

    public void SetAimFrozen(bool frozen)
    {
        aimFrozen = frozen;
        isAiming = frozen;

        previousFOV = cineCam.Lens.FieldOfView;
        isChangingFOV = true;
        t = 0;
        targetFOV = isAiming ? aimingFOV : standardFOV;
    }

    
}
