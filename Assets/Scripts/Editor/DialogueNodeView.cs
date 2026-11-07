
using System;
using System.Collections.Generic;

using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

// 输出端口类型
public enum DialoguePortKind
{
    Next,
    Choice
}

// 输出端口对应的数据
public class DialoguePortData
{
    public DialoguePortKind kind;
    public string choiceId;
}

public class DialogueNodeView : Node
{
    public DialogueNode dialogueNode;

    private Action<Action> onNodeEdited;

    public Port InputPort { get; private set; }

    public Port NextOutputPort { get; private set; }

    public Dictionary<string, Port> ChoiceOutputPorts
        = new Dictionary<string, Port>();

    public DialogueNodeView(
        DialogueNode node,
        Action<Action> editCallback)
    {
        dialogueNode = node;
        onNodeEdited = editCallback;

        title = node.id;

        capabilities |=
            Capabilities.Selectable |
            Capabilities.Movable |
            Capabilities.Deletable;

        CreateInputPort();
        CreateOutputPorts();
        CreateNodeFields();

        SetPosition(
            new Rect(
                node.editorPosition,
                new Vector2(260, 260)
            )
        );
    }

    // 创建节点内部文字编辑区域
    private void CreateNodeFields()
    {
        TextField speakerField = new TextField("Speaker")
        {
            value = dialogueNode.speaker ?? "",
            isDelayed = true
        };

        speakerField.RegisterValueChangedCallback(evt =>
        {
            onNodeEdited?.Invoke(() =>
            {
                dialogueNode.speaker = evt.newValue;
            });
        });

        TextField dialogueField = new TextField("Dialogue")
        {
            value = dialogueNode.text ?? "",
            multiline = true,
            isDelayed = true
        };

        dialogueField.style.minHeight = 110;

        dialogueField.RegisterValueChangedCallback(evt =>
        {
            onNodeEdited?.Invoke(() =>
            {
                dialogueNode.text = evt.newValue;
            });
        });

        extensionContainer.Add(speakerField);
        extensionContainer.Add(dialogueField);

        RefreshExpandedState();
        RefreshPorts();
    }

    // 输入端口
    private void CreateInputPort()
    {
        InputPort = InstantiatePort(
            Orientation.Horizontal,
            Direction.Input,
            Port.Capacity.Multi,
            typeof(bool)
        );

        InputPort.portName = "Input";

        inputContainer.Add(InputPort);
    }

    // 创建两种输出端口
    private void CreateOutputPorts()
    {
        // 普通自动跳转
        NextOutputPort = InstantiatePort(
            Orientation.Horizontal,
            Direction.Output,
            Port.Capacity.Single,
            typeof(bool)
        );

        NextOutputPort.portName = "Next";

        NextOutputPort.userData = new DialoguePortData
        {
            kind = DialoguePortKind.Next
        };

        outputContainer.Add(NextOutputPort);

        // 玩家选择分支
        if (dialogueNode.choices == null)
        {
            return;
        }

        foreach (DialogueChoice choice in dialogueNode.choices)
        {
            if (choice == null ||
                string.IsNullOrEmpty(choice.id))
            {
                continue;
            }

            Port outputPort = InstantiatePort(
                Orientation.Horizontal,
                Direction.Output,
                Port.Capacity.Single,
                typeof(bool)
            );

            outputPort.portName =
                string.IsNullOrEmpty(choice.text)
                    ? choice.id
                    : choice.text;

            outputPort.userData = new DialoguePortData
            {
                kind = DialoguePortKind.Choice,
                choiceId = choice.id
            };

            outputContainer.Add(outputPort);

            ChoiceOutputPorts.Add(
                choice.id,
                outputPort
            );
        }
    }
}
