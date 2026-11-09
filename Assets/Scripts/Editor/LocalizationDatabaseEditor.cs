using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizationDatabase))]
public class LocalizationDatabaseEditor : Editor
{
    private SerializedProperty entriesProperty;
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        entriesProperty = serializedObject.FindProperty("entries");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Keep the Script reference visible, as in Unity's default Inspector.
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField(
                "Script",
                MonoScript.FromScriptableObject((LocalizationDatabase)target),
                typeof(MonoScript),
                false
            );
        }

        EditorGUILayout.Space(4);

        // Let the Entries area use the remaining Inspector height.
        EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

        scrollPosition = EditorGUILayout.BeginScrollView(
            scrollPosition,
            false,
            true,
            GUILayout.ExpandHeight(true)
        );

        EditorGUILayout.PropertyField(entriesProperty, true);

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
