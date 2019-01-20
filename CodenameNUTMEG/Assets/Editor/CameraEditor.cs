using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SmoothCamNew))]
public class CameraEditor : Editor {

    public override void OnInspectorGUI()
    {

        SmoothCamNew camController = (SmoothCamNew)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Center On Target"))
        {
            camController.RecenterOnTarget();
        }
    }
}
