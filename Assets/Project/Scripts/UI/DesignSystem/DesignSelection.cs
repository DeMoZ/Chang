using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace Chang.UI.DesignSystem
{
    /// <summary>
    /// Highlights one item of a group (segmented control, politeness options…) when the design has no variants for it:
    /// the look of the item drawn as selected in the design and of an item drawn as normal are captured on first use,
    /// then <see cref="Select"/> paints every item with one of the two looks. Items must share the same layer structure.
    /// </summary>
    [DisallowMultipleComponent]
    public class DesignSelection : MonoBehaviour
    {
        [SerializeField] private List<RectTransform> _items = new();
        [SerializeField] private int _designSelectedIndex;
        [SerializeField] private int _designNormalIndex = 1;

        private struct Look
        {
            public bool HasColor;
            public Color Color;
            public float Border;
        }

        private Dictionary<string, Look> _selected;
        private Dictionary<string, Look> _normal;
        private int _current = -1;

        public event Action<int> Clicked;

        public int Current => _current;
        public int Count => _items.Count;

        private void Awake()
        {
            Capture();
            for (var i = 0; i < _items.Count; i++)
            {
                var index = i;
                var button = _items[i].GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => Clicked?.Invoke(index));
                }
            }
        }

        public void Select(int index)
        {
            Capture();
            for (var i = 0; i < _items.Count; i++)
            {
                Paint(_items[i], i == index ? _selected : _normal);
            }

            _current = index;
        }

        private void Capture()
        {
            if (_selected != null || _items.Count < 2)
            {
                return;
            }

            _selected = Snapshot(_items[Mathf.Clamp(_designSelectedIndex, 0, _items.Count - 1)]);
            _normal = Snapshot(_items[Mathf.Clamp(_designNormalIndex, 0, _items.Count - 1)]);
        }

        private static Dictionary<string, Look> Snapshot(Transform root)
        {
            var result = new Dictionary<string, Look>();
            Walk(root, "", (path, t) =>
            {
                var look = new Look();
                var graphic = t.GetComponent<Graphic>();
                if (graphic != null)
                {
                    look.HasColor = true;
                    look.Color = graphic.color;
                }

                if (graphic is ProceduralImage proc)
                {
                    look.Border = proc.BorderWidth;
                }

                result[path] = look;
            });
            return result;
        }

        private static void Paint(Transform root, Dictionary<string, Look> looks)
        {
            Walk(root, "", (path, t) =>
            {
                if (!looks.TryGetValue(path, out var look) || !look.HasColor)
                {
                    return;
                }

                var graphic = t.GetComponent<Graphic>();
                if (graphic == null)
                {
                    return;
                }

                graphic.color = look.Color;
                if (graphic is ProceduralImage proc)
                {
                    proc.BorderWidth = look.Border;
                }
            });
        }

        private static void Walk(Transform t, string path, Action<string, Transform> visit)
        {
            visit(path, t);
            // Items differ by content (texts, icons named after it), so layers are matched by position.
            for (var i = 0; i < t.childCount; i++)
            {
                Walk(t.GetChild(i), path + "/" + i, visit);
            }
        }
    }
}
