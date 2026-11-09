using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueHistoryManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxEntries = 500;

    private readonly List<DialogueHistoryEntry> entries =
        new List<DialogueHistoryEntry>();

    public IReadOnlyList<DialogueHistoryEntry> Entries
    {
        get { return entries; }
    }

    public event Action HistoryChanged;

    // 记录 NPC 对话
    public void AddDialogue(string speaker, string text)
    {
        Debug.Log(
            $"[History] Add Dialogue: {speaker} - {text}"
        );

        AddEntry(
            DialogueHistoryEntryType.Dialogue,
            speaker,
            text
        );
    }

    // 记录玩家选择
    public void AddPlayerChoice(string speaker, string text)
    {
        AddEntry(
            DialogueHistoryEntryType.PlayerChoice,
            speaker,
            text
        );
    }

    private void AddEntry(
        DialogueHistoryEntryType type,
        string speaker,
        string text)
    {
        DialogueHistoryEntry entry =
            new DialogueHistoryEntry(
                type,
                speaker,
                text
            );

        entries.Add(entry);

        // 限制历史记录数量，避免无限增长
        if (maxEntries > 0)
        {
            while (entries.Count > maxEntries)
            {
                entries.RemoveAt(0);
            }
        }

        HistoryChanged?.Invoke();
    }

    public void ClearHistory()
    {
        entries.Clear();

        HistoryChanged?.Invoke();
    }
}