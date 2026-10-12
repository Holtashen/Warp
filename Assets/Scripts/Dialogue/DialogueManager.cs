using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DialogueData dialogueData;
    public DialogueUI dialogueUI;

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
        dialogueUI.ShowDialogue(
            currentNode.speaker,
            currentNode.text
        );

        ClearChoices();

        if (currentNode.choices != null &&
            currentNode.choices.Count > 0)
        {
            //foreach (DialogueChoice choice in currentNode.choices)
            //{
            //    ChoiceUI choiceUI =
            //        Instantiate(choicePrefab, choiceContainer);

            //    choiceUI.Setup(
            //        choice.text,
            //        choice.targetNodeID,
            //        this
            //    );
            //}

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
            }

            dialogueUI.ShowChoices();
        }
    }

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
            }
        }
    }

}