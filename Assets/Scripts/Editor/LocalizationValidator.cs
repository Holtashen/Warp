
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class LocalizationValidator
{
    [MenuItem("Tools/Localization/Validate Localization")]
    public static void ValidateLocalization()
    {
        int errorCount = 0;
        int warningCount = 0;

        HashSet<string> allKeys = new HashSet<string>();

        // ==============================
        // 1. 检查 Localization Database
        // ==============================

        string[] databaseGuids =
            AssetDatabase.FindAssets("t:LocalizationDatabase");

        List<LocalizationDatabase> databases =
            new List<LocalizationDatabase>();

        foreach (string guid in databaseGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            LocalizationDatabase database =
                AssetDatabase.LoadAssetAtPath<LocalizationDatabase>(path);

            if (database == null)
            {
                continue;
            }

            databases.Add(database);

            if (database.entries == null)
            {
                Debug.LogError(
                    "[Localization] Database Entries 为空：" + path
                );

                errorCount++;
                continue;
            }

            HashSet<string> databaseKeys = new HashSet<string>();

            foreach (LocalizedEntry entry in database.entries)
            {
                if (entry == null)
                {
                    Debug.LogWarning(
                        "[Localization] 存在空 Entry：" + path
                    );

                    warningCount++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.key))
                {
                    Debug.LogError(
                        "[Localization] 存在空 Key：" + path
                    );

                    errorCount++;
                    continue;
                }

                if (!databaseKeys.Add(entry.key))
                {
                    Debug.LogError(
                        "[Localization] 重复 Key：" + entry.key
                        + "，Database：" + path
                    );

                    errorCount++;
                }

                allKeys.Add(entry.key);

                if (string.IsNullOrWhiteSpace(entry.chinese))
                {
                    Debug.LogWarning(
                        "[Localization] 缺少中文翻译：" + entry.key
                    );

                    warningCount++;
                }

                if (string.IsNullOrWhiteSpace(entry.japanese))
                {
                    Debug.LogWarning(
                        "[Localization] 缺少日语翻译：" + entry.key
                    );

                    warningCount++;
                }

                if (string.IsNullOrWhiteSpace(entry.english))
                {
                    Debug.LogWarning(
                        "[Localization] 缺少英语翻译：" + entry.key
                    );

                    warningCount++;
                }
            }
        }

        // ==============================
        // 2. 检查 DialogueData 引用
        // ==============================

        string[] dialogueGuids =
            AssetDatabase.FindAssets("t:DialogueData");

        foreach (string guid in dialogueGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            DialogueData data =
                AssetDatabase.LoadAssetAtPath<DialogueData>(path);

            if (data == null || data.nodes == null)
            {
                continue;
            }

            foreach (DialogueNode node in data.nodes)
            {
                if (node == null)
                {
                    continue;
                }

                CheckKey(
                    node.speakerKey,
                    "Speaker",
                    node.id,
                    path,
                    allKeys,
                    ref errorCount
                );

                CheckKey(
                    node.textKey,
                    "Dialogue",
                    node.id,
                    path,
                    allKeys,
                    ref errorCount
                );

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

                    CheckKey(
                        choice.textKey,
                        "Choice",
                        node.id + "." + choice.id,
                        path,
                        allKeys,
                        ref errorCount
                    );
                }
            }
        }

        // ==============================
        // 3. 输出结果
        // ==============================

        Debug.Log(
            "========== Localization Validation ==========\n"
            + "Database 数量：" + databases.Count + "\n"
            + "DialogueData 数量：" + dialogueGuids.Length + "\n"
            + "有效 Key 数量：" + allKeys.Count + "\n"
            + "错误数量：" + errorCount + "\n"
            + "警告数量：" + warningCount + "\n"
            + "============================================="
        );

        if (errorCount == 0 && warningCount == 0)
        {
            EditorUtility.DisplayDialog(
                "Localization Validation",
                "检查完成！没有发现错误或警告。",
                "确定"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Localization Validation",
                "检查完成。\n错误：" + errorCount
                + "\n警告：" + warningCount
                + "\n详细信息请查看 Console。",
                "确定"
            );
        }
    }

    // ==============================
    // 检查单个 Key
    // ==============================

    private static void CheckKey(
        string key,
        string type,
        string owner,
        string assetPath,
        HashSet<string> allKeys,
        ref int errorCount)
    {
        // 没有填写 Key 是允许的：
        // 运行时会使用原始文本。
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (!allKeys.Contains(key))
        {
            Debug.LogError(
                "[Localization] 找不到 Key："
                + key
                + " | 类型：" + type
                + " | 所属节点：" + owner
                + " | 资产：" + assetPath
            );

            errorCount++;
        }
    }
}
