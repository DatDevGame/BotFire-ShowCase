using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Reward))]
public class RewardDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float labelWidth = 120; 
        float fieldWidth = (position.width - labelWidth - 10) / 2;
        Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
        Rect typeRect = new Rect(position.x + labelWidth + 5, position.y, fieldWidth, position.height);
        Rect amountRect = new Rect(position.x + labelWidth + fieldWidth + 10, position.y, fieldWidth, position.height);

        EditorGUI.LabelField(labelRect, label.text);

        SerializedProperty typeProp = property.FindPropertyRelative("type");
        SerializedProperty amountProp = property.FindPropertyRelative("amount");

        EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);
        EditorGUI.PropertyField(amountRect, amountProp, GUIContent.none);

        EditorGUI.EndProperty();
    }
}
