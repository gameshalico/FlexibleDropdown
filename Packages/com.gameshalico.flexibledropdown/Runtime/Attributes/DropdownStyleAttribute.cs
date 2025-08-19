using System;
using UnityEngine;

namespace FlexibleDropdown.Attributes
{
    public enum DropdownStyle
    {
        /// <summary>
        ///     通常のPopupFieldスタイルのドロップダウン
        /// </summary>
        Standard,

        /// <summary>
        ///     AdvancedDropdownを使用した検索可能なドロップダウン
        /// </summary>
        Advanced
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DropdownStyleAttribute : PropertyAttribute
    {
        public DropdownStyleAttribute(DropdownStyle style = DropdownStyle.Standard)
        {
            Style = style;
        }

        public DropdownStyle Style { get; }
    }
}