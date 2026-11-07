
using System;
using System.Collections.Generic;
using System.Text;

using UnityEditor;
using UnityEngine;

public class LocalizationBatchGenerator : EditorWindow
{
    private DialogueData dialogueData;
    private LocalizationDatabase database;

    private GameLanguage sourceLanguage = GameLanguage.Japanese;

    [MenuItem("Tools/Localization/Batch Generator")]
    public static void OpenWindow()
    {
        LocalizationBatchGenerator window =
            GetWindow<LocalizationBatchGenerator>();

        window.titleContent = new GUIContent(
            "Localization Generator"
        );

        window.minSize = new Vector2(400, 260);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(
            "Localization Batch Generator",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space(10);

        dialogueData = (DialogueData)EditorGUILayout.ObjectField(
            "Dialogue Data",
            dialogueData,
            typeof(DialogueData),
            false
        );

        database = (LocalizationDatabase)EditorGUILayout.ObjectField(
            "Localization Database",
            database,
            typeof(LocalizationDatabase),
            false
        );

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField(
            "Source Language",
            EditorStyles.boldLabel
        );

        sourceLanguage = (GameLanguage)EditorGUILayout.EnumPopup(
            sourceLanguage
        );

        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "工具会为没有 Key 的对话和选项生成 Key，" +
            "并在数据库中创建对应条目。\n\n" +
            "原有 Key 和已有翻译不会被覆盖。" +
            "原始文本只会填入指定语言的空字段。",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        GUI.enabled =
            dialogueData != null &&
            database != null;

        if (GUILayout.Button(
            "Generate Localization Entries",
            GUILayout.Height(35)))
        {
            GenerateEntries();
        }

        GUI.enabled = true;
    }

    private void GenerateEntries()
    {
        if (dialogueData == null || database == null)
        {
            return;
        }

        if (dialogueData.nodes == null)
        {
            EditorUtility.DisplayDialog(
                "Localization Generator",
                "DialogueData 中没有 Nodes 列表。",
                "确定"
            );

            return;
        }

        bool confirmed = EditorUtility.DisplayDialog(
            "Generate Localization Entries",
            "即将扫描：\n"
            + dialogueData.name
            + "\n\n目标数据库：\n"
            + database.name
            + "\n\n源语言："
            + sourceLanguage
            + "\n\n是否继续？",
            "Generate",
            "Cancel"
        );

        if (!confirmed)
        {
            return;
        }

        Undo.RecordObject(
            dialogueData,
            "Generate Dialogue Localization Keys"
        );

        Undo.RecordObject(
            database,
            "Generate Localization Entries"
        );

        if (database.entries == null)
        {
            database.entries = new List<LocalizedEntry>();
        }

        Dictionary<string, LocalizedEntry> entryMap =
            new Dictionary<string, LocalizedEntry>();

        // 建立已有 Key 索引，不覆盖已有条目
        foreach (LocalizedEntry entry in database.entries)
        {
            if (entry == null ||
                string.IsNullOrWhiteSpace(entry.key))
            {
                continue;
            }

            if (!entryMap.ContainsKey(entry.key))
            {
                entryMap.Add(entry.key, entry);
            }
            else
            {
                Debug.LogWarning(
                    "[Localization Generator] 数据库存在重复 Key："
                    + entry.key
                );
            }
        }

        string prefix = SanitizeKey(dialogueData.name);

        int generatedKeyCount = 0;
        int createdEntryCount = 0;
        int skippedChoiceCount = 0;

        foreach (DialogueNode node in dialogueData.nodes)
        {
            if (node == null ||
                string.IsNullOrWhiteSpace(node.id))
            {
                continue;
            }

            string nodePrefix =
                prefix + "." + SanitizeKey(node.id);

            // Speaker
            if (!string.IsNullOrWhiteSpace(node.speaker) ||
                !string.IsNullOrWhiteSpace(node.speakerKey))
            {
                string key = node.speakerKey;

                if (string.IsNullOrWhiteSpace(key))
                {
                    key = nodePrefix + ".speaker";
                    node.speakerKey = key;
                    generatedKeyCount++;
                }

                EnsureEntry(
                    key,
                    node.speaker,
                    entryMap,
                    ref createdEntryCount
                );
            }

            // Dialogue
            if (!string.IsNullOrWhiteSpace(node.text) ||
                !string.IsNullOrWhiteSpace(node.textKey))
            {
                string key = node.textKey;

                if (string.IsNullOrWhiteSpace(key))
                {
                    key = nodePrefix + ".text";
                    node.textKey = key;
                    generatedKeyCount++;
                }

                EnsureEntry(
                    key,
                    node.text,
                    entryMap,
                    ref createdEntryCount
                );
            }

            // Choices
            if (node.choices == null)
            {
                continue;
            }

            foreach (DialogueChoice choice in node.choices)
            {
                if (choice == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(choice.text) &&
                    string.IsNullOrWhiteSpace(choice.textKey))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(choice.id))
                {
                    Debug.LogWarning(
                        "[Localization Generator] Choice 缺少 ID，跳过："
                        + node.id
                    );

                    skippedChoiceCount++;
                    continue;
                }

                string key = choice.textKey;

                if (string.IsNullOrWhiteSpace(key))
                {
                    key = nodePrefix + "."
                        + SanitizeKey(choice.id);

                    choice.textKey = key;
                    generatedKeyCount++;
                }

                EnsureEntry(
                    key,
                    choice.text,
                    entryMap,
                    ref createdEntryCount
                );
            }
        }

        EditorUtility.SetDirty(dialogueData);
        EditorUtility.SetDirty(database);

        AssetDatabase.SaveAssets();

        string result =
            "生成完成！\n\n"
            + "新生成的 Key：" + generatedKeyCount + "\n"
            + "新建的翻译条目：" + createdEntryCount + "\n"
            + "跳过的 Choice：" + skippedChoiceCount;

        Debug.Log(
            "[Localization Generator]\n" + result
        );

        EditorUtility.DisplayDialog(
            "Localization Generator",
            result,
            "确定"
        );
    }

    private void EnsureEntry(
        string key,
        string sourceText,
        Dictionary<string, LocalizedEntry> entryMap,
        ref int createdEntryCount)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (!entryMap.TryGetValue(key, out LocalizedEntry entry))
        {
            entry = new LocalizedEntry
            {
                key = key
            };

            database.entries.Add(entry);
            entryMap.Add(key, entry);

            createdEntryCount++;
        }

        if (string.IsNullOrWhiteSpace(sourceText))
        {
            return;
        }

        // 只填写指定语言的空字段，不覆盖已有翻译
        switch (sourceLanguage)
        {
            case GameLanguage.Chinese:
                if (string.IsNullOrWhiteSpace(entry.chinese))
                    entry.chinese = sourceText;
                break;

            case GameLanguage.Japanese:
                if (string.IsNullOrWhiteSpace(entry.japanese))
                    entry.japanese = sourceText;
                break;

            case GameLanguage.English:
                if (string.IsNullOrWhiteSpace(entry.english))
                    entry.english = sourceText;
                break;
        }
    }

    private string SanitizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unnamed";
        }

        StringBuilder builder = new StringBuilder();

        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c) ||
                c == '_' ||
                c == '-')
            {
                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append('_');
            }
        }

        return builder.ToString();
    }
}
