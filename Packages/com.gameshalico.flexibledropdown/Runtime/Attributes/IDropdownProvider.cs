using System.Collections.Generic;

namespace FlexibleDropdown.Attributes
{
    public interface IDropdownProvider<TValue>
    {
        IEnumerable<TValue> Choices { get; }
        TValue DefaultValue => default;
        bool IsPropertyFieldEnabled => false;

        string FormatListItem(TValue item)
        {
            return item?.ToString() ?? string.Empty;
        }

        string FormatSelectedValue(TValue value)
        {
            return value?.ToString() ?? string.Empty;
        }
    }
}