using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public bool active = false;

    public void TriggerDialogue()
    {
        DialogueManager.instance.TriggerDialogue(this, dialogue);
    }

    public void EndDialogue()
    {
        active = false;
    }
}
