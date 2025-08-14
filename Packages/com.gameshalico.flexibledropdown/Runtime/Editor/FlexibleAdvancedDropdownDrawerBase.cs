#if UNITY_EDITOR
using System.Linq;
using FlexibleDropdown.Attributes;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FlexibleDropdown.Editor
{
    public abstract class FlexibleAdvancedDropdownDrawerBase<TAttribute, TValue> : PropertyDrawer
        where TAttribute : PropertyAttribute, IDropdownProvider<TValue>
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var customAttribute = (TAttribute)attribute;
            if (!FlexibleDropdownHelper.ValidateAttributeType<TValue>(fieldInfo))
            {
                return FlexibleDropdownHelper.CreateInvalidTypeLabel<TAttribute, TValue>();
            }

            var choices = customAttribute.Choices.ToArray();

            var currentValue = FlexibleDropdownHelper.GetCurrentValue<TValue>(property);
            currentValue ??= customAttribute.DefaultValue;
            var selectedIndex =
                FlexibleDropdownHelper.GetSelectedIndex(choices, currentValue, customAttribute.DefaultValue);

            var container = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    marginTop = 1,
                    marginBottom = 1,
                    marginLeft = customAttribute.IsPropertyFieldEnabled ? 0 : 3,
                    minHeight = EditorGUIUtility.singleLineHeight
                }
            };
            var button = CreateButton(property, customAttribute, choices, out var dummyField);

            VisualElement label;
            if (customAttribute.IsPropertyFieldEnabled)
            {
                var foldout = new Foldout
                {
                    text = property.displayName,
                    value = property.isExpanded,
                    style = { flexGrow = 1 }
                };
                foldout.RegisterValueChangedCallback(evt => { property.isExpanded = evt.newValue; });

                var propertyField = new PropertyField(property, property.displayName)
                {
                    style = { flexGrow = 1 }
                };
                propertyField.BindProperty(property);
                foldout.Add(propertyField);
                label = foldout;
            }
            else
            {
                label = new Label(property.displayName)
                {
                    style =
                    {
                        paddingTop = 2,
                        flexGrow = 1,
                        height = EditorGUIUtility.singleLineHeight
                    }
                };
                label.AddToClassList(ObjectField.labelUssClassName);
            }

            container.Add(label);
            container.Add(button);
            container.Add(dummyField);

            return container;
        }

        private IMGUIContainer CreateButton(SerializedProperty property, TAttribute customAttribute,
            TValue[] choices, out VisualElement dummyField)
        {
            var button = new IMGUIContainer(() =>
            {
                ShowButton(property, customAttribute, EditorGUILayout.GetControlRect(), choices);
            })
            {
                style =
                {
                    flexGrow = 1,
                    position = Position.Absolute,
                    right = -2,
                    height = EditorGUIUtility.singleLineHeight
                }
            };
            dummyField = EditorUIToolkitUtility.CreateDummyInputField(property);
            var dummyFieldInput = dummyField.Q(className: EditorUIToolkitUtility.DummyInputUssClassName);
            dummyFieldInput.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                button.style.width = dummyFieldInput.resolvedStyle.width;
            });
            return button;
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

            var buttonWidth = position.width - EditorGUIUtility.labelWidth;
            var buttonRect = new Rect(position)
            {
                x = position.xMax - buttonWidth,
                height = EditorGUIUtility.singleLineHeight,
                width = buttonWidth
            };

            EditorGUI.BeginProperty(position, label, property);
            {
                ShowButton(property, customAttribute, buttonRect, choices);

                if (customAttribute.IsPropertyFieldEnabled)
                {
                    FlexibleDropdownHelper.LabelAndPropertyField(position, property, label);
                }
                else
                {
                    GUI.Label(new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height), label);
                }
            }
            EditorGUI.EndProperty();
        }

        private void ShowButton(SerializedProperty property, TAttribute customAttribute, Rect buttonRect,
            TValue[] choices)
        {
            var currentValue = FlexibleDropdownHelper.GetCurrentValue<TValue>(property);
            currentValue ??= customAttribute.DefaultValue;
            var displayText = customAttribute.FormatSelectedValue(currentValue);

            if (GUI.Button(buttonRect, displayText, EditorStyles.popup))
            {
                ShowAdvancedDropdown(choices, property, customAttribute, buttonRect);
            }
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

        private void ShowAdvancedDropdown(TValue[] choices, SerializedProperty property, TAttribute customAttribute,
            Rect buttonRect)
        {
            var dropdown =
                new PathAdvancedDropdown<TValue>(choices, customAttribute.FormatListItem, new AdvancedDropdownState())
                {
                    OnItemSelected = selectedValue =>
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, FlexibleDropdownHelper.UndoMessage);
                        FlexibleDropdownHelper.SetValue(property, selectedValue);
                    }
                };
            dropdown.Show(buttonRect);
        }
    }
}
#endif