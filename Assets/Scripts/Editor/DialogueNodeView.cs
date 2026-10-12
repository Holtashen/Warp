using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueNodeView : Node
{
    public DialogueNode dialogueNode;

    public DialogueNodeView(DialogueNode node)
    {
        dialogueNode = node;

        title = node.id;

        CreateInputPort();

        CreateOutputPorts();

        SetPosition(
            new Rect(
                node.editorPosition,
                new Vector2(260, 180)
            )
        );
    }

    private void CreateInputPort()
    {
        Port inputPort = InstantiatePort(
            Orientation.Horizontal,
            Direction.Input,
            Port.Capacity.Multi,
            typeof(bool)
        );

        inputPort.portName = "Input";

        inputContainer.Add(inputPort);
    }

    private void CreateOutputPorts()
    {
        if (dialogueNode.choices == null)
        {
            return;
        }

        foreach (DialogueChoice choice in dialogueNode.choices)
        {
            Port outputPort = InstantiatePort(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Single,
                typeof(bool)
            );

            outputPort.portName = choice.text;

            outputContainer.Add(outputPort);
        }
    }
}