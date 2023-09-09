using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIGuideMask : Graphic, ICanvasRaycastFilter
{
    public RectTransform m_Target;
    public Sprite m_Sprite;

    private Vector3[] selfCorners = new Vector3[4];
    private Vector3[] targetCorners = new Vector3[4];

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (null == m_Target)
        {
            base.OnPopulateMesh(vh);
            return;
        }

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;
        vert.uv0 = new Vector4(0, 0, 1, 0);
        rectTransform.GetLocalCorners(selfCorners);
        m_Target.GetWorldCorners(targetCorners);
        var worldToLocalMatrix = this.rectTransform.worldToLocalMatrix;

        vh.Clear();
        for (int i = 0; i < 4; i++)
        {
            // targetCorners[i] = worldToLocalMatrix.MultiplyPoint(targetCorners[i]);
            var corner = worldToLocalMatrix.MultiplyPoint(targetCorners[i]);
            targetCorners[i] = corner;
            vert.position = corner;
            vh.AddVert(vert);
            vert.position = selfCorners[i];
            vh.AddVert(vert);
        }

        if (targetCorners[0].x > selfCorners[0].x)
        {
            vh.AddTriangle(0, 1, 3);
            vh.AddTriangle(0, 3, 2);
        }

        if (targetCorners[1].y < selfCorners[1].y)
        {
            vh.AddTriangle(2, 3, 5);
            vh.AddTriangle(2, 5, 4);
        }

        if (targetCorners[2].x < selfCorners[2].x)
        {
            vh.AddTriangle(4, 5, 7);
            vh.AddTriangle(4, 7, 6);
        }

        if (targetCorners[3].y > selfCorners[3].y)
        {
            vh.AddTriangle(6, 7, 1);
            vh.AddTriangle(6, 1, 0);
        }

        if (m_Sprite != null)
        {
            Vector4 outer = UnityEngine.Sprites.DataUtility.GetOuterUV(m_Sprite);
            Vector4 inner = UnityEngine.Sprites.DataUtility.GetInnerUV(m_Sprite);
            // Vector4 padding = UnityEngine.Sprites.DataUtility.GetPadding(m_Sprite) / pixelsPerUnit;
            Vector4 border = m_Sprite.border;
            Rect rect = m_Target.rect;
            Vector4 adjustedBorders = GetTargetRectAdjustedBorders(border / pixelsPerUnit, rect);
            Vector4 VertX = new(targetCorners[0].x, targetCorners[0].x + adjustedBorders.x,
                targetCorners[2].x - adjustedBorders.z, targetCorners[2].x);
            Vector4 VertY = new(targetCorners[0].y, targetCorners[0].y + adjustedBorders.y,
                targetCorners[2].y - adjustedBorders.w, targetCorners[2].y);
            Vector4 UVX = new(outer.x, inner.x, inner.z, outer.z);
            Vector4 UVY = new(outer.y, inner.y, inner.w, outer.w);
            for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                vh.AddVert(
                    new Vector3(VertX[x], VertY[y], 0.0f),
                    color,
                    new Vector4(UVX[x], UVY[y], 0.0f, 0.0f)
                );

            for (int x = 8; x <= 10; x++)
            for (int i = x; i <= x + 8; i += 4)
            {
                vh.AddTriangle(i, i + 4, i + 5);
                vh.AddTriangle(i, i + 5, i + 1);
            }
        }
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        Vector3 worldPoint;
        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, eventCamera,
                out worldPoint))
            return false;
        if (m_Target != null)
        {
            Vector3[] targetCorners = new Vector3[4];
            m_Target.GetWorldCorners(targetCorners);
            bool isInTargetRect = worldPoint.x > targetCorners[0].x && worldPoint.y > targetCorners[0].y &&
                                  worldPoint.x < targetCorners[2].x && worldPoint.y < targetCorners[2].y;
            return !isInTargetRect;
        }

        return true;
    }


    float m_CachedReferencePixelsPerUnit = 100;
    public float pixelsPerUnit
    {
        get
        {
            float spritePixelsPerUnit = 100;
            if (m_Sprite)
                spritePixelsPerUnit = m_Sprite.pixelsPerUnit;

            if (canvas)
                m_CachedReferencePixelsPerUnit = canvas.referencePixelsPerUnit;

            return spritePixelsPerUnit / m_CachedReferencePixelsPerUnit;
        }
    }

    private Vector4 GetTargetRectAdjustedBorders(Vector4 border, Rect adjustedRect)
    {
        Rect originalRect = m_Target.rect;

        for (int axis = 0; axis <= 1; axis++)
        {
            float borderScaleRatio;

            // The adjusted rect (adjusted for pixel correctness)
            // may be slightly larger than the original rect.
            // Adjust the border to match the adjustedRect to avoid
            // small gaps between borders (case 833201).
            if (originalRect.size[axis] != 0)
            {
                borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }

            // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
            // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
            float combinedBorders = border[axis] + border[axis + 2];
            if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
            {
                borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }
        }

        return border;
    }
    
    public override Texture mainTexture
    {
        get
        {
            if (m_Sprite == null)
            {
                if (material != null && material.mainTexture != null)
                {
                    return material.mainTexture;
                }
                return s_WhiteTexture;
            }

            return m_Sprite.texture;
        }
    }
}