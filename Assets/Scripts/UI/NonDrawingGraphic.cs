using UnityEngine;
using UnityEngine.UI;

namespace GunSprint.UI
{
    /// <summary>
    /// Invisible raycast target. Unlike a transparent Image it generates no mesh,
    /// so it adds no draw call and no overdraw.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public class NonDrawingGraphic : Graphic
    {
        public override void SetMaterialDirty()
        {
        }

        public override void SetVerticesDirty()
        {
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
