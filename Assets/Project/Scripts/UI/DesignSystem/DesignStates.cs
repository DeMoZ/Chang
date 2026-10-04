using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace Chang.UI.DesignSystem
{
    /// <summary>
    /// Switches the look of an element between the states drawn in Penpot as component variants
    /// (OptionCard Default/Selected/Correct/Wrong, LessonNode Score 0…100%, TabBar Words/Sentences/…).
    /// Each state references its variant prefab; <see cref="Apply"/> copies the look (colors, sprites, borders,
    /// visibility) from that prefab onto this object, matching objects by their path. So a variant edited in Penpot
    /// and re-imported changes the runtime state without code changes.
    /// Put it on an instance of the variant that has the most layers: layers missing in a state are hidden.
    /// </summary>
    [DisallowMultipleComponent]
    public class DesignStates : MonoBehaviour
    {
        [Serializable]
        public class State
        {
            public string Name;
            public GameObject Prefab;
        }

        [SerializeField] private List<State> _states = new();

        [Tooltip("Texts whose content belongs to the state (e.g. a feedback title). Other texts are data and keep their content.")]
        [SerializeField] private List<TMP_Text> _stateTexts = new();

        [Tooltip("Objects whose visibility is controlled by code, not by the state.")]
        [SerializeField] private List<GameObject> _codeVisibility = new();

        [SerializeField] private string _initialState;

        private Dictionary<string, Transform> _self;
        private readonly Dictionary<string, Dictionary<string, Transform>> _statePaths = new();
        private string _current;

        public string Current => _current;

        public bool Has(string stateName) => _states.Exists(s => s.Name == stateName);

        private void Awake()
        {
            if (string.IsNullOrEmpty(_current) && !string.IsNullOrEmpty(_initialState))
            {
                Apply(_initialState);
            }
        }

        public void Apply(string stateName)
        {
            var state = _states.Find(s => s.Name == stateName);
            if (state == null || state.Prefab == null)
            {
                return;
            }

            _self ??= BuildPaths(transform);
            if (!_statePaths.TryGetValue(stateName, out var source))
            {
                source = BuildPaths(state.Prefab.transform);
                _statePaths[stateName] = source;
            }

            foreach (var (path, target) in _self)
            {
                if (target == null)
                {
                    continue;
                }

                var codeOwned = _codeVisibility.Contains(target.gameObject);
                if (!source.TryGetValue(path, out var from))
                {
                    if (!codeOwned && path.Length > 0)
                    {
                        target.gameObject.SetActive(false);
                    }

                    continue;
                }

                if (!codeOwned && path.Length > 0 && target.gameObject.activeSelf != from.gameObject.activeSelf)
                {
                    target.gameObject.SetActive(from.gameObject.activeSelf);
                }

                CopyLook(from, target);
            }

            _current = stateName;
        }

        private void CopyLook(Transform from, Transform to)
        {
            var src = from.GetComponent<Graphic>();
            var dst = to.GetComponent<Graphic>();
            if (src != null && dst != null)
            {
                dst.color = src.color;

                if (src is Image srcImage && dst is Image dstImage)
                {
                    if (srcImage.sprite != null && !(srcImage is ProceduralImage))
                    {
                        dstImage.sprite = srcImage.sprite;
                    }

                    if (srcImage is ProceduralImage srcProc && dstImage is ProceduralImage dstProc)
                    {
                        dstProc.BorderWidth = srcProc.BorderWidth;
                        var srcMod = from.GetComponent<FreeModifier>();
                        var dstMod = to.GetComponent<FreeModifier>();
                        if (srcMod != null && dstMod != null)
                        {
                            dstMod.Radius = srcMod.Radius;
                        }
                    }
                }

                if (src is TMP_Text srcText && dst is TMP_Text dstText && _stateTexts.Contains(dstText))
                {
                    dstText.text = srcText.text;
                }
            }

            var srcGroup = from.GetComponent<CanvasGroup>();
            var dstGroup = to.GetComponent<CanvasGroup>();
            if (dstGroup != null)
            {
                dstGroup.alpha = srcGroup != null ? srcGroup.alpha : 1f;
            }
        }

        /// <summary>Relative path → transform. Siblings with equal names get a "#n" suffix.</summary>
        private static Dictionary<string, Transform> BuildPaths(Transform root)
        {
            var result = new Dictionary<string, Transform> { [""] = root };
            Collect(root, "", result);
            return result;
        }

        private static void Collect(Transform parent, string prefix, Dictionary<string, Transform> result)
        {
            var seen = new Dictionary<string, int>();
            foreach (Transform child in parent)
            {
                seen.TryGetValue(child.name, out var n);
                seen[child.name] = n + 1;
                var path = prefix + "/" + child.name + (n > 0 ? "#" + n : "");
                result[path] = child;
                Collect(child, path, result);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Apply next state")]
        private void ApplyNextState()
        {
            if (_states.Count == 0)
            {
                return;
            }

            var index = (_states.FindIndex(s => s.Name == _current) + 1) % _states.Count;
            Apply(_states[index].Name);
        }
#endif
    }
}
