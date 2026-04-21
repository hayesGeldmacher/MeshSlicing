using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(MeshSliceScaffolding))]
public class MeshSlicerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MeshSliceScaffolding meshSlicer = target as MeshSliceScaffolding;

        if(GUILayout.Button("Slice Mesh"))
        {
            Undo.RecordObject(meshSlicer, "Slice");
            meshSlicer.StartSlice();
            EditorUtility.SetDirty(meshSlicer);
        }

        if(GUILayout.Button("Burst Mesh"))
        {
            Undo.RecordObject(meshSlicer, "Burst");
            meshSlicer.StartBurst();
            EditorUtility.SetDirty(meshSlicer);
        }
    }
}
