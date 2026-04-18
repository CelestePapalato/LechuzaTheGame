using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RaycastCollision))]
public class RaycastCollisionEditor : Editor
{
    private static readonly Vector2[] CardinalDirections =
    {
        Vector2.left,
        Vector2.right,
        Vector2.up,
        Vector2.down
    };

    private void OnSceneGUI()
    {
        var rc = (RaycastCollision)target;
        serializedObject.Update();
        SerializedProperty hLenProp = serializedObject.FindProperty("horizontalRayLength");
        SerializedProperty vLenProp = serializedObject.FindProperty("verticalRayLength");
        SerializedProperty offsetProp = serializedObject.FindProperty("rayOriginOffset");
        if (hLenProp == null || vLenProp == null || offsetProp == null)
            return;
        float hLen = hLenProp.floatValue;
        float vLen = vLenProp.floatValue;
        Vector2 off = offsetProp.vector2Value;

        Rigidbody2D rb = rc.GetComponent<Rigidbody2D>();
        Vector2 origin = (rb ? rb.position : (Vector2)rc.transform.position) + off;

        Handles.color = new Color(0.2f, 0.85f, 1f, 0.9f);

        foreach (Vector2 dir in CardinalDirections)
        {
            float len = dir.x != 0f ? hLen : vLen;
            Vector2 end = origin + dir * len;
            Handles.DrawLine(origin, end);
        }
    }
}
