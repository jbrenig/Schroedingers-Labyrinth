using Game;
using PotentialFunctions;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PotentialManager))]
    public class PotentialManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            PotentialManager script = (PotentialManager)target;
            if(GUILayout.Button("Export Potential Texture"))
            {
                script.BtnExportTexture();
            }
        }
    }
}