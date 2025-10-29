using UnityEngine;

namespace GBTools.Area2DConfiner
{

    [System.Serializable]
    public class GBArea2DConfinerSection
    {
        public Vector3 point;
        public Vector3 limitUp;
        public Vector3 limitDown;

        //editor vars
        [HideInInspector] public bool showPoint = false;

        public float GetMinX
        {
            get
            {
                if (point.x < limitUp.x && point.x < limitDown.x)
                {
                    return point.x;
                }
                else if (limitUp.x < point.x && limitUp.x < limitDown.x)
                {
                    return limitUp.x;
                }
                else
                {
                    return limitDown.x;
                }
            }
        }
        public float GetMaxX
        {
            get
            {
                if (point.x > limitUp.x && point.x > limitDown.x)
                {
                    return point.x;
                }
                else if (limitUp.x > point.x && limitUp.x > limitDown.x)
                {
                    return limitUp.x;
                }
                else
                {
                    return limitDown.x;
                }
            }
        }
        public float GetMinY
        {
            get
            {
                if (point.y < limitUp.y && point.y < limitDown.y)
                {
                    return point.y;
                }
                else if (limitUp.y < point.y && limitUp.y < limitDown.y)
                {
                    return limitUp.y;
                }
                else
                {
                    return limitDown.y;
                }
            }
        }
        public float GetMaxY
        {
            get
            {
                if (point.y > limitUp.y && point.y > limitDown.y)
                {
                    return point.y;
                }
                else if (limitUp.y > point.y && limitUp.y > limitDown.y)
                {
                    return limitUp.y;
                }
                else
                {
                    return limitDown.y;
                }
            }
        }
    }
}
