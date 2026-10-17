using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Draws cables in Canvas space. Contact positions remain editable in the prefab.
public class WireDragBoard : MaskableGraphic
{
    [Tooltip("Four left contact RectTransforms, then four right contacts.")]
    public RectTransform[] contacts;
    [Min(1)] public float thickness = 7;
    [Range(8, 64)] public int segments = 28;
    MinigameController controller;
    MinigameView view;
    WireMinigame wires;
    int source = -1, pointer;
    Vector2 cursor;
    readonly List<RaycastResult> hits = new List<RaycastResult>();
    public void Bind(MinigameController owner, MinigameView presentation)
    {
        controller = owner; view = presentation; wires = owner.Current as WireMinigame;
        raycastTarget = false; source = -1; SetVerticesDirty();
    }
    public void BeginWire(int index, PointerEventData data)
    {
        if (source >= 0 || data.button != PointerEventData.InputButton.Left || !wires || controller.Current != wires ||
            index < 0 || index >= 8 || contacts == null || contacts.Length != 8 || !contacts[index] || wires.Connected(index)) return;
        source = index; pointer = data.pointerId;
        cursor = Endpoint(index);
        MoveWire(data);
    }
    public void MoveWire(PointerEventData data)
    {
        if (source < 0 || data.pointerId != pointer) return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out var local))
            cursor = local;
        SetVerticesDirty();
    }
    public void EndWire(PointerEventData data)
    {
        if (source < 0 || data.pointerId != pointer) return;
        int start = source, destination = -1;
        source = -1;
        hits.Clear();
        if (EventSystem.current)
        {
            EventSystem.current.RaycastAll(data, hits);
            // Only the topmost hit may receive the drop; covered contacts are not valid targets.
            if (hits.Count > 0)
            {
                var contact = hits[0].gameObject.GetComponentInParent<WireContactDrag>();
                if (contact && contact.board == this && contact.isActiveAndEnabled) destination = contact.index;
            }
        }
        if (controller && controller.Current == wires)
        {
            controller.ConnectWires(start, destination);
            if (controller.Current == wires) view.Refresh();
        }
        SetVerticesDirty();
    }
    void LateUpdate() { if (wires) SetVerticesDirty(); }
    void OnApplicationFocus(bool focus) { if (!focus) { source = -1; SetVerticesDirty(); } }
    protected override void OnDisable() { source = -1; base.OnDisable(); }
    Vector2 Endpoint(int index)
    {
        var contact = contacts[index]; var rect = contact.rect;
        var local = new Vector3(index < 4 ? rect.xMax : rect.xMin, rect.center.y, 0);
        return rectTransform.InverseTransformPoint(contact.TransformPoint(local));
    }
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (!wires || !view || contacts == null || contacts.Length != 8) return;
        for (int i = 0; i < 4; i++)
        {
            if (!contacts[i] || !wires.Connected(i)) continue;
            for (int j = 4; j < 8; j++)
                if (contacts[j] && wires.WireAt(i) == wires.WireAt(j))
                { Cable(mesh, Endpoint(i), Endpoint(j), view.WireColor(wires.WireAt(i)), true); break; }
        }
        if (source >= 0 && contacts[source]) Cable(mesh, Endpoint(source), cursor, view.WireColor(wires.WireAt(source)), source < 4);
    }
    void Cable(VertexHelper mesh, Vector2 start, Vector2 end, Color tint, bool leftToRight)
    {
        float reach = Mathf.Max(60, Mathf.Abs(end.x - start.x) * .45f) * (leftToRight ? 1 : -1);
        Vector2 a = start + new Vector2(reach, 0), b = end - new Vector2(reach, 0), previous = start;
        int count = Mathf.Clamp(segments, 8, 64);
        for (int i = 1; i <= count; i++)
        {
            float t = (float)i / count, u = 1 - t;
            Vector2 next = u * u * u * start + 3 * u * u * t * a + 3 * u * t * t * b + t * t * t * end;
            Vector2 delta = next - previous;
            if (delta.sqrMagnitude > .001f)
            {
                Vector2 side = new Vector2(-delta.y, delta.x).normalized * Mathf.Max(1, thickness) * .5f;
                int first = mesh.currentVertCount;
                mesh.AddVert(previous - side, tint, Vector2.zero); mesh.AddVert(previous + side, tint, Vector2.zero);
                mesh.AddVert(next + side, tint, Vector2.zero); mesh.AddVert(next - side, tint, Vector2.zero);
                mesh.AddTriangle(first, first + 1, first + 2); mesh.AddTriangle(first, first + 2, first + 3);
            }
            previous = next;
        }
    }
}
