using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(ModifierImage))]
public class ModifierImageInspector : ImageEditor
{
    protected SerializedProperty m_VertPositionsSerializedProp;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_VertPositionsSerializedProp = serializedObject.FindProperty("m_VertPositions");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.PropertyField(m_VertPositionsSerializedProp);
        serializedObject.ApplyModifiedProperties();
    }
}