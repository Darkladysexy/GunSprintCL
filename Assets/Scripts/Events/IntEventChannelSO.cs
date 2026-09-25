using UnityEngine;

namespace GunSprint.Events
{
    /// <summary>
    /// Event channel carrying an int, e.g. coins rewarded for a kill or a target multiplier.
    /// </summary>
    [CreateAssetMenu(fileName = "IntEventChannel", menuName = "GunSprint/Events/Int Event Channel")]
    public class IntEventChannelSO : EventChannelSO<int>
    {
    }
}
