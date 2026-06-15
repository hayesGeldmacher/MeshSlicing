using UnityEngine;

public class PlayerArm : MonoBehaviour
{

    [SerializeField] private Transform handTarget;
    [SerializeField] private Vector2 targetLimitsX;
    [SerializeField] private Vector2 targetLimitsY;

    [SerializeField] private float inputX;
    [SerializeField] private float inputY;
   
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
         inputX = Input.GetAxisRaw("Horizontal");
         inputY = Input.GetAxisRaw("Vertical");
       // private Vector2 addTarget = (0, 0);
       // handTarget.position += addTarget;

    }
}
