using UnityEngine;

namespace Sample
{
    public class DemoComponent : MonoBehaviour
    {
        [SerializeField]
        [SceneNameDropdown]
        private string _sceneName;
    }
}