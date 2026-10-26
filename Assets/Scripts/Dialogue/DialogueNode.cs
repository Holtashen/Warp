using System;
using System.Collections.Generic;
using UnityEngine;

//[Serializable]
//public class DialogueChoice
//{
//    public string text;
//    public string targetNodeID;
//}
[Serializable]
public class DialogueChoice
{
    public string text;

    public string targetNodeID;

    public List<ChoiceEffect> effects;

    public List<ChoiceCondition> conditions;
}

//[Serializable]
//public class DialogueNode
//{
//    public string id;

//    public string speaker;

//    [TextArea(3, 8)]
//    public string text;

//    public List<DialogueChoice> choices;
//}

[Serializable]
public class DialogueNode
{
    public string id;

    [Tooltip("普通对话结束后进入的节点 ID；有分支时优先使用分支")]
    public string nextNodeID;

    public string speaker;

    [TextArea(6, 15)]
    public string text;

    public List<DialogueChoice> choices;

    [HideInInspector]
    public Vector2 editorPosition;
}