using Game;
using Ui;
using Ui.Dialog;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(DialogController))]
    public class IntroControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            DialogController script = (DialogController)target;
            if(GUILayout.Button("Next Page"))
            {
                script.BtnContinue();
            }
            if(GUILayout.Button("Previous Page"))
            {
                script.BtnPrevious();
            }
            if(GUILayout.Button("Reload Ui"))
            {
                script.Start();
            }
        }
    }
}