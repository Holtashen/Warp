
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueUI : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Transform choiceContainer;
    public DialogueManager dialogueManager;

    public void ShowDialogue(string speaker, string text)
    {
        nameText.text = speaker;
        dialogueText.text = text;
    }

    //public void ShowChoices()
    //{
    //    continueButton.SetActive(false);
    //}

    //public void HideChoices()
    //{
    //    continueButton.SetActive(true);
    //}

    public void ShowChoices()
    {
        if (choiceContainer != null)
        {
            choiceContainer.gameObject.SetActive(true);
        }
    }

    public void HideChoices()
    {
        if (choiceContainer != null)
        {
            choiceContainer.gameObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueUI ÉÐÎ´°ó¶¨ DialogueManager¡£");
            return;
        }

        dialogueManager.AdvanceDialogue();
    }
}
