using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceUI : MonoBehaviour
{
    public TMP_Text text;
    public Button button;

    private string targetNodeID;
    private DialogueManager dialogueManager;

    public void Setup(
        string choiceText,
        string targetID,
        DialogueManager manager)
    {
        text.text = choiceText;

        targetNodeID = targetID;
        dialogueManager = manager;

        button.onClick.RemoveAllListeners();

        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        dialogueManager.SelectChoice(targetNodeID);
    }
}