using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    #region Singleton

    public static DialogueManager instance;

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

    private Queue<string> sentences;

    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Animator textAnim;

    [Header("Timing Fields")]
    [SerializeField] private float maxDialogueTime;
    [SerializeField] private float currentDialogueTime;

    [SerializeField] private DialogueTrigger currentDialogue;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sentences = new Queue<string>();
    }

    public void TriggerDialogue(DialogueTrigger trigger, Dialogue dialogue)
    {
        currentDialogue = trigger;
        if (trigger.active) { DisplayNextSentence(); }
        else { StartDialogue(dialogue); }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        Debug.Log("Started Dialogue");
        sentences.Clear();
        foreach(string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        textAnim.SetTrigger("black");
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0) {

            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        dialogueText.text = sentence;
        Debug.Log(sentence);
    }

    public void EndDialogue()
    {
        Debug.Log("End of Conversation");
        textAnim.SetTrigger("white");
        currentDialogue.active = false;
        currentDialogue = null;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
