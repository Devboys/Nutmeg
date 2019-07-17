using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SmoothCam))]
public class CameraEditor : Editor {

    public override void OnInspectorGUI()
    {

        SmoothCam camController = (SmoothCam)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Center On Target"))
        {
            camController.RecenterOnTarget();
        }
    }
}
