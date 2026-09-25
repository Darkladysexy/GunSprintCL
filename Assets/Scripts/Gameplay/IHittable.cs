using UnityEngine;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Anything a bullet can hit (enemies, multiplier targets...). The bullet only knows this
    /// interface, so new hittable objects can be added without touching bullet code.
    /// </summary>
    public interface IHittable
    {
        void TakeHit(Vector3 point, Vector3 direction);
    }
}
