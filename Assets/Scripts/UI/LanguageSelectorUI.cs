
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageSelectorUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField]
    private TMP_Dropdown languageDropdown;

    private LocalizationManager localizationManager;

    private bool isSubscribed;

    private void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        // 如果组件启用时 Manager 已经存在，则立即尝试订阅。
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Initialize()
    {
        if (languageDropdown == null)
        {
            Debug.LogError(
                "LanguageSelectorUI：没有绑定 TMP_Dropdown！",
                this
            );

            return;
        }

        localizationManager = LocalizationManager.Instance;

        if (localizationManager == null)
        {
            Debug.LogError(
                "LanguageSelectorUI：找不到 LocalizationManager！",
                this
            );

            return;
        }

        CreateDropdownOptions();

        // 显示当前语言，但不触发 OnValueChanged。
        languageDropdown.SetValueWithoutNotify(
            (int)localizationManager.CurrentLanguage
        );

        languageDropdown.RefreshShownValue();

        TrySubscribe();

        // 注册用户选择事件。
        languageDropdown.onValueChanged.RemoveListener(
            OnLanguageSelected
        );

        languageDropdown.onValueChanged.AddListener(
            OnLanguageSelected
        );
    }

    private void CreateDropdownOptions()
    {
        List<string> options = new List<string>
        {
            "简体中文",
            "日本語",
            "English"
        };

        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(options);
    }

    private void OnLanguageSelected(int index)
    {
        if (localizationManager == null)
        {
            localizationManager = LocalizationManager.Instance;
        }

        if (localizationManager == null)
        {
            Debug.LogError(
                "无法切换语言：LocalizationManager 不存在。"
            );

            return;
        }

        if (index < 0 || index >= 3)
        {
            Debug.LogWarning(
                "无效的语言选项：" + index
            );

            return;
        }

        GameLanguage selectedLanguage =
            (GameLanguage)index;

        localizationManager.SetLanguage(selectedLanguage);
    }

    private void TrySubscribe()
    {
        if (isSubscribed)
        {
            return;
        }

        if (localizationManager == null)
        {
            localizationManager = LocalizationManager.Instance;
        }

        if (localizationManager == null)
        {
            return;
        }

        localizationManager.LanguageChanged +=
            HandleLanguageChanged;

        isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!isSubscribed)
        {
            return;
        }

        if (localizationManager != null)
        {
            localizationManager.LanguageChanged -=
                HandleLanguageChanged;
        }

        isSubscribed = false;
    }

    private void HandleLanguageChanged(GameLanguage language)
    {
        if (languageDropdown == null)
        {
            return;
        }

        languageDropdown.SetValueWithoutNotify((int)language);
        languageDropdown.RefreshShownValue();
    }
}
