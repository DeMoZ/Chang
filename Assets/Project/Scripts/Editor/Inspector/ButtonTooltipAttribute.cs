using System;
using Chang.Editor.Inspector;
using TriInspector;
using UnityEngine;

[assembly: RegisterTriAttributeDrawer(typeof(ButtonTooltipDrawer), TriDrawerOrder.Drawer + 1)]

namespace Chang.Editor.Inspector
{
    /// <summary>
    /// Shows a tooltip on hover over a Tri Inspector [Button].
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonTooltipAttribute : Attribute
    {
        public string Tooltip { get; }

        public ButtonTooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }
    }

    public class ButtonTooltipDrawer : TriAttributeDrawer<ButtonTooltipAttribute>
    {
        public override void OnGUI(Rect position, TriProperty property, TriElement next)
        {
            next.OnGUI(position);

            // the button is drawn at the bottom of the element, an empty label over it shows the tooltip
            float buttonHeight = property.TryGetAttribute(out ButtonAttribute button) && button.ButtonSize != 0
                ? button.ButtonSize
                : UnityEditor.EditorGUIUtility.singleLineHeight;

            Rect tooltipRect = new(position.x, position.yMax - buttonHeight, position.width, buttonHeight);
            GUI.Label(tooltipRect, new GUIContent(string.Empty, Attribute.Tooltip));
        }
    }
}
