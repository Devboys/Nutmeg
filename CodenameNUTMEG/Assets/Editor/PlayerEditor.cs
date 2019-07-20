using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterController))]
public class PlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CharacterController charcontroller = (CharacterController)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Reset Player"))
        {
            charcontroller.ResetPlayer();
        }
    }
}
