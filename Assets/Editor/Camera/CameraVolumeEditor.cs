using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraVolume))]
public class CameraVolumeEditor : Editor
{
    private static readonly string[] CornerNames =
    {
        "bottomLeft",
        "bottomRight",
        "topRight",
        "topLeft"
    };

    private SerializedProperty regionsProp;

    private void OnEnable()
    {
        regionsProp = serializedObject.FindProperty("regions");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(regionsProp, true);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Region"))
        {
            int index = regionsProp.arraySize;
            regionsProp.InsertArrayElementAtIndex(index);
            WriteRegion(regionsProp.GetArrayElementAtIndex(index), CameraRegion.CreateDefaultSquare());
        }

        EditorGUI.BeginDisabledGroup(regionsProp.arraySize == 0);
        if (GUILayout.Button("Remove Last"))
            regionsProp.arraySize--;
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        if (regionsProp == null)
            return;

        var volume = (CameraVolume)target;
        Transform volumeTransform = volume.transform;

        serializedObject.Update();

        Handles.color = new Color(0.2f, 0.85f, 1f, 0.9f);

        for (int i = 0; i < regionsProp.arraySize; i++)
            DrawRegionHandles(volumeTransform, regionsProp.GetArrayElementAtIndex(i));

        serializedObject.ApplyModifiedProperties();
    }

    private static void DrawRegionHandles(Transform volumeTransform, SerializedProperty regionProp)
    {
        Vector3[] worldCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            Vector2 local = regionProp.FindPropertyRelative(CornerNames[i]).vector2Value;
            worldCorners[i] = volumeTransform.TransformPoint(local);
        }

        for (int i = 0; i < 4; i++)
            Handles.DrawLine(worldCorners[i], worldCorners[(i + 1) % 4]);

        for (int i = 0; i < 4; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newWorld = Handles.PositionHandle(worldCorners[i], Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Vector3 local = volumeTransform.InverseTransformPoint(newWorld);
                regionProp.FindPropertyRelative(CornerNames[i]).vector2Value = new Vector2(local.x, local.y);
                worldCorners[i] = newWorld;
            }
        }

        for (int i = 0; i < 4; i++)
            DrawEdgeHandle(volumeTransform, regionProp, CornerNames[i], CornerNames[(i + 1) % 4]);

        Vector3 center = (worldCorners[0] + worldCorners[1] + worldCorners[2] + worldCorners[3]) * 0.25f;
        EditorGUI.BeginChangeCheck();
        Vector3 newCenter = Handles.PositionHandle(center, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Vector3 delta = newCenter - center;
            for (int i = 0; i < 4; i++)
            {
                SerializedProperty cornerProp = regionProp.FindPropertyRelative(CornerNames[i]);
                Vector3 world = volumeTransform.TransformPoint(cornerProp.vector2Value) + delta;
                Vector3 local = volumeTransform.InverseTransformPoint(world);
                cornerProp.vector2Value = new Vector2(local.x, local.y);
            }
        }
    }

    private static void DrawEdgeHandle(
        Transform volumeTransform,
        SerializedProperty regionProp,
        string cornerAName,
        string cornerBName)
    {
        SerializedProperty cornerAProp = regionProp.FindPropertyRelative(cornerAName);
        SerializedProperty cornerBProp = regionProp.FindPropertyRelative(cornerBName);

        Vector3 worldA = volumeTransform.TransformPoint(cornerAProp.vector2Value);
        Vector3 worldB = volumeTransform.TransformPoint(cornerBProp.vector2Value);
        Vector3 midpoint = (worldA + worldB) * 0.5f;

        Vector3 edge = worldB - worldA;
        if (edge.sqrMagnitude < 0.0001f)
            return;

        Vector3 perpendicular = new Vector3(-edge.y, edge.x, 0f).normalized;

        float handleSize = HandleUtility.GetHandleSize(midpoint) * 0.12f;

        EditorGUI.BeginChangeCheck();
        Vector3 newMidpoint = Handles.FreeMoveHandle(
            midpoint,
            handleSize,
            Vector3.zero,
            Handles.RectangleHandleCap
        );

        if (!EditorGUI.EndChangeCheck())
            return;

        Vector3 delta = Vector3.Project(newMidpoint - midpoint, perpendicular);

        Vector3 newWorldA = worldA + delta;
        Vector3 newWorldB = worldB + delta;

        Vector3 localA = volumeTransform.InverseTransformPoint(newWorldA);
        Vector3 localB = volumeTransform.InverseTransformPoint(newWorldB);

        cornerAProp.vector2Value = new Vector2(localA.x, localA.y);
        cornerBProp.vector2Value = new Vector2(localB.x, localB.y);
    }

    private static void WriteRegion(SerializedProperty regionProp, CameraRegion region)
    {
        regionProp.FindPropertyRelative("bottomLeft").vector2Value = region.bottomLeft;
        regionProp.FindPropertyRelative("bottomRight").vector2Value = region.bottomRight;
        regionProp.FindPropertyRelative("topRight").vector2Value = region.topRight;
        regionProp.FindPropertyRelative("topLeft").vector2Value = region.topLeft;
    }
}
