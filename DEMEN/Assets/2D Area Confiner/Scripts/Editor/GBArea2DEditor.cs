using UnityEngine;
using UnityEditor;

namespace GBTools.Area2DConfiner
{
    [CustomEditor(typeof(GBArea2DConfiner))]
    public class GBArea2DConfinerEditor : Editor
    {
        GBArea2DConfiner myTarget;
        private void OnEnable()
        {
            myTarget = (GBArea2DConfiner)target;
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(GBStyleEditor.GetBoxWindow(Color.black, 0.4f));

            DrawHeaderTittle();
            DrawButtons();
            DrawGizmosSection();
            DrawSections();
            DrawShortCuts();

            EditorGUILayout.EndVertical();
        }

        private void DrawHeaderTittle()
        {
            var tittleStyle = GBStyleEditor.GetStyle(Color.white, TextAnchor.MiddleCenter, 12, FontStyle.Bold);
            EditorGUILayout.LabelField("- GB 2D Area Confiner 1.0 - ", tittleStyle);
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var tittleStyle = GBStyleEditor.GetStyle(Color.white, TextAnchor.MiddleCenter, 10, FontStyle.Italic);
            EditorGUILayout.LabelField("Confiner Control Buttons", tittleStyle);
            if (GUILayout.Button("Add Section"))
            {
                NewSection();
            }
            if (GUILayout.Button("Clear Confiner Area"))
            {
                myTarget.ClearPath();
                EditorUtility.SetDirty(myTarget);
            }
            EditorGUILayout.EndVertical();
        }

        private static void DrawShortCuts()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var tittleStyle = GBStyleEditor.GetStyle(Color.white, TextAnchor.MiddleCenter, 10, FontStyle.Italic);
            EditorGUILayout.LabelField("Shortcuts", tittleStyle);

            EditorGUILayout.LabelField("Shift   : Keep Y position");
            EditorGUILayout.LabelField("Control : up and down pos copy x center Position");
            EditorGUILayout.EndVertical();
        }

        private void DrawGizmosSection()
        {
            var subtittleStyle = GBStyleEditor.GetStyle(Color.white, TextAnchor.MiddleCenter, 10, FontStyle.Italic);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Gizmos", subtittleStyle);

            myTarget.handlesType = (EditorHandlesType)EditorGUILayout.EnumPopup("Handles Type", myTarget.handlesType);
            if (myTarget.handlesType == EditorHandlesType.Point)
                myTarget.handlesSize = EditorGUILayout.Slider("Handles Size: ", myTarget.handlesSize, 0.02f, 1f);
            myTarget.gizmosType = (GBArea2DGizmosType)EditorGUILayout.EnumPopup("Gizmos Type", myTarget.gizmosType);
            myTarget.gizmosColor = EditorGUILayout.ColorField("Gizmos Color: ", myTarget.gizmosColor);

            SceneView.RepaintAll();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            var styleButtons = GBStyleEditor.GetButtonStyle(Color.black);
            var labelStyleHandles = GBStyleEditor.GetStyle(Color.white, TextAnchor.MiddleCenter, 10, FontStyle.Normal);
            EditorGUILayout.LabelField("Active/Disable Handles", labelStyleHandles);
            if (myTarget.m_editUpPathPoints == false)
            {                
                if (GUILayout.Button("<color=red>●↑</color>", styleButtons))
                {
                    myTarget.m_editUpPathPoints = true;
                    EditorUtility.SetDirty(myTarget);
                }
            }
            else
            {
                if (GUILayout.Button("<color=green>●↑</color>", styleButtons))
                {
                    myTarget.m_editUpPathPoints = false;
                    EditorUtility.SetDirty(myTarget);
                }
            }
            if (myTarget.m_editPathPoints == false)
            {
                if (GUILayout.Button("<color=red>●→</color>", styleButtons))
                {
                    
                    myTarget.m_editPathPoints = true;
                    EditorUtility.SetDirty(myTarget);
                }
            }
            else
            {
                if (GUILayout.Button("<color=green>●→</color>", styleButtons))
                {
                    myTarget.m_editPathPoints = false;
                    EditorUtility.SetDirty(myTarget);
                }
            }
            if (myTarget.m_editDownPathPoints == false)
            {
                if (GUILayout.Button("<color=red>●↓</color>", styleButtons))
                {
                    myTarget.m_editDownPathPoints = true;
                    EditorUtility.SetDirty(myTarget);
                }
            }
            else
            {
                if (GUILayout.Button("<color=green>●↓</color>", styleButtons))
                {
                    myTarget.m_editDownPathPoints = false;
                    EditorUtility.SetDirty(myTarget);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

        }

        private void DrawSections()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            var subtittleStyle = GBStyleEditor.GetStyle(Color.white, TextAnchor.MiddleCenter, 10, FontStyle.Italic);
            EditorGUILayout.LabelField("Sections", subtittleStyle);

            var buttonStyle = GBStyleEditor.GetButtonStyle(Color.black);

            var text = "Show Sections";
            if (myTarget.showSections)
                text = "Hide Sections";

            if (GUILayout.Button(text + ": "+myTarget.sections.Count))
            {
                myTarget.showSections = !myTarget.showSections;
            }
            if (myTarget.showSections)
            {

                for (int i = 0; i < myTarget.sections.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Section [" + i + "]", buttonStyle))
                        myTarget.sections[i].showPoint = !myTarget.sections[i].showPoint;
                    if (GUILayout.Button("Delete", buttonStyle))
                    {
                        myTarget.sections.RemoveAt(i);
                        return;
                    }
                    EditorGUILayout.EndHorizontal();

                    Repaint();
                    if (myTarget.sections[i].showPoint)
                    {
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        myTarget.sections[i].point = EditorGUILayout.Vector3Field("Center: ", myTarget.sections[i].point);
                        myTarget.sections[i].limitUp = EditorGUILayout.Vector3Field("Up: ", myTarget.sections[i].limitUp);
                        myTarget.sections[i].limitDown = EditorGUILayout.Vector3Field("Down: ", myTarget.sections[i].limitDown);
                        EditorGUILayout.EndVertical();
                    }
                   

                }
            }
            EditorGUILayout.EndVertical();

        }

        private void NewSection()
        {
            bool isFristPoint = false;
            var newSection = new GBArea2DConfinerSection();
            var newPos = myTarget.transform.position;
            if (myTarget.sections.Count > 0)
            {
                newPos = myTarget.sections[myTarget.sections.Count - 1].point + new Vector3(5, 0);
                var posUp = myTarget.sections[myTarget.sections.Count - 1].limitUp + new Vector3(5, 0) /*+ new Vector3(0, myTarget.extendsYPath);*/;
                var posDown = myTarget.sections[myTarget.sections.Count - 1].limitDown + new Vector3(5, 0) /* newPos - new Vector3(0, myTarget.extendsYPath)*/;

                newSection.point = newPos;
                newSection.limitUp = posUp;
                newSection.limitDown = posDown;
            }
            else
            {
                newSection.point = newPos;
                newSection.limitUp = newPos + new Vector3(0, 5);
                newSection.limitDown = newPos + new Vector3(0, -5);
                isFristPoint = true;
            }

            myTarget.sections.Add(newSection);

            EditorUtility.SetDirty(myTarget);
            if (isFristPoint)
                NewSection();
        }

        protected virtual void OnSceneGUI()
        {
            //hiding gameobject positionHadles
            Tools.current = Tool.None;
            CheckInput();

            if (myTarget.m_editPathPoints)
            {
                for (int i = 0; i < myTarget.sections.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    GBHandlesFreeCircle.DragHandleResult Result;
                    var prevY = myTarget.sections[i].point.y;
                    UnityEditor.Handles.color = Color.green;
                
                    var newTargetPosition = Vector2.zero;
                    if (myTarget.handlesType == EditorHandlesType.Point)
                        newTargetPosition = GBHandlesFreeCircle.DragHandle(myTarget.sections[i].point, myTarget.handlesSize, UnityEditor.Handles.SphereHandleCap, Color.green, Color.red, out Result);
                    else
                        newTargetPosition = UnityEditor.Handles.PositionHandle(myTarget.sections[i].point, Quaternion.identity);

                    if (isShiftPress)
                        newTargetPosition.y = prevY;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(myTarget, "Change Look At Target Position");
                        myTarget.sections[i].point = newTargetPosition;
                        if (isControlPress)
                            myTarget.UpdateLimitUPDownToCenter(i);
                        EditorUtility.SetDirty(myTarget);
                    }
                }
            }
            if (myTarget.m_editUpPathPoints)
            {
                for (int i = 0; i < myTarget.sections.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    GBHandlesFreeCircle.DragHandleResult Result;
                    var prevY = myTarget.sections[i].limitUp.y;
                    UnityEditor.Handles.color = Color.blue;

                    var newTargetPosition = Vector2.zero;
                    if (myTarget.handlesType == EditorHandlesType.Point)
                        newTargetPosition = GBHandlesFreeCircle.DragHandle(myTarget.sections[i].limitUp, myTarget.handlesSize, UnityEditor.Handles.SphereHandleCap, Color.blue, Color.red, out Result);
                    else
                        newTargetPosition = UnityEditor.Handles.PositionHandle(myTarget.sections[i].limitUp, Quaternion.identity);

                    if (isShiftPress)
                        newTargetPosition.y = prevY;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(myTarget, "Change Look At Target Position");
                        myTarget.sections[i].limitUp = newTargetPosition;
                        EditorUtility.SetDirty(myTarget);
                    }
                }
            }
            if (myTarget.m_editDownPathPoints)
            {
                for (int i = 0; i < myTarget.sections.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    GBHandlesFreeCircle.DragHandleResult Result;
                    var prevY = myTarget.sections[i].limitDown.y;
                    UnityEditor.Handles.color = Color.blue;

                    var newTargetPosition = Vector2.zero;
                    if (myTarget.handlesType == EditorHandlesType.Point)
                        newTargetPosition = GBHandlesFreeCircle.DragHandle(myTarget.sections[i].limitDown, myTarget.handlesSize, UnityEditor.Handles.SphereHandleCap, Color.blue, Color.red, out Result);
                    else
                        newTargetPosition = UnityEditor.Handles.PositionHandle(myTarget.sections[i].limitDown, Quaternion.identity);

                    if (isShiftPress)
                        newTargetPosition.y = prevY;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(myTarget, "Change Look At Target Position");
                        myTarget.sections[i].limitDown = newTargetPosition;
                    }
                }
            }

        }
        bool isShiftPress;
        bool isControlPress;
        private void CheckInput()
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == (KeyCode.LeftShift))
                        {
                            isShiftPress = true;
                            e.Use();
                        }
                        if (Event.current.keyCode == (KeyCode.LeftControl))
                        {
                            isControlPress = true;
                            e.Use();
                        }
                        break;
                    }
                case EventType.KeyUp:
                    {
                        if (Event.current.keyCode == (KeyCode.LeftShift))
                        {
                            isShiftPress = false;
                            e.Use();
                        }
                        if (Event.current.keyCode == (KeyCode.LeftControl))
                        {
                            isControlPress = false;
                            e.Use();
                        }
                        break;
                    }
            }
        }
    }
}

