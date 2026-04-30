using UnityEngine;

public class GunBase : MonoBehaviour
{

    [Header("Firing Fields")]
    [SerializeField] protected float cooldownTimer;
    private float cooldownCurrent;
    [SerializeField] protected float range;
    [SerializeField] protected float rangeFalloff; //how far does damage deteriorate outside range
    public bool canFire = true;

    [Header("Damage Fields")]
    [SerializeField] protected float damage;

    [Header("Aiming Fields")]
    [SerializeField] private Transform gunParent;
    [SerializeField] private Transform barrellPoint;
    [SerializeField] protected bool aiming = false;

    [SerializeField] protected float aimSensitivityX;
    [SerializeField] protected float aimSensitivityY;

    [SerializeField] protected Vector2 aimBoundsX;
    [SerializeField] protected Vector2 aimBoundsY;

    [SerializeField] protected LayerMask fireMask;

    protected float aimInputX;
    protected float aimInputY;

    protected float aimRotationX = 0;
    protected float aimRotationY = 0;

    [Header("Animation Fields")]
    [SerializeField] protected Animator gunAnim;
    [SerializeField] protected AudioSource fireSource;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        
    }

    protected virtual void PlayFireAnimation()
    {
        gunAnim.SetTrigger("fire");
    }

    protected virtual void Fire()
    {
        Debug.Log("Fired a gun!");
        PlayFireAnimation();

        Vector3 forward = barrellPoint.up;
        Debug.DrawRay(barrellPoint.position, forward * 100, Color.red, 20);

        RaycastHit hit;
        if(Physics.Raycast(barrellPoint.position, forward, out hit, range, fireMask))
        {
            GameObject hitObject = hit.transform.gameObject;

            Debug.Log("Shot Collided with object!");
            if (hitObject.TryGetComponent<Hurtable>(out Hurtable hurt))
            {
                hurt.TakeDamage(damage);
            }
            else
            {
                Debug.Log("Shot object has no hurtable component");
            }
        }
    }

    protected virtual bool CheckCooldown()
    {
        if (cooldownCurrent < 0.0f) { return true; }
        cooldownCurrent -= Time.deltaTime;
        return false;
    }

    protected virtual void FireUpdate()
    {
        if (Input.GetButtonDown("Fire1")) { 
            Fire();
            cooldownCurrent = cooldownTimer;
        }
    }

    protected virtual void EnterAim(bool enter)
    {
        if (enter)
        {
            aiming = true;
            Debug.Log("Entered Aim Mode");
            PController.instance.SetAimFrozen(true);
            CamController.instance.SetAimFrozen(true);
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            aiming = false;
            Debug.Log("Exited Aim Mode");
            PController.instance.SetAimFrozen(false);
            CamController.instance.SetAimFrozen(false);
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    protected virtual void AimUpdate()
    {
        bool grounded = PController.instance.GetGrounded();

        if (Input.GetButton("Fire2") && !aiming && grounded) { EnterAim(true); }
        else if (Input.GetButtonUp("Fire2") || !grounded) { EnterAim(false); }

        if (aiming)
        {
            aimInputX = Input.GetAxis("ControllerX") * aimSensitivityX;
            aimInputY = Input.GetAxis("ControllerY") * aimSensitivityY;

            aimRotationX -= aimInputY;
            aimRotationY += aimInputX;

            aimRotationX = Mathf.Clamp(aimRotationX, aimBoundsX.x, aimBoundsX.y);
            aimRotationY = Mathf.Clamp(aimRotationY, aimBoundsY.x, aimBoundsY.y);

            gunParent.transform.localRotation = Quaternion.Euler(aimRotationX, aimRotationY, 0);
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {

        AimUpdate();
        if (CheckCooldown() && aiming) { FireUpdate(); }

    }
}
