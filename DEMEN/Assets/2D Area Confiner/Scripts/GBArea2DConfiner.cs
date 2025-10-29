using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace GBTools.Area2DConfiner
{
    public class GBArea2DConfiner : MonoBehaviour
    {
        public float handlesSize;
        public bool showSections = false;
        public List<GBArea2DConfinerSection> sections = new List<GBArea2DConfinerSection>();
        public bool activeConstrains = true;
        public EditorHandlesType handlesType;
        public GBArea2DGizmosType gizmosType;
        public Color gizmosColor = Color.green;

        #region Vars for Editor
        [HideInInspector] public bool m_editPathPoints = true;
        [HideInInspector] public bool m_editUpPathPoints = true;
        [HideInInspector] public bool m_editDownPathPoints = true;
        #endregion


        public void ClearPath()
        {
            sections.Clear();
        }
       
        public void UpdateLimitsUpDownToCenter()
        {
            for (int i = 0; i < sections.Count; i++)
            {
                sections[i].limitUp = new Vector3(sections[i].point.x, sections[i].limitUp.y);
                sections[i].limitDown = new Vector3(sections[i].point.x, sections[i].limitDown.y);
            }
        }

        public void UpdateLimitUPDownToCenter(int i)
        {
            sections[i].limitUp = new Vector3(sections[i].point.x, sections[i].limitUp.y);
            sections[i].limitDown = new Vector3(sections[i].point.x, sections[i].limitDown.y);
        }

        public GBArea2DConfinerSection GetCurrentPathSection(Transform transform)
        {
            var cPoint = GetCurrentPoint(transform);
            if (cPoint >= 0)
            {
                return sections[cPoint];
            }
            return null;
        }

        public Vector3 GetCenterSection(Transform transform)
        {
            var currentIndexPoint = GetCurrentPoint(transform);
            return Vector3.Lerp(sections[currentIndexPoint].point, sections[currentIndexPoint + 1].point, 0.5f);
        }
        /// <summary>
        /// Get Current sections index with Transform position
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        public int GetCurrentPoint(Transform transform)
        {
            for (int i = 0; i < sections.Count; i++)
            {
                if (i <= sections.Count - 2)
                {
                    if (transform.position.x >= sections[i].GetMinX && transform.position.x < sections[i + 1].GetMaxX)
                    {
                        return i;
                    }
                    if (transform.position.x >= sections[i].GetMinX && transform.position.x < sections[i + 1].GetMaxX)
                    {
                        return i;
                    }
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        /// <summary>
        /// Get Current sections index with Vector3 position 
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int GetCurrentPoint(Vector3 position)
        {
            for (int i = 0; i < sections.Count; i++)
            {
                if (i <= sections.Count - 2)
                {
                    if (position.x >= sections[i].GetMinX && position.x < sections[i + 1].GetMaxX)
                    {
                        return i;
                    }
                    if (position.x >= sections[i].GetMinX && position.x < sections[i + 1].GetMaxX)
                    {
                        return i;
                    }
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }
        /// <summary>
        /// Constrains pointUpFix desired Transform to pointUpFix Section Area
        /// </summary>
        /// <param name="desiredPosition">Target position checks</param>
        /// <param name="transformCheck">this Transform is used to get section index</param>
        /// <returns>A valid position on section</returns>
        public Vector3 ConstrainsTransform(Vector3 desiredPosition, Transform transformCheck)
        {
            var currentPoint = GetCurrentPoint(transformCheck);
            var resultPoint = GetConstrainsVectorOnPath(desiredPosition, currentPoint, transformCheck);
            var pointWithObstacle = CheckObstacle(resultPoint, transformCheck);

            if (pointWithObstacle.hitObstacle)
            {
                resultPoint = pointWithObstacle.point;
            }

            return resultPoint;
        }
        /// <summary>
        /// Constrains pointUpFix desired Vector3 to pointUpFix Section Area
        /// </summary>
        /// <param name="desiredPosition">Target position checks</param>
        /// <param name="baseCheck">this Poisiton is used to get section index</param>
        /// <returns>A valid position on section</returns>
        public Vector3 ConstrainsTransform(Vector3 desiredPosition, Vector3 baseCheck)
        {
            var currentPoint = GetCurrentPoint(baseCheck);
            var resultPoint = GetConstrainsVectorOnPath(desiredPosition, currentPoint, baseCheck);
            var pointWithObstacle = CheckObstacle(resultPoint, baseCheck);

            if (pointWithObstacle.hitObstacle)
            {
                resultPoint = pointWithObstacle.point;
            }

            return resultPoint;
        }

        public Vector3 ConstrainsTransform(Vector3 desiredPosition, Transform transformCheck, float offsetMutipler)
        {
            var currentPoint = GetCurrentPoint(transformCheck);
            if (currentPoint < 0)
                currentPoint = 0;
            return GetConstrainsVectorOnPath(desiredPosition, currentPoint, transformCheck, offsetMutipler);
        }

        private GBArea2DConfinerObstacleData CheckObstacle(Vector3 point, Transform transformCheck)
        {
            var hits = Physics2D.OverlapCircleAll(point, 0.2f);
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    var obstacle = hit.transform.GetComponent<GBArea2DObstacle>();
                    if (obstacle)
                    {
                        Debug.Log("baseCheck : " + transformCheck.name);
                        var pResult = obstacle.coll.bounds.ClosestPoint(transformCheck.position);

                        var dir = transformCheck.position - pResult;

                        dir = dir.normalized;
                        //    dir.y = 0;
                        return new GBArea2DConfinerObstacleData(true, pResult + (dir * 0.2f));

                    }
                }

            }
            return new GBArea2DConfinerObstacleData(false, Vector3.zero);
        }
        private GBArea2DConfinerObstacleData CheckObstacle(Vector3 point, Vector3 positionCheck)
        {
            var hits = Physics2D.OverlapCircleAll(point, 0.2f);
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    var obstacle = hit.transform.GetComponent<GBArea2DObstacle>();
                    if (obstacle)
                    {
                        var pResult = obstacle.coll.bounds.ClosestPoint(positionCheck);
                        var dir = positionCheck - pResult;
                        dir = dir.normalized;
                        return new GBArea2DConfinerObstacleData(true, pResult + (dir * 0.2f));
                    }
                }
            }
            return new GBArea2DConfinerObstacleData(false, Vector3.zero);
        }

        private Vector3 GetConstrainsVectorOnPath(Vector3 targetPosition, int currentPoint, Transform transformCheck, float offsetMutipler = 0)
        {
            var lasPoint = sections.Count - 1;
            var firstAviablePoint = 0;

            if (activeConstrains)
            {
                var limitLef = GetLimitLeft(firstAviablePoint, transformCheck, offsetMutipler);
                var limitRight = GetLimitRight(lasPoint, transformCheck, offsetMutipler);
                var limitDown = GetLimitDown(currentPoint, transformCheck, offsetMutipler);
                var limitUp = GetLimitUP(currentPoint, transformCheck, offsetMutipler);

                targetPosition.y = Mathf.Clamp(targetPosition.y, limitDown, limitUp);
                targetPosition.x = Mathf.Clamp(targetPosition.x, limitLef, limitRight);
            }
            return targetPosition;
        }
        private Vector3 GetConstrainsVectorOnPath(Vector3 targetPosition, int currentPoint, Vector3 positionCheck, float offsetMutipler = 0)
        {
            var lasPoint = sections.Count - 1;
            var firstAviablePoint = 0;

            if (activeConstrains)
            {
                var limitLef = GetLimitLeft(firstAviablePoint, positionCheck, offsetMutipler);
                var limitRight = GetLimitRight(lasPoint, positionCheck, offsetMutipler);
                var limitDown = GetLimitDown(currentPoint, positionCheck, offsetMutipler);
                var limitUp = GetLimitUP(currentPoint, positionCheck, offsetMutipler);

                

                targetPosition.y = Mathf.Clamp(targetPosition.y, limitDown, limitUp);
                targetPosition.x = Mathf.Clamp(targetPosition.x, limitLef, limitRight);
            }
            return targetPosition;
        }
        private float GetLimitUP(int point, Transform transformCheck, float offsetMultipler = 0)
        {
            var pointUp = sections[point].limitUp;
            var pointCenter = sections[point].point;

            var pointCenterNext = sections[point+1].point;
            var pointUPNext = sections[point + 1].limitUp;

            var lerpVectorTarget = pointUPNext;
            var lerpVectorFrom = pointUp;

            if (transformCheck.position.x < pointUp.x && point == 0)
                lerpVectorTarget = pointCenter;

            if (point == sections.Count - 2 && transformCheck.position.x > pointUPNext.x)
            {
                if (pointCenterNext.x > pointUPNext.x)
                {
                    lerpVectorTarget = pointCenterNext;
                    lerpVectorFrom = pointUPNext;
                }
            }
            return Vector3.Lerp(lerpVectorFrom, lerpVectorTarget, Mathf.InverseLerp(lerpVectorFrom.x, lerpVectorTarget.x, transformCheck.position.x)).y;
        }
        private float GetLimitUP(int point, Vector3 positionCheck, float offsetMultipler = 0)
        {
            var pointUp = sections[point].limitUp;
            var pointCenter = sections[point].point;

            var pointCenterNext = sections[point + 1].point;
            var pointUPNext = sections[point + 1].limitUp;

            var lerpVectorTarget = pointUPNext;
            var lerpVectorFrom = pointUp;

            if (positionCheck.x < pointUp.x && point == 0)
                lerpVectorTarget = pointCenter;

            if (point == sections.Count - 2 && positionCheck.x > pointUPNext.x)
            {
                if (pointCenterNext.x > pointUPNext.x)
                {
                    lerpVectorTarget = pointCenterNext;
                    lerpVectorFrom = pointUPNext;
                }
            }
            return Vector3.Lerp(lerpVectorFrom, lerpVectorTarget, Mathf.InverseLerp(lerpVectorFrom.x, lerpVectorTarget.x, positionCheck.x)).y;
        }
        private float GetLimitDown(int point, Transform transformCheck, float offsetMultipler = 0)
        {
            var pointDown = sections[point].limitDown;
            var pointCenter = sections[point].point;

            var pointDownNext = sections[point + 1].limitDown;
            var pointCenterNext = sections[point+1].point;

            var lerpVectorTarget = pointDownNext;
            var lerpVectorFrom = pointDown;

            if (transformCheck.position.x < pointDown.x && point == 0)
                lerpVectorTarget = pointCenter;

            if (point == sections.Count -2 && transformCheck.position.x > pointDownNext.x)
            {
                if (pointCenterNext.x > pointDownNext.x)
                {
                    lerpVectorTarget = pointCenterNext;
                    lerpVectorFrom = pointDownNext;
                }
            }

            return Vector3.Lerp(lerpVectorFrom, lerpVectorTarget, Mathf.InverseLerp(lerpVectorFrom.x, lerpVectorTarget.x, transformCheck.position.x)).y;
        }
        private float GetLimitDown(int point, Vector3 positionCheck, float offsetMultipler = 0)
        {
            var pointDown = sections[point].limitDown;
            var pointCenter = sections[point].point;

            var pointDownNext = sections[point + 1].limitDown;
            var pointCenterNext = sections[point + 1].point;

            var lerpVectorTarget = pointDownNext;
            var lerpVectorFrom = pointDown;

            if (positionCheck.x < pointDown.x && point == 0)
                lerpVectorTarget = pointCenter;

            if (point == sections.Count - 2 && positionCheck.x > pointDownNext.x)
            {
                if (pointCenterNext.x > pointDownNext.x)
                {
                    lerpVectorTarget = pointCenterNext;
                    lerpVectorFrom = pointDownNext;
                }
            }

            return Vector3.Lerp(lerpVectorFrom, lerpVectorTarget, Mathf.InverseLerp(lerpVectorFrom.x, lerpVectorTarget.x, positionCheck.x)).y;
        }
        private float GetLimitLeft(int point, Transform transformCheck, float offsetMultipler = 0)
        {
            var pointUp = sections[point].limitUp;
            var pointDown = sections[point].limitDown;
            var pointCenter = sections[point].point;

            var pointUpFix = pointUp;
            if (pointCenter.x < pointUpFix.x)
                pointUpFix = pointCenter;

            var pointDownFix = pointDown;
            if (pointCenter.x < pointDownFix.x)
                pointDownFix = pointCenter;

            return Vector3.Lerp(pointUpFix, pointDownFix, Mathf.InverseLerp(pointUpFix.y, pointDownFix.y, transformCheck.position.y)).x + 0.01f + offsetMultipler;
        }
        private float GetLimitLeft(int point, Vector3 positionCheck, float offsetMultipler = 0)
        {
            var pointUp = sections[point].limitUp;
            var pointDown = sections[point].limitDown;
            var pointCenter = sections[point].point;

            var pointUpFix = pointUp;
            if (pointCenter.x < pointUpFix.x)
                pointUpFix = pointCenter;

            var pointDownFix = pointDown;
            if (pointCenter.x < pointDownFix.x)
                pointDownFix = pointCenter;

            return Vector3.Lerp(pointUpFix, pointDownFix, Mathf.InverseLerp(pointUpFix.y, pointDownFix.y, positionCheck.y)).x + 0.01f + offsetMultipler;
        }
        private float GetLimitRight(int point, Transform transformCheck, float offsetMultipler = 0)
        {
            var pointUp = sections[point].limitUp;
            var pointDown = sections[point].limitDown;
            var pointCenter = sections[point].point;

            var pointUpFix = pointUp;
            if (pointCenter.x > pointUpFix.x)
                pointUpFix = pointCenter;

            var pointDownFix = pointDown;
            if (pointCenter.x > pointDownFix.x)
                pointDownFix = pointCenter;

            return Vector3.Lerp(pointUpFix, pointDownFix, Mathf.InverseLerp(pointUpFix.y, pointDownFix.y, transformCheck.position.y)).x - 0.01f + offsetMultipler;
        }
        private float GetLimitRight(int point, Vector3 positionCheck, float offsetMultipler = 0)
        {
            var pointUp = sections[point].limitUp;
            var pointDown = sections[point].limitDown;
            var pointCenter = sections[point].point;

            var pointUpFix = pointUp;
            if (pointCenter.x > pointUpFix.x)
                pointUpFix = pointCenter;

            var pointDownFix = pointDown;
            if (pointCenter.x > pointDownFix.x)
                pointDownFix = pointCenter;

            return Vector3.Lerp(pointUpFix, pointDownFix, Mathf.InverseLerp(pointUpFix.y, pointDownFix.y, positionCheck.y)).x - 0.01f + offsetMultipler;
        }

        private void OnDrawGizmosSelected()
        {
            if (sections.Count == 0)
                return;

            if (gizmosType == GBArea2DGizmosType.Selected)
            {
                DrawSectionsGizmos();
            }

        }
        private void OnDrawGizmos()
        {
            if (gizmosType == GBArea2DGizmosType.Always)
            {
                DrawSectionsGizmos();
            }
        }

        private void DrawSectionsGizmos()
        {
#if UNITY_EDITOR
            for (int i = 0; i < sections.Count; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawSphere(sections[i].point, 0.1f);
                Gizmos.DrawSphere(sections[i].limitUp, 0.1f);
                Gizmos.DrawSphere(sections[i].limitDown, 0.1f);
                Gizmos.color = Color.black;
                UnityEditor.Handles.color = Color.black;

                if (i + 1 < sections.Count)
                {
                    // var mid = sections[i + 1].point.x - sections[i].point.x / 2;

                    var pointsA = new List<Vector3>();
                    var pointsB = new List<Vector3>();
                    var pointsC = new List<Vector3>();
                    var pointsD = new List<Vector3>();

                    pointsA.Add(new Vector3(sections[i].limitDown.x, sections[i].limitDown.y));
                    pointsA.Add(new Vector3(sections[i].point.x, sections[i].point.y));
                    pointsA.Add(new Vector3(sections[i + 1].limitDown.x, sections[i + 1].limitDown.y));

                    pointsB.Add(new Vector3(sections[i + 1].point.x, sections[i + 1].point.y));
                    pointsB.Add(new Vector3(sections[i].point.x, sections[i].point.y));
                    pointsB.Add(new Vector3(sections[i].limitUp.x, sections[i].limitUp.y));

                    pointsC.Add(new Vector3(sections[i].point.x, sections[i].point.y));
                    pointsC.Add(new Vector3(sections[i + 1].point.x, sections[i + 1].point.y));
                    pointsC.Add(new Vector3(sections[i + 1].limitDown.x, sections[i + 1].limitDown.y));

                    pointsD.Add(new Vector3(sections[i + 1].point.x, sections[i + 1].point.y));
                    pointsD.Add(new Vector3(sections[i + 1].limitUp.x, sections[i + 1].limitUp.y));
                    pointsD.Add(new Vector3(sections[i].limitUp.x, sections[i].limitUp.y));


                    var color = gizmosColor;
                    color.a = 0.35f;
                    UnityEditor.Handles.color = color;

                    // UnityEditor.Handles.DrawAAPolyLine(points.ToArray());
                    UnityEditor.Handles.DrawAAConvexPolygon(pointsA.ToArray());
                    UnityEditor.Handles.DrawAAConvexPolygon(pointsB.ToArray());
                    UnityEditor.Handles.DrawAAConvexPolygon(pointsC.ToArray());
                    UnityEditor.Handles.DrawAAConvexPolygon(pointsD.ToArray());

                    Vector3 center = Vector3.Lerp(sections[i].point, sections[i + 1].point, 0.5f);
                  //  var midPoint = Vector3.Lerp(sections[i].limitDown, sections[i + 1].limitUp, 0.5f);
                   // var midPoint = Vector3.Lerp(sections[i].point, sections[i + 1].point, 0.5f);
                    var tittleStyle = GetStyle(Color.black, TextAnchor.MiddleCenter, 10, FontStyle.Normal);

                    UnityEditor.Handles.Label(center + new Vector3(-0.5f,0,0), "Section : [" + i.ToString() + " - " + (i + 1) + "]", tittleStyle);
                }



                //Draw Sections Lines
                //  Gizmos.color = Color.gray;
                Gizmos.DrawLine(sections[i].point, sections[i].limitDown);
                Gizmos.DrawLine(sections[i].point, sections[i].limitUp);
            }
        }
        public GUIStyle GetStyle(Color color, TextAnchor align = TextAnchor.MiddleCenter, int size = 11, FontStyle st = FontStyle.Normal)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.normal.textColor = color;
            style.alignment = align;
            style.fontSize = size;
            style.fontStyle = st;
            style.richText = true;
            return style;
        }
#endif

    }


}
