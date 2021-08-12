using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
#endif

namespace Toguchi.Rendering
{
    public class TexDraw : VolumeComponent
    {
        
    }
    
#if UNITY_EDITOR
    [VolumeComponentEditor(typeof(TexDraw))]
    public class TexDrawEditor : VolumeComponentEditor
    {
        private string _outputName = "output";
        
        public override void OnInspectorGUI()
        {
            _outputName = EditorGUILayout.TextField(_outputName);
            TexDrawPass.outputName = _outputName;
            
            if (GUILayout.Button("Record"))
            {
                TexDrawPass.isRecord = true;
            }
        }
    }    
#endif
}