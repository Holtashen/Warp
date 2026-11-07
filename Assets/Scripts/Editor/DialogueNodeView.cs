
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

    // ==============================
    // 节点内部编辑区域
    // ==============================

    private void CreateNodeFields()
    {
        // Speaker
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

        extensionContainer.Add(speakerField);

        // Speaker Localization Key
        TextField speakerKeyField = new TextField("Speaker Key")
        {
            value = dialogueNode.speakerKey ?? "",
            isDelayed = true
        };

        speakerKeyField.RegisterValueChangedCallback(evt =>
        {
            onNodeEdited?.Invoke(() =>
            {
                dialogueNode.speakerKey = evt.newValue;
            });
        });

        extensionContainer.Add(speakerKeyField);

        // Dialogue
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

        extensionContainer.Add(dialogueField);

        // Dialogue Localization Key
        TextField dialogueKeyField = new TextField("Dialogue Key")
        {
            value = dialogueNode.textKey ?? "",
            isDelayed = true
        };

        dialogueKeyField.RegisterValueChangedCallback(evt =>
        {
            onNodeEdited?.Invoke(() =>
            {
                dialogueNode.textKey = evt.newValue;
            });
        });

        extensionContainer.Add(dialogueKeyField);

        // Choice 编辑区域
        CreateChoiceFields();

        RefreshExpandedState();
        RefreshPorts();
    }

    // ==============================
    // Choice 文本和 Key 编辑
    // ==============================

    private void CreateChoiceFields()
    {
        if (dialogueNode.choices == null ||
            dialogueNode.choices.Count == 0)
        {
            return;
        }

        Foldout choicesFoldout = new Foldout
        {
            text = "Choices",
            value = false
        };

        foreach (DialogueChoice choice in dialogueNode.choices)
        {
            if (choice == null)
            {
                continue;
            }

            Foldout choiceFoldout = new Foldout
            {
                text = choice.id,
                value = false
            };

            // Choice ID
            Label choiceIDLabel = new Label(
                "Choice ID: " + choice.id
            );

            choiceFoldout.Add(choiceIDLabel);

            // Choice Text
            TextField choiceTextField = new TextField("Text")
            {
                value = choice.text ?? "",
                isDelayed = true
            };

            choiceTextField.RegisterValueChangedCallback(evt =>
            {
                onNodeEdited?.Invoke(() =>
                {
                    choice.text = evt.newValue;

                    choiceFoldout.text =
                        string.IsNullOrEmpty(choice.text)
                            ? choice.id
                            : choice.text;

                    UpdateChoicePortLabel(choice);
                });
            });

            choiceFoldout.Add(choiceTextField);

            // Choice Localization Key
            TextField choiceKeyField = new TextField("Text Key")
            {
                value = choice.textKey ?? "",
                isDelayed = true
            };

            choiceKeyField.RegisterValueChangedCallback(evt =>
            {
                onNodeEdited?.Invoke(() =>
                {
                    choice.textKey = evt.newValue;
                });
            });

            choiceFoldout.Add(choiceKeyField);

            choicesFoldout.Add(choiceFoldout);
        }

        extensionContainer.Add(choicesFoldout);
    }

    // 修改 Choice 文本时同步更新输出端口名称
    private void UpdateChoicePortLabel(DialogueChoice choice)
    {
        if (choice == null ||
            string.IsNullOrEmpty(choice.id))
        {
            return;
        }

        if (ChoiceOutputPorts.TryGetValue(
            choice.id,
            out Port outputPort))
        {
            outputPort.portName =
                string.IsNullOrEmpty(choice.text)
                    ? choice.id
                    : choice.text;

            RefreshPorts();
        }
    }

    // ==============================
    // 输入端口
    // ==============================

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

    // ==============================
    // 创建输出端口
    // ==============================

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
