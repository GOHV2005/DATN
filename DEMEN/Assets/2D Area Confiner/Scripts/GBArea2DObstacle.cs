using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GBTools.Area2DConfiner
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class GBArea2DObstacle : MonoBehaviour
    {
        public BoxCollider2D coll;


#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (coll == null)
            {
                coll = GetComponent<BoxCollider2D>();
            }
            if (coll.isTrigger == false)
            {
                coll.isTrigger = true;
            }
        }
#endif
    }
}
