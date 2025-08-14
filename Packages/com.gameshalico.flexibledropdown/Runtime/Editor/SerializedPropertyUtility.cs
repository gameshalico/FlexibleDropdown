#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace FlexibleDropdown.Editor
{
    internal static class SerializedPropertyUtility
    {
        /// <summary>
        ///     Reflectionsを用いてSerializedProperty.propertyPathの値を取得する。
        /// </summary>
        public static object TracePropertyValue(this SerializedProperty property)
        {
            return TracePathParts(ParsePathParts(property.propertyPath), property.serializedObject.targetObject);
        }

        /// <summary>
        ///     Reflectionsを用いてSerializedProperty.propertyPathの値を設定する。
        /// </summary>
        public static void SetPropertyValue(this SerializedProperty property, object value)
        {
            var pathParts = ParsePathParts(property.propertyPath);
            object root = property.serializedObject.targetObject;

            var lastPart = pathParts[^1];
            var span = pathParts.AsSpan().Slice(0, pathParts.Length - 1);
            var parentObject = TracePathParts(span, root);

            if (lastPart.Index.HasValue)
            {
                var list = GetFieldValue(parentObject, lastPart.Name) as IList;
                if (list == null)
                {
                    throw new InvalidOperationException($"Field '{lastPart.Name}' is not a list or array.");
                }

                list[lastPart.Index.Value] = value;
            }
            else
            {
                var info = GetNextFieldInfo(lastPart, parentObject);
                if (info == null)
                {
                    throw new InvalidOperationException(
                        $"Field '{lastPart.Name}' not found in {parentObject.GetType()}.");
                }

                info.SetValue(parentObject, value);
            }
        }

        /// <summary>
        ///     SerializedProperty.propertyPathの親の値を取得する。
        /// </summary>
        public static object TraceParentValue(this SerializedProperty property)
        {
            var pathParts = ParsePathParts(property.propertyPath);
            var span = pathParts.AsSpan().Slice(0, pathParts.Length - 1);
            object root = property.serializedObject.targetObject;
            return TracePathParts(span, root);
        }

        private static object GetFieldValue(object source, string name)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source) + " is null.");
            }

            if (ReflectionUtility.TryFindFieldValue(source, name, out var value))
            {
                return value;
            }

            throw new ArgumentException(name + " is not a field of " + source.GetType());
        }

        private static object GetFieldValueWithIndex(object source, string name, int index)
        {
            var value = GetFieldValue(source, name);

            if (value == null)
            {
                return null;
            }

            var enumerable = value as IEnumerable;
            if (enumerable == null)
            {
                return null;
            }

            var enumerator = enumerable.GetEnumerator();
            using var disposable = enumerator as IDisposable;
            for (var i = 0; i <= index; i++)
            {
                if (!enumerator.MoveNext())
                {
                    return null;
                }
            }

            value = enumerator.Current;

            return value;
        }

        private static PathPart[] ParsePathParts(string propertyPath)
        {
            var pathPartStrings = propertyPath.Replace(".Array.data[", "[").Split('.');
            var pathParts = new PathPart[pathPartStrings.Length];

            for (var i = 0; i < pathPartStrings.Length; i++)
            {
                var part = pathPartStrings[i];
                if (part.Contains("["))
                {
                    var startIndex = part.IndexOf('[');
                    var endIndex = part.IndexOf(']');
                    var elementName = part.Substring(0, startIndex);

                    var index = Convert.ToInt32(part.Substring(startIndex + 1, endIndex - startIndex - 1));
                    pathParts[i] = new PathPart(elementName, index);
                }
                else
                {
                    pathParts[i] = new PathPart(part);
                }
            }

            return pathParts;
        }

        private static object GetNext(PathPart part, object targetObject)
        {
            return part.Index.HasValue
                ? GetFieldValueWithIndex(targetObject, part.Name, part.Index.Value)
                : GetFieldValue(targetObject, part.Name);
        }

        private static FieldInfo GetNextFieldInfo(PathPart part, object targetObject)
        {
            if (ReflectionUtility.TryFindFieldInfo(targetObject, part.Name, out var info))
            {
                return info;
            }

            return null;
        }

        private static object TracePathParts(Span<PathPart> pathParts, object targetObject)
        {
            var currentObject = targetObject;
            foreach (var part in pathParts)
            {
                currentObject = GetNext(part, currentObject);
            }

            return currentObject;
        }


        private readonly struct PathPart
        {
            public string Name { get; }
            public int? Index { get; }

            public PathPart(string name, int? index = null)
            {
                Name = name;
                Index = index;
            }

            public override string ToString()
            {
                return Index.HasValue ? $"{Name}[{Index.Value}]" : Name;
            }
        }
    }
}

#endif