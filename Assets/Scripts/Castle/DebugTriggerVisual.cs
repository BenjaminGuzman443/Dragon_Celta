using UnityEngine;

namespace DragonCeltas
{
    public class DebugTriggerVisual : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box == null) return;

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Vector3 center = transform.TransformPoint(box.offset);
            Vector3 size = box.size;
            size.Scale(transform.lossyScale);
            Gizmos.DrawCube(center, size);
        }
    }
}
