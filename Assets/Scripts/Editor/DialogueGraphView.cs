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

    public DialogueGraphView()
    {
        SetupZoom(
            ContentZoomer.DefaultMinScale,
            ContentZoomer.DefaultMaxScale
        );

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        style.position = Position.Absolute;
        style.left = 0;
        style.right = 0;
        style.top = 0;
        style.bottom = 0;
    }

    public void LoadDialogueData(DialogueData data)
    {
        DeleteElements(graphElements.ToList());

        if (data == null)
        {
            return;
        }

        ArrangeNodes(data);

        foreach (DialogueNode node in data.nodes)
        {
            DialogueNodeView nodeView =
                new DialogueNodeView(node);

            AddElement(nodeView);
        }
    }

    private void ArrangeNodes(DialogueData data)
    {
        int columnCount = 3;

        float cellWidth =
            NodeWidth + HorizontalSpacing;

        float cellHeight =
            NodeHeight + VerticalSpacing;

        for (int i = 0; i < data.nodes.Count; i++)
        {
            DialogueNode node = data.nodes[i];

            int column = i % columnCount;
            int row = i / columnCount;

            node.editorPosition = new Vector2(
                column * cellWidth,
                row * cellHeight
            );
        }

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }
}