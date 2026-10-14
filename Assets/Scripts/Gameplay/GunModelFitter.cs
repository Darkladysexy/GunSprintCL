using UnityEngine;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Places third-party gun models (different scales, pivots and part names) in a common frame:
    /// barrel along +Z, a fixed length, centred on a given point. Used by the in-game gun and the
    /// menu showcase so every model in the shop fits the same colliders and camera framing.
    /// </summary>
    public static class GunModelFitter
    {
        private static readonly Vector3[] s_corners = new Vector3[8];

        /// <summary>
        /// Instantiates a visual-only copy of the model under <paramref name="parent"/>: colliders,
        /// rigidbodies, audio and scripts that came with the asset are removed.
        /// </summary>
        public static Transform Spawn(GameObject modelPrefab, Transform parent, Quaternion modelRotation)
        {
            GameObject model = Object.Instantiate(modelPrefab, parent);
            model.name = modelPrefab.name;
            Transform t = model.transform;
            t.localPosition = Vector3.zero;
            t.localRotation = modelRotation;
            t.localScale = Vector3.one;

            foreach (Collider c in model.GetComponentsInChildren<Collider>(true))
            {
                Object.Destroy(c);
            }

            foreach (Rigidbody body in model.GetComponentsInChildren<Rigidbody>(true))
            {
                Object.Destroy(body);
            }

            foreach (MonoBehaviour behaviour in model.GetComponentsInChildren<MonoBehaviour>(true))
            {
                Object.Destroy(behaviour);
            }

            foreach (AudioSource source in model.GetComponentsInChildren<AudioSource>(true))
            {
                Object.Destroy(source);
            }

            return t;
        }

        /// <summary>
        /// Uniformly scales and moves <paramref name="model"/> so its bounds (in <paramref name="space"/>)
        /// are <paramref name="length"/> long along Z and centred on <paramref name="center"/>.
        /// </summary>
        public static void Fit(Transform model, Transform space, float length, Vector3 center)
        {
            if (!TryGetBounds(model, space, out Bounds bounds) || bounds.size.z <= 0f)
            {
                return;
            }

            float scale = length / bounds.size.z;
            model.localScale *= scale;
            if (TryGetBounds(model, space, out bounds))
            {
                Vector3 offset = center - bounds.center;
                model.position += space.TransformVector(offset);
            }
        }

        /// <summary>
        /// Bounds of every mesh under <paramref name="root"/>, expressed in <paramref name="space"/>.
        /// Uses mesh bounds, so it works on frame 0 and on inactive objects.
        /// </summary>
        public static bool TryGetBounds(Transform root, Transform space, out Bounds bounds)
        {
            bounds = default;
            bool hasBounds = false;
            Matrix4x4 toSpace = space.worldToLocalMatrix;

            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh != null)
                {
                    Encapsulate(ref bounds, ref hasBounds, filter.sharedMesh.bounds, toSpace * filter.transform.localToWorldMatrix);
                }
            }

            foreach (SkinnedMeshRenderer skinned in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (skinned.sharedMesh != null)
                {
                    Encapsulate(ref bounds, ref hasBounds, skinned.sharedMesh.bounds, toSpace * skinned.transform.localToWorldMatrix);
                }
            }

            return hasBounds;
        }

        /// <summary>
        /// Best guess of the muzzle in <paramref name="space"/>: the front of the part named "Barrel"
        /// when the model has one, otherwise the front of the upper part of the whole model.
        /// </summary>
        public static Vector3 FindMuzzle(Transform model, Transform space)
        {
            Transform barrel = FindPart(model, "Barrel");
            if (barrel != null && TryGetBounds(barrel, space, out Bounds barrelBounds))
            {
                return new Vector3(barrelBounds.center.x, barrelBounds.center.y, barrelBounds.max.z);
            }

            TryGetBounds(model, space, out Bounds bounds);
            return new Vector3(bounds.center.x, bounds.center.y + bounds.extents.y * 0.45f, bounds.max.z);
        }

        /// <summary>First child whose name contains any of the keywords (case-insensitive).</summary>
        public static Transform FindPart(Transform root, params string[] keywords)
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                string name = child.name.ToLowerInvariant();
                foreach (string keyword in keywords)
                {
                    if (child != root && name.Contains(keyword.ToLowerInvariant()))
                    {
                        return child;
                    }
                }
            }

            return null;
        }

        private static void Encapsulate(ref Bounds bounds, ref bool hasBounds, Bounds local, Matrix4x4 matrix)
        {
            Vector3 min = local.min;
            Vector3 max = local.max;
            s_corners[0] = new Vector3(min.x, min.y, min.z);
            s_corners[1] = new Vector3(min.x, min.y, max.z);
            s_corners[2] = new Vector3(min.x, max.y, min.z);
            s_corners[3] = new Vector3(min.x, max.y, max.z);
            s_corners[4] = new Vector3(max.x, min.y, min.z);
            s_corners[5] = new Vector3(max.x, min.y, max.z);
            s_corners[6] = new Vector3(max.x, max.y, min.z);
            s_corners[7] = new Vector3(max.x, max.y, max.z);

            for (int i = 0; i < s_corners.Length; i++)
            {
                Vector3 point = matrix.MultiplyPoint3x4(s_corners[i]);
                if (hasBounds)
                {
                    bounds.Encapsulate(point);
                }
                else
                {
                    bounds = new Bounds(point, Vector3.zero);
                    hasBounds = true;
                }
            }
        }
    }
}
