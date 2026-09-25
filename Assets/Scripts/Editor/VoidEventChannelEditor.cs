using GunSprint.Events;
using UnityEditor;
using UnityEngine;

namespace GunSprint.Editor
{
    /// <summary>
    /// Adds a "Raise Event" button to void event channels, to trigger listeners by hand while debugging.
    /// </summary>
    [CustomEditor(typeof(VoidEventChannelSO))]
    public class VoidEventChannelEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Raise Event"))
                {
                    ((VoidEventChannelSO)target).RaiseEvent();
                }
            }
        }
    }
}
