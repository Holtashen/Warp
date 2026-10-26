using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DialogueData dialogueData;
    public DialogueUI dialogueUI;
    private int availableChoiceCount = 0;

    //public void AdvanceDialogue()
    //{
    //    if (currentNode == null)
    //    {
    //        return;
    //    }

    //    // 有分支时，不允许点击面板跳过选择
    //    if (currentNode.choices != null &&
    //        currentNode.choices.Count > 0)
    //    {
    //        return;
    //    }

    //    // 没有下一节点，结束推进
    //    if (string.IsNullOrEmpty(currentNode.nextNodeID))
    //    {
    //        Debug.Log("当前对话已经结束：" + currentNode.id);
    //        return;
    //    }

    //    StartDialogue(currentNode.nextNodeID);
    //}


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
            Debug.Log("当前对话已经结束：" + currentNode.id);
            return;
        }

        StartDialogue(currentNode.nextNodeID);
    }


    private Dictionary<string, DialogueNode> nodeDictionary;

    private DialogueNode currentNode;

    public ChoiceUI choicePrefab;
    public Transform choiceContainer;

    void Start()
    {
        BuildDictionary();

        StartDialogue("room_001");
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

    public void StartDialogue(string nodeID)
    {
        if (!nodeDictionary.TryGetValue(nodeID, out currentNode))
        {
            Debug.LogError("找不到 Node: " + nodeID);
            return;
        }

        ShowCurrentNode();
    }

    //void ShowCurrentNode()
    //{
    //    dialogueUI.ShowDialogue(
    //        currentNode.speaker,
    //        currentNode.text
    //    );
    //}


    void ShowCurrentNode()
    {
        availableChoiceCount = 0;
        dialogueUI.ShowDialogue(
            currentNode.speaker,
            currentNode.text
        );

        ClearChoices();

       // int availableChoiceCount = 0;

        if (currentNode.choices != null)
        {
            foreach (DialogueChoice choice in currentNode.choices)
            {
                if (!CheckConditions(choice))
                {
                    
                    continue;
                }

                ChoiceUI choiceUI =
                    Instantiate(choicePrefab, choiceContainer);

                choiceUI.Setup(
                    choice.text,
                    choice.targetNodeID,
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

    public void SelectChoice(string targetNodeID)
    {
        DialogueChoice selectedChoice = null;

        foreach (DialogueChoice choice in currentNode.choices)
        {
            if (choice.targetNodeID == targetNodeID)
            {
                selectedChoice = choice;
                break;
            }
        }

        if (selectedChoice == null)
        {
            Debug.LogError("找不到对应的 Choice: " + targetNodeID);
            return;
        }

        ApplyEffects(selectedChoice);

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

}