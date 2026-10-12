using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DialogueData",
    menuName = "Dialogue/Dialogue Data"
)]
public class DialogueData : ScriptableObject
{
    public List<DialogueNode> nodes;
}