using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DialogueData dialogueData;
    public DialogueUI dialogueUI;

    [Header("Dialogue History")]
    [SerializeField]
    private DialogueHistoryManager historyManager;

    [SerializeField]
    private string playerChoiceSpeaker = "You";

    [SerializeField]
    private string playerChoiceSpeakerKey;

    [Header("Startup")]
    [SerializeField] private bool startDialogueOnStart = true;
    [SerializeField] private string initialNodeID = "room_001";

    private int availableChoiceCount = 0;



    public void AdvanceDialogue()
    {
        if (currentNode == null)
        {
            return;
        }

        // 存在有效选项时，必须由玩家选择
        if (availableChoiceCount > 0)
        {
            return;
        }

        // 没有可用选项时，按普通对话的下一节点推进
        if (string.IsNullOrEmpty(currentNode.nextNodeID))
        {
            EndDialogue();
            return;
        }

        StartDialogue(currentNode.nextNodeID);
    }


    private Dictionary<string, DialogueNode> nodeDictionary;

    private DialogueNode currentNode;

    private bool isListeningLocalization;

    public ChoiceUI choicePrefab;
    public Transform choiceContainer;

    void Start()
    {
        SubscribeToLocalization();

        BuildDictionary();

        if (startDialogueOnStart)
        {
            StartDialogue(initialNodeID);
        }
    }

    void BuildDictionary()
    {
        nodeDictionary = new Dictionary<string, DialogueNode>();

        foreach (DialogueNode node in dialogueData.nodes)
        {
            if (nodeDictionary.ContainsKey(node.id))
            {
                Debug.LogError("重复的 Node ID: " + node.id);
                continue;
            }

            nodeDictionary.Add(node.id, node);
        }
    }

    //public void StartDialogue(string nodeID)
    //{
    //    if (!nodeDictionary.TryGetValue(nodeID, out currentNode))
    //    {
    //        Debug.LogError("Cannot find Node: " + nodeID);
    //        return;
    //    }

    //    if (dialogueUI != null)
    //        dialogueUI.ShowPanel();

    //    ShowCurrentNode();
    //}

    private void RecordCurrentDialogue()
    {
        if (historyManager == null)
            return;

        string speakerText = GetLocalizedText(
            currentNode.speakerKey,
            currentNode.speaker
        );

        string dialogueText = GetLocalizedText(
            currentNode.textKey,
            currentNode.text
        );

        historyManager.AddDialogue(
            speakerText,
            dialogueText
        );
    }

    public void EndDialogue()
    {
        currentNode = null;
        availableChoiceCount = 0;

        ClearChoices();

        if (dialogueUI != null)
        {
            dialogueUI.HideChoices();
            dialogueUI.HidePanel();
        }
    }

    //void ShowCurrentNode()
    //{
    //    dialogueUI.ShowDialogue(
    //        currentNode.speaker,
    //        currentNode.text
    //    );
    //}


    public void StartDialogue(string nodeID)
    {
        if (!nodeDictionary.TryGetValue(nodeID, out currentNode))
        {
            Debug.LogError("找不到 Node: " + nodeID);
            return;
        }

        if (dialogueUI != null)
        {
            dialogueUI.ShowPanel();
        }

        // 记录当前节点的对话
        RecordCurrentDialogue();

        ShowCurrentNode();
    }


    void ShowCurrentNode()
    {
        availableChoiceCount = 0;

        // 获取说话者名称
        string speakerText = GetLocalizedText(
            currentNode.speakerKey,
            currentNode.speaker
        );

        // 获取对话内容
        string dialogueText = GetLocalizedText(
            currentNode.textKey,
            currentNode.text
        );

        // 显示对话
        dialogueUI.UpdateDialogueContent(
     speakerText,
     dialogueText
 );

        // 清除旧的 Choice UI
        ClearChoices();

        // 创建当前节点的 Choice
        if (currentNode.choices != null)
        {
            foreach (DialogueChoice choice in currentNode.choices)
            {
                if (choice == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(choice.id))
                {
                    Debug.LogWarning(
                        "发现没有 ID 的 Choice，跳过。Node: "
                        + currentNode.id
                    );

                    continue;
                }

                if (!CheckConditions(choice))
                {
                    continue;
                }

                // 根据当前语言获取 Choice 文本
                string choiceText = GetLocalizedText(
                    choice.textKey,
                    choice.text
                );

                ChoiceUI choiceUI = Instantiate(
                    choicePrefab,
                    choiceContainer
                );

                // 这里仍然传入 choice.id，而不是 targetNodeID
                choiceUI.Setup(
                    choiceText,
                    choice.id,
                    this
                );

                availableChoiceCount++;
            }
        }

        if (availableChoiceCount > 0)
        {
            dialogueUI.ShowChoices();
        }
        else
        {
            dialogueUI.HideChoices();
        }
    }


    //void ShowCurrentNode()
    //{
    //    dialogueUI.ShowDialogue(
    //        currentNode.speaker,
    //        currentNode.text
    //    );

    //    ClearChoices();

    //    if (currentNode.choices != null &&
    //        currentNode.choices.Count > 0)
    //    {
    //        //foreach (DialogueChoice choice in currentNode.choices)
    //        //{
    //        //    ChoiceUI choiceUI =
    //        //        Instantiate(choicePrefab, choiceContainer);

    //        //    choiceUI.Setup(
    //        //        choice.text,
    //        //        choice.targetNodeID,
    //        //        this
    //        //    );
    //        //}

    //        foreach (DialogueChoice choice in currentNode.choices)
    //        {
    //            if (!CheckConditions(choice))
    //            {
    //                continue;
    //            }

    //            ChoiceUI choiceUI =
    //                Instantiate(choicePrefab, choiceContainer);

    //            choiceUI.Setup(
    //                choice.text,
    //                choice.targetNodeID,
    //                this
    //            );
    //        }

    //        dialogueUI.ShowChoices();
    //    }
    //}

    void ClearChoices()
    {
        for (int i = choiceContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(choiceContainer.GetChild(i).gameObject);
        }
    }

    private bool CheckConditions(DialogueChoice choice)
    {
        if (choice.conditions == null)
        {
            return true;
        }

        foreach (ChoiceCondition condition in choice.conditions)
        {
            switch (condition.type)
            {
                case ConditionType.HasFlag:

                    if (!GameState.Instance.HasFlag(condition.key))
                    {
                        return false;
                    }

                    break;

                case ConditionType.NotHasFlag:

                    if (GameState.Instance.HasFlag(condition.key))
                    {
                        return false;
                    }

                    break;

                case ConditionType.IntGreaterOrEqual:

                    if (GameState.Instance.GetInt(condition.key)
                        < condition.value)
                    {
                        return false;
                    }

                    break;

                case ConditionType.IntLessOrEqual:

                    if (GameState.Instance.GetInt(condition.key)
                        > condition.value)
                    {
                        return false;
                    }

                    break;
            }
        }

        return true;
    }

    //public void SelectChoice(string targetNodeID)
    //{
    //    StartDialogue(targetNodeID);
    //}


    public void SelectChoice(string choiceID)
    {
        if (currentNode == null)
        {
            Debug.LogError(
                "Cannot select Choice: currentNode is null."
            );

            return;
        }

        if (string.IsNullOrEmpty(choiceID))
        {
            Debug.LogError("Choice ID is empty.");
            return;
        }

        DialogueChoice selectedChoice = null;

        // 根据独立 Choice ID 查找选项
        foreach (DialogueChoice choice in currentNode.choices)
        {
            if (choice == null)
            {
                continue;
            }

            if (choice.id == choiceID)
            {
                if (selectedChoice != null)
                {
                    Debug.LogError(
                        "Duplicate Choice ID: " + choiceID
                    );

                    return;
                }

                selectedChoice = choice;
            }
        }

        if (selectedChoice == null)
        {
            Debug.LogError(
                "Cannot find Choice ID: " + choiceID
            );

            return;
        }

        // 再次检查条件，防止过期选项被执行
        if (!CheckConditions(selectedChoice))
        {
            Debug.LogWarning(
                "Choice conditions are no longer satisfied: "
                + choiceID
            );

            ShowCurrentNode();
            return;
        }

        // 检查目标节点
        string targetNodeID = selectedChoice.targetNodeID;

        if (string.IsNullOrEmpty(targetNodeID))
        {
            Debug.LogError(
                "Choice has no target Node: " + choiceID
            );

            return;
        }

        if (!nodeDictionary.ContainsKey(targetNodeID))
        {
            Debug.LogError(
                "Target Node does not exist: " + targetNodeID
            );

            return;
        }

        // 记录玩家实际选择的选项
        if (historyManager != null)
        {
            string choiceText = GetLocalizedText(
                selectedChoice.textKey,
                selectedChoice.text
            );

            string playerSpeaker = GetLocalizedText(
                playerChoiceSpeakerKey,
                playerChoiceSpeaker
            );

            historyManager.AddPlayerChoice(
                playerSpeaker,
                choiceText
            );
        }
        // 执行当前选项的 Effect
        ApplyEffects(selectedChoice);

        // 进入目标节点
        StartDialogue(targetNodeID);
    }


    private void ApplyEffects(DialogueChoice choice)
    {
        if (choice.effects == null)
        {
            return;
        }

        foreach (ChoiceEffect effect in choice.effects)
        {
            switch (effect.type)
            {
                case EffectType.SetFlag:

                    GameState.Instance.SetFlag(effect.key);

                    Debug.Log("设置 Flag: " + effect.key);

                    break;

                case EffectType.AddInt:

                    GameState.Instance.AddInt(
                        effect.key,
                        effect.value
                    );

                    break;

                case EffectType.SetDimension2D:

                    DimensionSwitcher switcher =
                        FindObjectOfType<DimensionSwitcher>();

                    if (switcher != null)
                    {
                        switcher.SetDimension(effect.boolValue);
                    }
                    else
                    {
                        Debug.LogError("找不到 DimensionSwitcher！");
                    }

                    break;
            }
        }
    }


    private void OnEnable()
    {
        SubscribeToLocalization();
    }

    private void OnDisable()
    {
        UnsubscribeFromLocalization();
    }

    private void SubscribeToLocalization()
    {
        if (isListeningLocalization)
        {
            return;
        }

        if (LocalizationManager.Instance == null)
        {
            return;
        }

        LocalizationManager.Instance.LanguageChanged +=
            HandleLanguageChanged;

        isListeningLocalization = true;
    }

    private void UnsubscribeFromLocalization()
    {
        if (!isListeningLocalization)
        {
            return;
        }

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.LanguageChanged -=
                HandleLanguageChanged;
        }

        isListeningLocalization = false;
    }

    private void HandleLanguageChanged(GameLanguage language)
    {
        if (currentNode == null)
        {
            return;
        }

        ShowCurrentNode();
    }

    private string GetLocalizedText(string key, string fallbackText)
    {
        // 没有设置 Key，使用原始文本
        if (string.IsNullOrWhiteSpace(key))
        {
            return fallbackText ?? "";
        }

        // LocalizationManager 不存在时，使用原始文本
        if (LocalizationManager.Instance == null)
        {
            Debug.LogWarning(
                "LocalizationManager 不存在，使用原始文本：" + key
            );

            return fallbackText ?? "";
        }

        // 根据当前语言取得翻译
        return LocalizationManager.Instance.GetText(key);
    }

}