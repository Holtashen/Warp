//using System.Linq;
//using UnityEditor;
//using UnityEditor.Experimental.GraphView;
//using UnityEngine;
//using UnityEngine.UIElements;

//public class DialogueGraphView : GraphView
//{
//    private const float NodeWidth = 260f;
//    private const float NodeHeight = 180f;

//    private const float HorizontalSpacing = 80f;
//    private const float VerticalSpacing = 60f;

//    public DialogueGraphView()
//    {
//        SetupZoom(
//            ContentZoomer.DefaultMinScale,
//            ContentZoomer.DefaultMaxScale
//        );

//        this.AddManipulator(new ContentDragger());
//        this.AddManipulator(new SelectionDragger());
//        this.AddManipulator(new RectangleSelector());

//        style.position = Position.Absolute;
//        style.left = 0;
//        style.right = 0;
//        style.top = 0;
//        style.bottom = 0;
//    }

//    public void LoadDialogueData(DialogueData data)
//    {
//        DeleteElements(graphElements.ToList());

//        if (data == null)
//        {
//            return;
//        }

//        ArrangeNodes(data);

//        foreach (DialogueNode node in data.nodes)
//        {
//            DialogueNodeView nodeView =
//                new DialogueNodeView(node);

//            AddElement(nodeView);
//        }
//    }

//    private void ArrangeNodes(DialogueData data)
//    {
//        int columnCount = 3;
//        //对话节点排列的列数，目前是3

//        float cellWidth =
//            NodeWidth + HorizontalSpacing;

//        float cellHeight =
//            NodeHeight + VerticalSpacing;

//        for (int i = 0; i < data.nodes.Count; i++)
//        {
//            DialogueNode node = data.nodes[i];

//            int column = i % columnCount;
//            int row = i / columnCount;

//            node.editorPosition = new Vector2(
//                column * cellWidth,
//                row * cellHeight
//            );
//        }

//        EditorUtility.SetDirty(data);
//        AssetDatabase.SaveAssets();
//    }
//}


using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphView
{
    private const float NodeWidth = 260f;
    private const float NodeHeight = 180f;

    private const float HorizontalSpacing = 80f;
    private const float VerticalSpacing = 60f;

    private DialogueData currentData;

    public DialogueGraphView()
    {
        SetupZoom(
            ContentZoomer.DefaultMinScale,
            ContentZoomer.DefaultMaxScale
        );

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        // 监听节点移动
        graphViewChanged = OnGraphViewChanged;

        style.position = Position.Absolute;
        style.left = 0;
        style.right = 0;
        style.top = 0;
        style.bottom = 0;
    }

    public void LoadDialogueData(DialogueData data)
    {
        // 清除上一次加载的节点
        DeleteElements(graphElements.ToList());

        currentData = data;

        if (data == null || data.nodes == null)
        {
            return;
        }

        // 仅在首次布局时自动排列
        if (NeedsInitialLayout(data))
        {
            ArrangeNodes(data);
        }

        foreach (DialogueNode node in data.nodes)
        {
            if (node == null)
            {
                continue;
            }

            DialogueNodeView nodeView =
                new DialogueNodeView(node);

            AddElement(nodeView);
        }
    }

    private bool NeedsInitialLayout(DialogueData data)
    {
        if (data.nodes == null || data.nodes.Count == 0)
        {
            return false;
        }

        // 所有节点都处于默认原点时，
        // 才认为这份数据尚未完成初始布局。
        return data.nodes
            .Where(node => node != null)
            .All(node => node.editorPosition == Vector2.zero);
    }

    private void ArrangeNodes(DialogueData data)
    {
        const int columnCount = 3;

        float cellWidth =
            NodeWidth + HorizontalSpacing;

        float cellHeight =
            NodeHeight + VerticalSpacing;

        for (int i = 0; i < data.nodes.Count; i++)
        {
            DialogueNode node = data.nodes[i];

            if (node == null)
            {
                continue;
            }

            int column = i % columnCount;
            int row = i / columnCount;

            node.editorPosition = new Vector2(
                column * cellWidth,
                row * cellHeight
            );
        }

        SaveDialogueData();
    }

    private GraphViewChange OnGraphViewChanged(
        GraphViewChange change)
    {
        if (change.movedElements == null ||
            currentData == null)
        {
            return change;
        }

        bool changedAnyNode = false;

        foreach (GraphElement element in change.movedElements)
        {
            if (element is DialogueNodeView nodeView &&
                nodeView.dialogueNode != null)
            {
                // 将节点的新坐标写回对话数据
                nodeView.dialogueNode.editorPosition =
                    nodeView.GetPosition().position;

                changedAnyNode = true;
            }
        }

        if (changedAnyNode)
        {
            SaveDialogueData();
        }

        return change;
    }

    private void SaveDialogueData()
    {
        if (currentData == null)
        {
            return;
        }

        // 标记资产已修改，并保存到磁盘
        EditorUtility.SetDirty(currentData);
        AssetDatabase.SaveAssets();
    }
}
