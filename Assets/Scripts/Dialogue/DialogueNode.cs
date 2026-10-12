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

    public string speaker;

    [TextArea(3, 8)]
    public string text;

    public List<DialogueChoice> choices;

    [HideInInspector]
    public Vector2 editorPosition;
}