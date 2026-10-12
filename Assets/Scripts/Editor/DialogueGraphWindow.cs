using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class DialogueGraphWindow : EditorWindow
{
    private DialogueGraphView graphView;

    private DialogueData dialogueData;

    [MenuItem("Window/Dialogue Editor")]
    public static void Open()
    {
        DialogueGraphWindow window =
            GetWindow<DialogueGraphWindow>();

        window.titleContent =
            new GUIContent("Dialogue Editor");

        window.Show();
    }

    private void OnEnable()
    {
        graphView = new DialogueGraphView();

        rootVisualElement.Add(graphView);

        ObjectField dialogueField =
            new ObjectField("Dialogue Data");

        dialogueField.objectType =
            typeof(DialogueData);

        dialogueField.RegisterValueChangedCallback(
            evt =>
            {
                dialogueData =
                    evt.newValue as DialogueData;

                graphView.LoadDialogueData(dialogueData);
            }
        );

        rootVisualElement.Add(dialogueField);
    }

    private void OnDisable()
    {
        if (graphView != null)
        {
            rootVisualElement.Remove(graphView);
        }
    }
}