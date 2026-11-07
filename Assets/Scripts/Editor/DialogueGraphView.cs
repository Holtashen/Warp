
using System;
using System.Linq;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    private const float NodeWidth = 260f;
    private const float NodeHeight = 260f;

    private DialogueData currentData;

    private Dictionary<string, DialogueNodeView> nodeViews
        = new Dictionary<string, DialogueNodeView>();

    public DialogueGraphView()
    {
        SetupZoom(
            ContentZoomer.DefaultMinScale,
            ContentZoomer.DefaultMaxScale
        );

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        graphViewChanged = OnGraphViewChanged;

        style.position = Position.Absolute;
        style.left = 0;
        style.right = 0;
        style.top = 0;
        style.bottom = 0;

        Undo.undoRedoPerformed += HandleUndoRedo;
    }

    // ==============================
    // 加载数据
    // ==============================

    public void LoadDialogueData(DialogueData data)
    {
        // 避免清理旧视图时修改旧数据
        currentData = null;

        DeleteElements(graphElements.ToList());

        nodeViews.Clear();

        currentData = data;

        if (currentData == null)
        {
            return;
        }

        if (currentData.nodes == null)
        {
            Undo.RecordObject(
                currentData,
                "Initialize Dialogue Nodes"
            );

            currentData.nodes = new List<DialogueNode>();

            SaveDialogueData();
        }

        // 确保所有 Choice 都有独立 ID
        if (EnsureChoiceIDs())
        {
            SaveDialogueData();
        }

        // 第一遍：创建所有节点
        foreach (DialogueNode node in currentData.nodes)
        {
            if (node == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(node.id))
            {
                Debug.LogError(
                    "Dialogue Node has an empty ID."
                );

                continue;
            }

            if (nodeViews.ContainsKey(node.id))
            {
                Debug.LogError(
                    "Duplicate Node ID: " + node.id
                );

                continue;
            }

            DialogueNodeView nodeView =
                new DialogueNodeView(
                    node,
                    HandleNodeEdit
                );

            AddElement(nodeView);

            nodeViews.Add(
                node.id,
                nodeView
            );
        }

        // 第二遍：根据数据恢复所有连接
        RestoreConnections();
    }

    // ==============================
    // Choice ID 管理
    // ==============================

    private bool EnsureChoiceIDs()
    {
        bool changed = false;

        foreach (DialogueNode node in currentData.nodes)
        {
            if (node == null || node.choices == null)
            {
                continue;
            }

            HashSet<string> usedIDs =
                new HashSet<string>();

            foreach (DialogueChoice choice in node.choices)
            {
                if (choice == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(choice.id) ||
                    usedIDs.Contains(choice.id))
                {
                    choice.id = GenerateChoiceID(usedIDs);

                    changed = true;
                }

                usedIDs.Add(choice.id);
            }
        }

        return changed;
    }

    private string GenerateChoiceID(HashSet<string> usedIDs)
    {
        int index = 1;

        string candidate;

        do
        {
            candidate = $"choice_{index:000}";
            index++;
        }
        while (usedIDs.Contains(candidate));

        return candidate;
    }

    // ==============================
    // 端口连接兼容性
    // ==============================

    public override List<Port> GetCompatiblePorts(
        Port startPort,
        NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(port =>
            port != startPort &&
            port.direction != startPort.direction &&
            port.portType == startPort.portType
        ).ToList();
    }

    // ==============================
    // 恢复连接
    // ==============================

    private void RestoreConnections()
    {
        foreach (DialogueNode node in currentData.nodes)
        {
            if (node == null ||
                !nodeViews.TryGetValue(node.id, out DialogueNodeView sourceView))
            {
                continue;
            }

            // 恢复普通自动跳转
            if (!string.IsNullOrEmpty(node.nextNodeID))
            {
                if (nodeViews.TryGetValue(
                    node.nextNodeID,
                    out DialogueNodeView targetView))
                {
                    CreateEdge(
                        sourceView.NextOutputPort,
                        targetView.InputPort
                    );
                }
                else
                {
                    Debug.LogWarning(
                        $"Next target not found: {node.id} -> {node.nextNodeID}"
                    );
                }
            }

            // 恢复 Choice 分支
            if (node.choices == null)
            {
                continue;
            }

            foreach (DialogueChoice choice in node.choices)
            {
                if (choice == null ||
                    string.IsNullOrEmpty(choice.targetNodeID))
                {
                    continue;
                }

                if (!sourceView.ChoiceOutputPorts.TryGetValue(
                    choice.id,
                    out Port outputPort))
                {
                    Debug.LogWarning(
                        $"Choice output port not found: {node.id}.{choice.id}"
                    );

                    continue;
                }

                if (!nodeViews.TryGetValue(
                    choice.targetNodeID,
                    out DialogueNodeView targetView))
                {
                    Debug.LogWarning(
                        $"Choice target not found: {choice.targetNodeID}"
                    );

                    continue;
                }

                CreateEdge(
                    outputPort,
                    targetView.InputPort
                );
            }
        }
    }

    private void CreateEdge(
        Port outputPort,
        Port inputPort)
    {
        if (outputPort == null || inputPort == null)
        {
            return;
        }

        Edge edge = new Edge
        {
            output = outputPort,
            input = inputPort
        };

        outputPort.Connect(edge);
        inputPort.Connect(edge);

        AddElement(edge);
    }

    // ==============================
    // 创建节点
    // ==============================

    public override void BuildContextualMenu(
        ContextualMenuPopulateEvent evt)
    {
        base.BuildContextualMenu(evt);

        if (currentData == null)
        {
            evt.menu.AppendAction(
                "Create Dialogue Node",
                _ => { },
                DropdownMenuAction.Status.Disabled
            );

            return;
        }

        Vector2 createPosition =
            contentViewContainer.WorldToLocal(
                evt.mousePosition
            );

        evt.menu.AppendAction(
            "Create Dialogue Node",
            _ => CreateNode(createPosition),
            DropdownMenuAction.Status.Normal
        );
    }

    private void CreateNode(Vector2 position)
    {
        if (currentData == null)
        {
            return;
        }

        Undo.RecordObject(
            currentData,
            "Create Dialogue Node"
        );

        DialogueNode newNode = new DialogueNode
        {
            id = GenerateUniqueNodeID(),
            nextNodeID = "",
            speaker = "",
            text = "",
            choices = new List<DialogueChoice>(),
            editorPosition = position
        };

        currentData.nodes.Add(newNode);

        DialogueNodeView nodeView =
            new DialogueNodeView(
                newNode,
                HandleNodeEdit
            );

        AddElement(nodeView);

        nodeViews.Add(
            newNode.id,
            nodeView
        );

        SaveDialogueData();
    }

    private string GenerateUniqueNodeID()
    {
        int index = 1;

        string candidate;

        do
        {
            candidate = $"node_{index:000}";
            index++;
        }
        while (
            currentData.nodes.Any(
                node =>
                    node != null &&
                    node.id == candidate
            )
        );

        return candidate;
    }

    // ==============================
    // 节点文字编辑
    // ==============================

    private void HandleNodeEdit(Action editAction)
    {
        if (currentData == null || editAction == null)
        {
            return;
        }

        Undo.RecordObject(
            currentData,
            "Edit Dialogue Node"
        );

        editAction();

        SaveDialogueData();
    }

    // ==============================
    // GraphView 变化监听
    // ==============================

    private GraphViewChange OnGraphViewChanged(
        GraphViewChange change)
    {
        if (currentData == null)
        {
            return change;
        }

        bool dataChanged = false;

        bool needsUndo =
            (change.elementsToRemove != null &&
             change.elementsToRemove.Any(
                 element =>
                     element is Edge ||
                     element is DialogueNodeView
             ))
            ||
            (change.edgesToCreate != null &&
             change.edgesToCreate.Count > 0);

        if (needsUndo)
        {
            Undo.RecordObject(
                currentData,
                "Modify Dialogue Connections"
            );
        }

        // 1. 删除连线
        if (change.elementsToRemove != null)
        {
            foreach (Edge edge in change.elementsToRemove
                .OfType<Edge>())
            {
                if (ClearConnection(edge))
                {
                    dataChanged = true;
                }
            }

            // 2. 删除节点
            List<DialogueNode> nodesToRemove =
                change.elementsToRemove
                    .OfType<DialogueNodeView>()
                    .Select(view => view.dialogueNode)
                    .Where(node => node != null)
                    .Distinct()
                    .ToList();

            foreach (DialogueNode removedNode in nodesToRemove)
            {
                RemoveNodeReferences(removedNode);

                currentData.nodes.Remove(removedNode);

                dataChanged = true;
            }
        }

        // 3. 创建连线
        if (change.edgesToCreate != null)
        {
            foreach (Edge edge in change.edgesToCreate.ToList())
            {
                if (SetConnection(edge))
                {
                    dataChanged = true;
                }
                else
                {
                    change.edgesToCreate.Remove(edge);

                    Debug.LogWarning(
                        "Invalid dialogue connection rejected."
                    );
                }
            }
        }

        // 4. 保存节点位置
        if (change.movedElements != null)
        {
            foreach (GraphElement element in change.movedElements)
            {
                if (element is DialogueNodeView nodeView &&
                    nodeView.dialogueNode != null)
                {
                    nodeView.dialogueNode.editorPosition =
                        nodeView.GetPosition().position;

                    dataChanged = true;
                }
            }
        }

        if (dataChanged)
        {
            SaveDialogueData();
        }

        return change;
    }

    // ==============================
    // 创建连接时更新数据
    // ==============================

    private bool SetConnection(Edge edge)
    {
        if (edge.output == null || edge.input == null)
        {
            return false;
        }

        DialogueNodeView sourceView =
            edge.output.node as DialogueNodeView;

        DialogueNodeView targetView =
            edge.input.node as DialogueNodeView;

        if (sourceView == null || targetView == null)
        {
            return false;
        }

        DialogueNode sourceNode =
            sourceView.dialogueNode;

        DialogueNode targetNode =
            targetView.dialogueNode;

        DialoguePortData portData =
            edge.output.userData as DialoguePortData;

        if (portData == null)
        {
            return false;
        }

        // 普通自动跳转
        if (portData.kind == DialoguePortKind.Next)
        {
            sourceNode.nextNodeID = targetNode.id;

            return true;
        }

        // 玩家选择分支
        if (portData.kind == DialoguePortKind.Choice)
        {
            if (sourceNode.choices == null)
            {
                return false;
            }

            DialogueChoice choice =
                sourceNode.choices.FirstOrDefault(
                    item =>
                        item != null &&
                        item.id == portData.choiceId
                );

            if (choice == null)
            {
                return false;
            }

            choice.targetNodeID = targetNode.id;

            return true;
        }

        return false;
    }

    // ==============================
    // 删除连接时更新数据
    // ==============================

    private bool ClearConnection(Edge edge)
    {
        if (edge.output == null || edge.input == null)
        {
            return false;
        }

        DialogueNodeView sourceView =
            edge.output.node as DialogueNodeView;

        DialogueNodeView targetView =
            edge.input.node as DialogueNodeView;

        if (sourceView == null || targetView == null)
        {
            return false;
        }

        DialogueNode sourceNode =
            sourceView.dialogueNode;

        DialogueNode targetNode =
            targetView.dialogueNode;

        DialoguePortData portData =
            edge.output.userData as DialoguePortData;

        if (portData == null)
        {
            return false;
        }

        if (portData.kind == DialoguePortKind.Next)
        {
            if (sourceNode.nextNodeID != targetNode.id)
            {
                return false;
            }

            sourceNode.nextNodeID = "";

            return true;
        }

        if (portData.kind == DialoguePortKind.Choice)
        {
            if (sourceNode.choices == null)
            {
                return false;
            }

            DialogueChoice choice =
                sourceNode.choices.FirstOrDefault(
                    item =>
                        item != null &&
                        item.id == portData.choiceId
                );

            if (choice == null ||
                choice.targetNodeID != targetNode.id)
            {
                return false;
            }

            choice.targetNodeID = "";

            return true;
        }

        return false;
    }

    // ==============================
    // 删除节点时清理引用
    // ==============================

    private void RemoveNodeReferences(
        DialogueNode removedNode)
    {
        foreach (DialogueNode node in currentData.nodes)
        {
            if (node == null || node == removedNode)
            {
                continue;
            }

            if (node.nextNodeID == removedNode.id)
            {
                node.nextNodeID = "";
            }

            if (node.choices == null)
            {
                continue;
            }

            foreach (DialogueChoice choice in node.choices)
            {
                if (choice != null &&
                    choice.targetNodeID == removedNode.id)
                {
                    choice.targetNodeID = "";
                }
            }
        }
    }

    // ==============================
    // 保存
    // ==============================

    private void SaveDialogueData()
    {
        if (currentData == null)
        {
            return;
        }

        EditorUtility.SetDirty(currentData);

        AssetDatabase.SaveAssets();
    }

    // ==============================
    // Undo / Redo
    // ==============================

    private void HandleUndoRedo()
    {
        if (currentData != null)
        {
            LoadDialogueData(currentData);
        }
    }

    public void Dispose()
    {
        Undo.undoRedoPerformed -= HandleUndoRedo;
    }

    // 自动布局接口，暂不实现
    public void AutoArrangeNodes()
    {
        // TODO: Implement automatic layout.
    }
}
