using GunSprint.Data;
using UnityEngine;

namespace GunSprint.Events
{
    /// <summary>
    /// Event channel carrying a gun, e.g. the gun the shop is currently showing.
    /// </summary>
    [CreateAssetMenu(fileName = "GunEventChannel", menuName = "GunSprint/Events/Gun Event Channel")]
    public class GunEventChannelSO : EventChannelSO<GunDefinitionSO>
    {
    }
}
