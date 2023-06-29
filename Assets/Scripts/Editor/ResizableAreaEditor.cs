using Game;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(ResizableArea))]
    public class ResizableAreaEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            ResizableArea script = (ResizableArea)target;
            if(GUILayout.Button("Update"))
            {
                script.OnEditorUpdate();
            }
        }
    }
}