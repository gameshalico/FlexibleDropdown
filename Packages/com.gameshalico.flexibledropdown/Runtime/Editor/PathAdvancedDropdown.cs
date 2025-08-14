#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace FlexibleDropdown.Editor
{
    internal class PathAdvancedDropdown<TValue> : AdvancedDropdown
    {
        private static readonly Vector2 DefaultMinimumSize = new(200, 300);
        private readonly Dictionary<int, TValue> _pathLookup = new();
        private readonly IReadOnlyCollection<TValue> _choices;
        private readonly Func<TValue, string> _formatListItem;
        public Action<TValue> OnItemSelected;

        public PathAdvancedDropdown(IReadOnlyCollection<TValue> choices, Func<TValue, string> formatListItem,
            Vector2 minimumSize,
            AdvancedDropdownState state)
            : base(state)
        {
            _choices = choices ?? new List<TValue>();
            _formatListItem = formatListItem ?? throw new ArgumentNullException(nameof(formatListItem));
            this.minimumSize = minimumSize;
        }

        public PathAdvancedDropdown(IReadOnlyCollection<TValue> choices, Func<TValue, string> formatListItem,
            AdvancedDropdownState state)
            : this(choices, formatListItem, DefaultMinimumSize, state)
        {
        }

        private AdvancedDropdownItem BuildHierarchy(IEnumerable<TValue> choices, Func<TValue, string> formatListItem)
        {
            var root = new AdvancedDropdownItem("Root");
            var pathNodes = new Dictionary<string, AdvancedDropdownItem>();

            foreach (var choice in choices)
            {
                var path = formatListItem(choice);

                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var currentPath = "";
                var currentParent = root;

                for (var i = 0; i < parts.Length; i++)
                {
                    var part = parts[i];
                    var parentPath = currentPath;
                    currentPath = string.IsNullOrEmpty(currentPath) ? part : $"{currentPath}/{part}";

                    if (!pathNodes.TryGetValue(currentPath, out var node))
                    {
                        node = new AdvancedDropdownItem(part);
                        pathNodes[currentPath] = node;
                        currentParent.AddChild(node);

                        if (i == parts.Length - 1)
                        {
                            _pathLookup[node.id] = choice;
                        }
                    }

                    currentParent = node;
                }
            }

            return root;
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            _pathLookup.Clear();
            return BuildHierarchy(_choices, _formatListItem);
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (_pathLookup.TryGetValue(item.id, out var selectedValue))
            {
                OnItemSelected?.Invoke(selectedValue);
            }
        }
    }
}
#endif