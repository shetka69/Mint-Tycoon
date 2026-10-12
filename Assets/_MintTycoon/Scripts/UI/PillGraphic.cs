using UnityEngine;
using UnityEngine.UI;

namespace MintTycoon.UI
{
    /// <summary>A tiny rounded HUD surface drawn without a texture asset.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PillGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();
            Rect rect = rectTransform.rect;
            float radius = Mathf.Min(rect.width, rect.height) * 0.5f;
            Vector2[] centers = {
                new Vector2(rect.xMax - radius, rect.yMax - radius),
                new Vector2(rect.xMin + radius, rect.yMax - radius),
                new Vector2(rect.xMin + radius, rect.yMin + radius),
                new Vector2(rect.xMax - radius, rect.yMin + radius)
            };
            helper.AddVert(rect.center, color, Vector2.zero);
            const int arcSegments = 8;
            for (int corner = 0; corner < 4; corner++)
                for (int segment = 0; segment <= arcSegments; segment++)
                {
                    float angle = (corner * 90f + segment * 90f / arcSegments) * Mathf.Deg2Rad;
                    Vector2 position = centers[corner] + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    helper.AddVert(position, color, Vector2.zero);
                }
            int count = 4 * (arcSegments + 1);
            for (int index = 1; index <= count; index++) helper.AddTriangle(0, index, index == count ? 1 : index + 1);
        }
    }
}
