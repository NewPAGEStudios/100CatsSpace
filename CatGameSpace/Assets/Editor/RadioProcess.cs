#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RadioObject))]
public class RadioProcess : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RadioObject obj = (RadioObject)target;

        if (GUILayout.Button("RandomizeClips()"))
        {
            obj.RandomizeClips();
            EditorUtility.SetDirty(obj);
        }
    }

}
#endif