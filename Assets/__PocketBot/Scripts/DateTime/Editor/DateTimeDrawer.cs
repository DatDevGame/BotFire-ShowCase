using UnityEditor;
using UnityEngine;
using System;
using System.Globalization;

[CustomPropertyDrawer(typeof(DateTimeSerialized))]
public class DateTimeDrawer : PropertyDrawer
{
    private const string DateTimeFormat = "dd/MM/yyyy - HH:mm:ss";

    private class DatePickerWindow : PopupWindowContent
    {
        private DateTime _selectedDate;
        private Action<DateTime> _onDateSelected;

        public DatePickerWindow(DateTime selectedDate, Action<DateTime> onDateSelected)
        {
            _selectedDate = selectedDate;
            _onDateSelected = onDateSelected;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(220, 150);
        }

        public override void OnGUI(Rect rect)
        {
            int month = _selectedDate.Month;
            int year = _selectedDate.Year;

            string[] monthNames = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames;
            int selectedMonthIndex = month - 1;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Month:", GUILayout.Width(60));
            selectedMonthIndex = EditorGUILayout.Popup(selectedMonthIndex, monthNames);
            month = selectedMonthIndex + 1;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Year:", GUILayout.Width(60));
            year = EditorGUILayout.IntField(year);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int day = Mathf.Min(_selectedDate.Day, daysInMonth);
            EditorGUILayout.LabelField("Day:", GUILayout.Width(60));
            day = EditorGUILayout.IntSlider(day, 1, daysInMonth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Hour:", GUILayout.Width(60));
            int hour = EditorGUILayout.IntField(_selectedDate.Hour, GUILayout.Width(30));
            hour = Mathf.Clamp(hour, 0, 23);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Minute:", GUILayout.Width(60));
            int minute = EditorGUILayout.IntField(_selectedDate.Minute, GUILayout.Width(30));
            minute = Mathf.Clamp(minute, 0, 59);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Second:", GUILayout.Width(60));
            int second = EditorGUILayout.IntField(_selectedDate.Second, GUILayout.Width(30));
            second = Mathf.Clamp(second, 0, 59);
            EditorGUILayout.EndHorizontal();

            _selectedDate = new DateTime(year, month, day, hour, minute, second);

            if (GUILayout.Button("OK"))
            {
                _onDateSelected(_selectedDate);
                editorWindow.Close();
            }
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty dateTimeTicksProp = property.FindPropertyRelative("dateTimeTicks");
        DateTime dateTime = new DateTime(dateTimeTicksProp.longValue);

        position = EditorGUI.PrefixLabel(position, label);

        if (GUI.Button(position, dateTime.ToString(DateTimeFormat, CultureInfo.InvariantCulture), EditorStyles.popup))
        {
            PopupWindow.Show(position, new DatePickerWindow(dateTime, (newDate) =>
            {
                dateTime = newDate;
                dateTimeTicksProp.longValue = dateTime.Ticks;
                property.serializedObject.ApplyModifiedProperties();
            }));
        }

        EditorGUI.EndProperty();
    }
}