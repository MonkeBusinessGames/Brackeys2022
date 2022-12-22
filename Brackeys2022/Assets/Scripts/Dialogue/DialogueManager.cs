using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;

public class DialogueManager : MonoBehaviour
{
    #region Events
    public delegate void Action();
    public static event Action LevelEnd;
    public static event Action CutsceneEnd;
    #endregion

    private Queue<string> sentences;
    private Queue<Dialogue> dialogues;
    private Dialogue currentDialogue;
    [SerializeField] GameObject DialogueArea;
    [SerializeField] private Image imageArea;
    [SerializeField] private TMP_Text textArea;
    private bool dialoging = false;
    public static bool reachedGoal = false;
    public static bool imaginaryFriend = false;
    public static GameObject[] triggers;

    private void Awake()
    {
        dialogues = new Queue<Dialogue>();
        sentences = new Queue<string>();
        DialogueArea.SetActive(false);
    }

    private void Update()
    {
        if(dialoging)
            if (Input.GetKeyDown("f")) //Input.GetButtonDown("Jump")
                DisplayNextSentence();
    }

    public void StartDialogue(Dialogue[] dialogue)
    {
        DialogueArea.SetActive(true);
        dialoging = true;
        Time.timeScale = 0;
        dialogues.Clear();
        foreach (Dialogue dial in dialogue)
            dialogues.Enqueue(dial);
        currentDialogue = dialogues.Dequeue();
        imageArea.sprite = currentDialogue.sprite;
        sentences.Clear();
        foreach (LocalizedString strings in currentDialogue.sentences)
        {
            var sentence = strings.GetLocalizedString();
            sentences.Enqueue(sentence);
        }
            
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if(sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        textArea.text = sentences.Dequeue();
    }

    private void EndDialogue()
    {
        if (dialogues.Count == 0)
        {
            dialoging = false;
            Time.timeScale = 1;
            DialogueArea.SetActive(false);
            if (imaginaryFriend)
            {
                CutsceneEnd();
            }
            else
                PlayerController.frozen = false;

        }
        else
            NextDialogue();
    }

    private void NextDialogue()
    {
        currentDialogue = dialogues.Dequeue();
        imageArea.sprite = currentDialogue.sprite;
        sentences.Clear();
        foreach (LocalizedString strings in currentDialogue.sentences)
        {
            var sentence = strings.GetLocalizedString();
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }
}
