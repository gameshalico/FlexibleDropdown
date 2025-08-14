#if UNITY_EDITOR
using System;
using System.Reflection;

namespace FlexibleDropdown.Editor
{
    internal static class ReflectionUtility
    {
        public const BindingFlags FindAllBindingFlags =
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
            BindingFlags.Static;

        public static bool TryFindFieldOrPropertyValue(object source, string name, out object value,
            BindingFlags bindingFlags = FindAllBindingFlags)
        {
            value = null;
            if (source == null)
            {
                return false;
            }

            var type = source.GetType();

            while (type != null)
            {
                var field = type.GetField(name, bindingFlags);
                if (field != null)
                {
                    value = field.GetValue(source);
                    return true;
                }

                var property = type.GetProperty(name, bindingFlags);
                if (property != null)
                {
                    value = property.GetValue(source);
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        public static bool TryFindFieldValue(object source, string name, out object value,
            BindingFlags bindingFlags = FindAllBindingFlags)
        {
            value = null;
            if (source == null)
            {
                return false;
            }

            var type = source.GetType();

            while (type != null)
            {
                var field = type.GetField(name, bindingFlags);
                if (field != null)
                {
                    value = field.GetValue(source);
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        public static bool TryFindFieldInfo(object source, string name, out FieldInfo field,
            BindingFlags bindingFlags = FindAllBindingFlags)
        {
            field = null;
            if (source == null)
            {
                return false;
            }

            var sourceType = source.GetType();

            while (sourceType != null)
            {
                field = sourceType.GetField(name, bindingFlags);
                if (field != null)
                {
                    return true;
                }

                sourceType = sourceType.BaseType;
            }

            return false;
        }


        public static Type GetFieldElementType(FieldInfo info)
        {
            if (info.FieldType.IsArray)
            {
                return info.FieldType.GetElementType();
            }

            if (info.FieldType.IsGenericType)
            {
                return info.FieldType.GetGenericArguments()[0];
            }

            return info.FieldType;
        }
    }
}

#endif