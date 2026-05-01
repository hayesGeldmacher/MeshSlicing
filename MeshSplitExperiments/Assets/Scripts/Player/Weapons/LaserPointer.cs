using UnityEngine;

public class LaserPointer : MonoBehaviour
{

    [SerializeField] private bool isActive = false;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }
}
