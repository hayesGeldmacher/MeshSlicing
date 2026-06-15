using UnityEngine;

public class FadeImage : MonoBehaviour
{

    #region Singleton

    public static FadeImage instance;

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

    private Animator fadeAnim;
    private void Start()
    {
        fadeAnim = transform.GetComponent<Animator>();
    }

    public void CallBlack()
    {
        fadeAnim.SetTrigger("black");
    }

    public void CallWhite() 
    {
        fadeAnim.SetTrigger("white");    
    }
}
