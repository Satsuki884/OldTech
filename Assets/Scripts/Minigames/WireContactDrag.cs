using UnityEngine;
using UnityEngine.EventSystems;

public class WireContactDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public WireDragBoard board;
    [Range(0, 7)] public int index;
    public void OnBeginDrag(PointerEventData data) { if (board) board.BeginWire(index, data); }
    public void OnDrag(PointerEventData data) { if (board) board.MoveWire(data); }
    public void OnEndDrag(PointerEventData data) { if (board) board.EndWire(data); }
}
