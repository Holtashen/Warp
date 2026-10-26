//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class ChoiceUI : MonoBehaviour
//{
//    public TMP_Text text;
//    public Button button;

//    private string targetNodeID;
//    private DialogueManager dialogueManager;

//    public void Setup(
//        string choiceText,
//        string targetID,
//        DialogueManager manager)
//    {
//        text.text = choiceText;

//        targetNodeID = targetID;
//        dialogueManager = manager;

//        button.onClick.RemoveAllListeners();

//        button.onClick.AddListener(OnClick);
//    }

//    void OnClick()
//    {
//        dialogueManager.SelectChoice(targetNodeID);
//    }
//}

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
        dialogueManager.SelectChoice(targetNodeID);
    }
}

