#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;

namespace FlexibleDropdown.Editor
{
    internal static class EditorUIToolkitUtility
    {
        public static readonly string DummyInputUssClassName = IntegerField.inputUssClassName;

        public static VisualElement CreateDummyInputField(SerializedProperty property)
        {
            var dummyField = new IntegerField(property.displayName)
            {
                style =
                {
                    flexGrow = 1,
                    visibility = Visibility.Hidden,
                    height = 0,
                    marginTop = 0,
                    marginBottom = 0
                }
            };
            dummyField.AddToClassList(IntegerField.alignedFieldUssClassName);
            return dummyField;
        }
    }
}
#endif