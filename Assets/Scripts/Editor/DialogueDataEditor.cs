
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueData))]
public class DialogueDataEditor : Editor
{
    private SerializedProperty nodesProperty;
    private Vector2 scrollPosition;

    private void OnEnable()
    {
        nodesProperty = serializedObject.FindProperty("nodes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField(
            "Dialogue Data",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space(4);

        // 让 Nodes 使用可伸缩的滚动区域
        EditorGUILayout.BeginVertical(
            GUILayout.ExpandHeight(true)
        );

        scrollPosition = EditorGUILayout.BeginScrollView(
            scrollPosition,
            GUILayout.ExpandHeight(true)
        );

        // 保留 Nodes 内所有嵌套字段的默认绘制行为
        EditorGUILayout.PropertyField(
            nodesProperty,
            true
        );

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
