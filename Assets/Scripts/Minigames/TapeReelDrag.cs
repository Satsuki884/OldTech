using UnityEngine;
using UnityEngine.EventSystems;

public class TapeReelDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
    IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform rotor;
    [Range(.05f, .8f)] public float deadZone = .22f;
    MinigameController controller;
    MinigameView view;
    TapeMinigame tape;
    int pointerId;
    bool held, previousValid;
    Vector2 previous;
    Quaternion initialRotation;
    public void Bind(MinigameController owner, MinigameView presentation)
    {
        controller = owner; view = presentation; tape = owner.Current as TapeMinigame;
        initialRotation = rotor ? rotor.localRotation : Quaternion.identity;
        UpdateRotor();
    }
    public void OnInitializePotentialDrag(PointerEventData data) { data.useDragThreshold = false; }
    public void OnPointerDown(PointerEventData data)
    {
        if (held || data.button != PointerEventData.InputButton.Left || !tape || controller.Current != tape) return;
        if (!TryDirection(data, out previous)) return;
        pointerId = data.pointerId; held = previousValid = true;
    }
    public void OnBeginDrag(PointerEventData data) { }
    public void OnDrag(PointerEventData data)
    {
        if (!held || data.pointerId != pointerId || !tape || controller.Current != tape) return;
        if (!TryDirection(data, out var next)) { previousValid = false; return; }
        if (previousValid)
        {
            float clockwise = -Vector2.SignedAngle(previous, next);
            // Ignore jumps across the hub or missing pointer samples instead of awarding turns.
            if (Mathf.Abs(clockwise) <= 120 && controller.RotateTape(clockwise) && controller.Current == tape)
            { UpdateRotor(); view.Refresh(); }
        }
        previous = next; previousValid = true;
    }
    bool TryDirection(PointerEventData data, out Vector2 direction)
    {
        direction = Vector2.zero;
        var rect = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, data.position, data.pressEventCamera, out var local)) return false;
        direction = local - rect.rect.center;
        float radius = Mathf.Min(rect.rect.width, rect.rect.height) * .5f;
        float distance = direction.magnitude;
        return radius > 0 && distance >= radius * deadZone && distance <= radius * 1.6f;
    }
    void UpdateRotor()
    {
        if (rotor && tape) rotor.localRotation = initialRotation * Quaternion.Euler(0, 0, -Mathf.Repeat(tape.WoundTurns, 1) * 360);
    }
    public void OnPointerUp(PointerEventData data) { if (held && data.pointerId == pointerId) Cancel(); }
    public void OnEndDrag(PointerEventData data) { if (held && data.pointerId == pointerId) Cancel(); }
    void OnDisable() { Cancel(); }
    void OnApplicationFocus(bool focus) { if (!focus) Cancel(); }
    void Cancel() { held = previousValid = false; }
}
