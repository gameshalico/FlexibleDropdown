using System;
using System.Collections.Generic;
using FlexibleDropdown.Attributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sample
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SceneNameDropdownAttribute : PropertyAttribute, IDropdownProvider<string>
    {
        public IEnumerable<string> Choices
        {
            get
            {
#if UNITY_EDITOR
                foreach (var guid in AssetDatabase.FindAssets("t:Scene"))
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var scene = AssetDatabase.LoadMainAssetAtPath(path) as SceneAsset;
                    if (scene != null)
                    {
                        yield return scene.name;
                    }
                }
#else
                yield break;
#endif
            }
        }
    }
}