using UnityEngine;

namespace GunSprint.Utils
{
    /// <summary>
    /// Helpers for the Vefects trail prefabs used by bullets and the flying gun.
    /// </summary>
    public static class TrailEffects
    {
        /// <summary>
        /// Turns off the screen-distortion layer of a trail effect. Distortion shaders sample the
        /// camera opaque texture, which the mobile URP asset disables for performance; without it
        /// they draw as a flat grey smear.
        /// </summary>
        public static void DisableScreenDistortion(GameObject effect)
        {
            foreach (Renderer renderer in effect.GetComponentsInChildren<Renderer>(true))
            {
                Material material = renderer.sharedMaterial;
                if (material != null && material.shader != null && material.shader.name.Contains("Distortion"))
                {
                    renderer.enabled = false;
                }
            }
        }
    }
}
