#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using FlexibleDropdown.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlexibleDropdown.Editor
{
    internal static class FlexibleDropdownHelper
    {
        public const string UndoMessage = "Change Dropdown Value";

        public static void LabelAndPropertyField(Rect position, SerializedProperty property, GUIContent label)
        {
            property.isExpanded =
                EditorGUI.Foldout(
                    new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight),
                    property.isExpanded, label, true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(new Rect(position.x,
                    position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                    position.width, EditorGUIUtility.singleLineHeight), property, label, true);
                EditorGUI.indentLevel--;
            }
        }

        private static string CreateInvalidTypeLabelMessage<TAttribute, TValue>()
            where TAttribute : Attribute, IDropdownProvider<TValue>
        {
            return $"Use {typeof(TAttribute).Name} with {typeof(TValue).Name}.";
        }

        public static void InvalidTypeLabel<TAttribute, TValue>(Rect position)
            where TAttribute : Attribute, IDropdownProvider<TValue>
        {
            GUI.Label(position, CreateInvalidTypeLabelMessage<TAttribute, TValue>());
        }

        public static VisualElement CreateInvalidTypeLabel<TAttribute, TValue>()
            where TAttribute : Attribute, IDropdownProvider<TValue>
        {
            return new Label(CreateInvalidTypeLabelMessage<TAttribute, TValue>());
        }

        public static bool ValidateAttributeType<TValue>(FieldInfo fieldInfo)
        {
            var fieldType = ReflectionUtility.GetFieldElementType(fieldInfo);
            return fieldType == typeof(TValue) || fieldType.IsSubclassOf(typeof(TValue));
        }

        public static TValue GetCurrentValue<TValue>(SerializedProperty property)
        {
            try
            {
                var value = property.TracePropertyValue();
                return value is TValue typedValue ? typedValue : default;
            }
            catch
            {
                return default;
            }
        }

        public static void SetValue<TValue>(SerializedProperty property, TValue value)
        {
            try
            {
                property.SetPropertyValue(value);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to set property value: {e.Message}");
            }
        }

        public static int GetSelectedIndex<TValue>(IReadOnlyCollection<TValue> choices, TValue currentValue,
            TValue defaultValue)
        {
            var valueToFind = currentValue ?? defaultValue;
            var index = 0;
            foreach (var choice in choices)
            {
                if (EqualityComparer<TValue>.Default.Equals(choice, valueToFind))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }
    }
}
#endif