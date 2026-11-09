using TMPro;
using UnityEngine;

public class DialogueHistoryItemUI : MonoBehaviour
{
    [Header("Text References")]
    public TMP_Text speakerText;
    public TMP_Text contentText;

    [Header("Colors")]
    public Color dialogueSpeakerColor =
        new Color(0.91f, 0.88f, 0.82f, 1f);

    public Color dialogueTextColor =
        new Color(0.91f, 0.88f, 0.82f, 1f);

    public Color choiceSpeakerColor =
        new Color(0.95f, 0.75f, 0.40f, 1f);

    public Color choiceTextColor =
        new Color(0.95f, 0.75f, 0.40f, 1f);

    public void Setup(DialogueHistoryEntry entry)
    {
        if (entry == null)
            return;

        bool isChoice =
            entry.type ==
            DialogueHistoryEntryType.PlayerChoice;

        if (speakerText != null)
        {
            speakerText.text = entry.speaker ?? "";

            speakerText.color = isChoice
                ? choiceSpeakerColor
                : dialogueSpeakerColor;
        }

        if (contentText != null)
        {
            contentText.text = entry.text ?? "";

            contentText.color = isChoice
                ? choiceTextColor
                : dialogueTextColor;
        }
    }
}