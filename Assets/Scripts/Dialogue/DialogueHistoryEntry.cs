using System;

public enum DialogueHistoryEntryType
{
    Dialogue,
    PlayerChoice
}

[Serializable]
public class DialogueHistoryEntry
{
    public DialogueHistoryEntryType type;

    public string speaker;
    public string text;

    public DialogueHistoryEntry(
        DialogueHistoryEntryType type,
        string speaker,
        string text)
    {
        this.type = type;
        this.speaker = speaker;
        this.text = text;
    }
}