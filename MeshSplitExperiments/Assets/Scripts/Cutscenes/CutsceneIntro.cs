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
    [SerializeField] public Segment currentSegment;
    [SerializeField] private int segIndex = 0;
    [SerializeField] private int currentSegClick;

    [Header("Timing Fields")]
    [SerializeField] private bool hasStarted = false;
    [SerializeField] private bool startedEnding = false;
    [SerializeField] private float introWait = 5; //how long before the black screen fades out
    [SerializeField] private bool canInteract = false;

    [Header("Scene Fields")]
    [SerializeField] private string nextScene = "next";

    [SerializeField] private DialogueTrigger trigger;

    [Header("Audio Fields")]
    [SerializeField] private AudioSource source;
    

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
        canInteract = true;
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

    void IncrementSegment()
    {
        currentSegClick = 0;
        Segment segment = segments[segIndex];
        currentSegment = segment;
        segment.InitializeSegment();
        if (segment.transitionScene) { screenAnim.SetTrigger(segment.sceneCue); }
        if (segment.playAudioCue) { source.clip = segment.audioCue; source.Play(); }
        trigger.dialogue = segment.dialogue;
        TriggerSegment();
        segIndex++;
    }

    public void CheckCurrentClick()
    {
       if(currentSegment == null) { IncrementSegment(); return; }
        
        if(currentSegClick >= currentSegment.clickNum)
        {
            IncrementSegment();
        }
        else
        {
            TriggerSegment();
        }
    }


    public void TriggerSegment()
    {
        trigger.TriggerDialogue();
        currentSegClick++;
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
        if (canInteract)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                CheckCurrentClick();
            }
        }
    }
}

[System.Serializable]
public class Segment
{
    public bool started = false;
    [Header("Animation")]
    [SerializeField] public bool transitionScene = false;
    public string sceneCue;

    public Dialogue dialogue;
    public int clickNum = 1;

    [Header("Audio")]
    public bool playAudioCue = false;
    public AudioClip audioCue;

    public void InitializeSegment()
    {
        started = true;
        int count = dialogue.sentences.Length;
        if(count > 0) { clickNum = count; }
    }

}

