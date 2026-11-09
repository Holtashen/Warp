using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    [Header("Database")]
    [SerializeField]
    private LocalizationDatabase database;

    [Header("Language")]
    [SerializeField]
    private GameLanguage initialLanguage = GameLanguage.Japanese;

    public GameLanguage CurrentLanguage { get; private set; }

    public event Action<GameLanguage> LanguageChanged;

    private Dictionary<string, LocalizedEntry> entryDictionary =
        new Dictionary<string, LocalizedEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentLanguage = initialLanguage;

        BuildDictionary();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void BuildDictionary()
    {
        entryDictionary.Clear();

        if (database == null)
        {
            Debug.LogError("LocalizationDatabase 未设置！");
            return;
        }

        if (database.entries == null)
        {
            Debug.LogError("LocalizationDatabase 的 Entries 列表为空！");
            return;
        }

        foreach (LocalizedEntry entry in database.entries)
        {
            if (entry == null)
            {
                Debug.LogWarning("LocalizationDatabase 中存在空条目。");
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.key))
            {
                Debug.LogWarning("发现没有 Key 的翻译条目。");
                continue;
            }

            if (entryDictionary.ContainsKey(entry.key))
            {
                Debug.LogError(
                    "发现重复的 Localization Key：" + entry.key
                );
                continue;
            }

            entryDictionary.Add(entry.key, entry);
        }
    }

    public void SetLanguage(GameLanguage language)
    {
        if (CurrentLanguage == language)
        {
            return;
        }

        CurrentLanguage = language;

        LanguageChanged?.Invoke(CurrentLanguage);

        Debug.Log("语言已切换为：" + CurrentLanguage);
    }

    //private void Update()
    //{
    //    // 按 F4 显示当前语言
    //    if (Input.GetKeyDown(KeyCode.F4))
    //    {
    //        Debug.Log("语言为：" + CurrentLanguage);
    //    }
    //}


    public string GetText(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            Debug.LogWarning("Localization Key 为空。");
            return string.Empty;
        }

        if (!entryDictionary.TryGetValue(key, out LocalizedEntry entry))
        {
            Debug.LogWarning("找不到 Localization Key：" + key);
            return key;
        }

        string localizedText = GetLanguageText(entry, CurrentLanguage);

        // 当前语言没有翻译时，回退到英语。
        if (string.IsNullOrEmpty(localizedText))
        {
            localizedText = entry.english;
        }

        // 英语也没有翻译时，返回 Key，便于定位缺失文本。
        if (string.IsNullOrEmpty(localizedText))
        {
            Debug.LogWarning("文本翻译缺失：" + key);
            return key;
        }

        return localizedText;
    }

    private string GetLanguageText(
        LocalizedEntry entry,
        GameLanguage language)
    {
        switch (language)
        {
            case GameLanguage.Chinese:
                return entry.chinese;

            case GameLanguage.Japanese:
                return entry.japanese;

            case GameLanguage.English:
                return entry.english;

            default:
                return string.Empty;
        }
    }
}