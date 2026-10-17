using UnityEngine;
using UnityEngine.UI;

// Procedural placeholder artwork; artists can replace it with an Image on the same rotor.
public class TapeReelGraphic : MaskableGraphic
{
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Vector2 center = rectTransform.rect.center;
        float radius = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * .48f;
        Ring(mesh, center, radius * .79f, radius);
        Ring(mesh, center, radius * .12f, radius * .30f);
        for (int i = 0; i < 3; i++)
        {
            float angle = i * Mathf.PI * 2 / 3;
            Vector2 axis = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 side = new Vector2(-axis.y, axis.x) * radius * .065f;
            Quad(mesh, center + axis * radius * .27f - side, center + axis * radius * .82f - side,
                center + axis * radius * .82f + side, center + axis * radius * .27f + side);
        }
    }
    void Ring(VertexHelper mesh, Vector2 center, float inner, float outer)
    {
        const int segments = 64;
        for (int i = 0; i < segments; i++)
        {
            float a = i * Mathf.PI * 2 / segments, b = (i + 1) * Mathf.PI * 2 / segments;
            Vector2 u = new Vector2(Mathf.Cos(a), Mathf.Sin(a)), v = new Vector2(Mathf.Cos(b), Mathf.Sin(b));
            Quad(mesh, center + u * inner, center + u * outer, center + v * outer, center + v * inner);
        }
    }
    void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        int start = mesh.currentVertCount;
        mesh.AddVert(a, color, Vector2.zero); mesh.AddVert(b, color, Vector2.zero);
        mesh.AddVert(c, color, Vector2.zero); mesh.AddVert(d, color, Vector2.zero);
        mesh.AddTriangle(start, start + 1, start + 2); mesh.AddTriangle(start, start + 2, start + 3);
    }
}
