#if UNITY_EDITOR
using System.Linq;
using FlexibleDropdown.Attributes;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlexibleDropdown.Editor
{
    public abstract class FlexibleDropdownDrawerBase<TAttribute, TValue> : PropertyDrawer
        where TAttribute : PropertyAttribute, IDropdownProvider<TValue>
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var customAttribute = (TAttribute)attribute;
            if (!FlexibleDropdownHelper.ValidateAttributeType<TValue>(fieldInfo))
            {
                return FlexibleDropdownHelper.CreateInvalidTypeLabel<TAttribute, TValue>();
            }

            var choices = customAttribute.Choices.ToList();

            var currentValue = FlexibleDropdownHelper.GetCurrentValue<TValue>(property);
            currentValue ??= customAttribute.DefaultValue;
            var selectedIndex =
                FlexibleDropdownHelper.GetSelectedIndex(choices, currentValue, customAttribute.DefaultValue);

            var container = new VisualElement
            {
                style =
                {
                    minHeight = EditorGUIUtility.singleLineHeight
                }
            };

            var popupLabel = customAttribute.IsPropertyFieldEnabled ? "" : property.displayName;
            var popup = new PopupField<TValue>(popupLabel, choices,
                selectedIndex, customAttribute.FormatSelectedValue, customAttribute.FormatListItem)
            {
                style = { flexGrow = 1 }
            };
            popup.BindProperty(property);

            if (customAttribute.IsPropertyFieldEnabled)
            {
                var foldout = new Foldout
                {
                    text = property.displayName,
                    value = property.isExpanded,
                    style = { flexGrow = 1 }
                };
                var propertyField = new PropertyField(property, property.displayName)
                {
                    style = { flexGrow = 1 }
                };
                propertyField.BindProperty(property);

                foldout.Add(propertyField);
                container.Add(foldout);

                var dummyField = EditorUIToolkitUtility.CreateDummyInputField(property);
                container.Add(dummyField);
                var dummyFieldInput = dummyField.Q(className: EditorUIToolkitUtility.DummyInputUssClassName);
                dummyFieldInput.RegisterCallback<GeometryChangedEvent>(evt =>
                {
                    popup.style.width = dummyFieldInput.resolvedStyle.width;
                });

                popup.style.position = Position.Absolute;
                popup.style.right = 0;
            }
            else
            {
                popup.AddToClassList(BaseField<TValue>.alignedFieldUssClassName);
            }

            popup.RegisterValueChangedCallback(evt =>
            {
                Undo.RecordObject(property.serializedObject.targetObject, FlexibleDropdownHelper.UndoMessage);
                FlexibleDropdownHelper.SetValue(property, evt.newValue);
            });
            container.Add(popup);

            return container;
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var customAttribute = (TAttribute)attribute;
            if (!FlexibleDropdownHelper.ValidateAttributeType<TValue>(fieldInfo))
            {
                FlexibleDropdownHelper.InvalidTypeLabel<TAttribute, TValue>(position);
                return;
            }

            var choices = customAttribute.Choices.ToArray();

            EditorGUI.BeginProperty(position, label, property);
            {
                var currentValue = FlexibleDropdownHelper.GetCurrentValue<TValue>(property);
                currentValue ??= customAttribute.DefaultValue;
                var selectedIndex =
                    FlexibleDropdownHelper.GetSelectedIndex(choices, currentValue, customAttribute.DefaultValue);

                var popupLabel = customAttribute.IsPropertyFieldEnabled ? GUIContent.none : label;
                var popupPosition = position;
                if (customAttribute.IsPropertyFieldEnabled)
                {
                    FlexibleDropdownHelper.LabelAndPropertyField(position, property, label);
                    popupPosition = new Rect(position.x + EditorGUIUtility.labelWidth, position.y,
                        position.width - EditorGUIUtility.labelWidth, position.height);
                }

                EditorGUI.BeginChangeCheck();
                selectedIndex = EditorGUI.Popup(popupPosition, popupLabel, selectedIndex,
                    choices.Select(customAttribute.FormatListItem).Select(static item => new GUIContent(item))
                        .ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(property.serializedObject.targetObject, FlexibleDropdownHelper.UndoMessage);
                    FlexibleDropdownHelper.SetValue(property, choices[selectedIndex]);
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.isExpanded)
            {
                return EditorGUI.GetPropertyHeight(property, label, true) +
                       EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
            }

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
#endif