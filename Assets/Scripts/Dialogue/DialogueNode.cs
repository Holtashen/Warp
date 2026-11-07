
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueChoice
{
    [Header("Basic Information")]
    public string id;

    [Tooltip("原始选项文本，作为没有 Localization Key 时的回退文本")]
    public string text;

    [Tooltip("选项文本对应的 Localization Key")]
    public string textKey;

    public string targetNodeID;

    [Header("Effects and Conditions")]
    public List<ChoiceEffect> effects;

    public List<ChoiceCondition> conditions;
}

[Serializable]
public class DialogueNode
{
    [Header("Basic Information")]
    public string id;

    [Tooltip("普通自动跳转的目标节点 ID")]
    public string nextNodeID;

    [Header("Speaker")]
    [Tooltip("原始说话者名称，没有 Localization Key 时使用")]
    public string speaker;

    [Tooltip("说话者名称对应的 Localization Key")]
    public string speakerKey;

    [Header("Dialogue")]
    [TextArea(6, 15)]
    [Tooltip("原始对话文本，没有 Localization Key 时使用")]
    public string text;

    [Tooltip("对话内容对应的 Localization Key")]
    public string textKey;

    [Header("Choices")]
    public List<DialogueChoice> choices;

    [HideInInspector]
    public Vector2 editorPosition;
}
