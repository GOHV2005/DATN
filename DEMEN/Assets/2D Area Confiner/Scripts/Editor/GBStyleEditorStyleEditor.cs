#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


namespace GBTools.Area2DConfiner
{
    public static class GBStyleEditor
    {
        public static GUIStyle GetStyle(Color color, TextAnchor align = TextAnchor.MiddleCenter, int size = 11, FontStyle st = FontStyle.Normal)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = color;
            style.alignment = align;
            style.fontSize = size;
            style.fontStyle = st;
            style.richText = true;
            return style;
        }
        public static GUIStyle SetBox(this GUIStyle style, Color color, float alpha)
        {

            var bk = new Texture2D(1, 1);
            color.a = alpha;
            bk.SetPixel(0, 0, color);
            bk.Apply();
            style.normal.background = bk;
            style.stretchWidth = true;
            style.stretchHeight = true;
            return style;
        }
        public static GUIStyle GetBoxWindow(Color color, float alpha = 1)
        {
            GUIStyle styleBox = new GUIStyle(GUI.skin.box);
            styleBox.normal.background = GBStyleEditor.GenerateTexture(color, alpha);
            return styleBox;
        }
        public static GUIStyle GetButtonStyle(Color color)
        {
            GUIStyle styleBox = new GUIStyle(GUI.skin.button);
            styleBox.richText = true;
            styleBox.normal.background = GBStyleEditor.GenerateTexture(color, .5f);
            styleBox.hover.background = GBStyleEditor.GenerateTexture(color, .7f);
            var activeCOlor = Color.Lerp(color, Color.black, 0.5f);
            styleBox.active.background = GBStyleEditor.GenerateTexture(activeCOlor, .7f);
            //styleBox..background = StyleEditor.GenerateTexture(color, 1f);
            return styleBox;
        }
        public static GUIStyle GetButtonStyle()
        {
            GUIStyle styleBox = new GUIStyle(GUI.skin.button);
            return styleBox;
        }
        public static GUIStyle SetButtonColor(this GUIStyle style, Color color, float alpha)
        {
            // style.b.GenerateTexture(color, alpha);
            //style..background.GenerateTexture(color, alpha);
            // style.onHover.background.GenerateTexture(color, alpha);
            return style;
        }
        public static GUIStyle GetCustomStyle(string style)
        {
            var path = AssetDatabase.FindAssets("CustomEditorSkin");
            var t = AssetDatabase.GUIDToAssetPath(path[0]);
            GUISkin styleBase = AssetDatabase.LoadAssetAtPath<GUISkin>(t);
            return styleBase.GetStyle(style);
        }
        public static Texture2D GenerateTexture(this Texture2D text, Color color, float alpha)
        {
            var bk = new Texture2D(1, 1);
            color.a = alpha;
            bk.SetPixel(0, 0, color);
            bk.Apply();
            return text;
        }
        public static Texture2D GenerateTexture(Color color, float alpha)
        {
            var bk = new Texture2D(1, 1);
            color.a = alpha;
            bk.SetPixel(0, 0, color);
            bk.Apply();
            return bk;
        }
    }
}
#endif
