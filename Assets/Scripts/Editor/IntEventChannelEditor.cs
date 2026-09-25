using GunSprint.Events;
using UnityEditor;
using UnityEngine;

namespace GunSprint.Editor
{
    /// <summary>
    /// Adds a value field and a "Raise Event" button to int event channels for debugging.
    /// </summary>
    [CustomEditor(typeof(IntEventChannelSO))]
    public class IntEventChannelEditor : UnityEditor.Editor
    {
        private int _debugValue = 1;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                _debugValue = EditorGUILayout.IntField("Debug Value", _debugValue);
                if (GUILayout.Button("Raise Event"))
                {
                    ((IntEventChannelSO)target).RaiseEvent(_debugValue);
                }
            }
        }
    }
}
