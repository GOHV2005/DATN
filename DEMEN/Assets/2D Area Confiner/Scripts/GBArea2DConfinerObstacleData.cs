using UnityEngine;

namespace GBTools.Area2DConfiner
{
    public class GBArea2DConfinerObstacleData
    {
        public bool hitObstacle = false;
        public Vector3 point;

        public GBArea2DConfinerObstacleData(bool hitObstacle, Vector3 point)
        {
            this.hitObstacle = hitObstacle;
            this.point = point;
        }
    }
}
