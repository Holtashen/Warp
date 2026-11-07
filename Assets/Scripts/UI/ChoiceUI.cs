
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChoiceUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public TMP_Text text;
    public Button button;

    [Header("Hover Colors")]
    public Color normalColor = new Color(
        0.91f, 0.88f, 0.82f, 1f
    );

    public Color hoverColor = new Color(
        1f, 0.82f, 0.42f, 1f
    );

    private string choiceID;
    private DialogueManager dialogueManager;

    public void Setup(
        string choiceText,
        string id,
        DialogueManager manager)
    {
        text.text = choiceText;

        choiceID = id;
        dialogueManager = manager;

        text.color = normalColor;

        button.onClick.RemoveListener(OnClick);
        button.onClick.AddListener(OnClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = normalColor;
    }

    private void OnClick()
    {
        if (dialogueManager == null ||
            string.IsNullOrEmpty(choiceID))
        {
            Debug.LogError("ChoiceUI: Invalid choice reference.");
            return;
        }

        dialogueManager.SelectChoice(choiceID);
    }
}
