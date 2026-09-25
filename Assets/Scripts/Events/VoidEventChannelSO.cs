using System;
using UnityEngine;

namespace GunSprint.Events
{
    /// <summary>
    /// Event channel without a payload, e.g. "fire pressed" or "pause requested".
    /// </summary>
    [CreateAssetMenu(fileName = "VoidEventChannel", menuName = "GunSprint/Events/Void Event Channel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public event Action OnEventRaised;

        public void RaiseEvent()
        {
            OnEventRaised?.Invoke();
        }
    }
}
