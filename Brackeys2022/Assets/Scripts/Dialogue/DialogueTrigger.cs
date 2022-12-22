using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private Dialogue[] dialogue;

    public bool atEndDestroyDialogue = false;

    public void TriggerDialogue()
    {
		if (atEndDestroyDialogue == true)
		{
            FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
            Debug.Log("Dialogue started!");
            Destroy(gameObject);
        }
        else
        {
            FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
            Debug.Log("Dialogue started!");
        }
    }

}
