using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class CutsceneIntro : MonoBehaviour
{
    [Header("Animation Fields")]
    [SerializeField] private Animator screenAnim; //the background idle beach anim
    [SerializeField] private Animator textAnim; //the text fading in or out

    [SerializeField] public List<Segment> segments = new List<Segment>();
    [SerializeField] private int segIndex = 0;

    [Header("Timing Fields")]
    [SerializeField] private bool hasStarted = false;
    [SerializeField] private bool startedEnding = false;
    [SerializeField] private float introWait = 5; //how long before the black screen fades out
    [SerializeField] private bool canInteract = false;

    [Header("Scene Fields")]
    [SerializeField] private string nextScene = "next";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!hasStarted) {
            hasStarted = true;
            StartCoroutine(BeginScene());
        }
    }

    private IEnumerator BeginScene()
    {
        yield return new WaitForSeconds(introWait);
        FadeImage.instance.CallWhite();
        yield return new WaitForSeconds(2f);
    }

    private void CheckForIndex()
    {
        if (segments.Count <= segIndex)
        {
            Debug.Log("no more segments, ending the scene");
            CallEndScene();
            return;
        }
    }

    private void CallEndScene()
    {
        if (!startedEnding) { startedEnding = true; StartCoroutine(EndScene()); }
    }

    private IEnumerator EndScene()
    {
        FadeImage.instance.CallBlack();
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(nextScene);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class Segment
{
    [Header("Animation")]
    [SerializeField] public bool transitionScene = false;
    public string sceneCue;

    [Header("Audio")]
    public AudioSource audioCue;

    [Header("Dialogue")]
    public string dialogue;
    public bool hasDialogue = false;

}

