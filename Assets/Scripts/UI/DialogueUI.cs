using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text dialogueText;

    public GameObject continueButton;
    public Transform choiceContainer;

    public void ShowDialogue(string speaker, string text)
    {
        nameText.text = speaker;
        dialogueText.text = text;
    }

    public void ShowChoices()
    {
        continueButton.SetActive(false);
    }

    public void HideChoices()
    {
        continueButton.SetActive(true);
    }
}