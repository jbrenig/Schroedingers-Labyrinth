using Game;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(GpuQuantumSimulator))]
    public class GpuSimulatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            GpuQuantumSimulator script = (GpuQuantumSimulator)target;
            if(GUILayout.Button("Update"))
            {
                script.OnEditorUpdate();
            }
        }
    }
}