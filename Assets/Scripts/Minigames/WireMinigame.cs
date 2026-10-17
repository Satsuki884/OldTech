using UnityEngine;
public class WireMinigame : MinigameBase
{
    readonly bool[] connected = new bool[4];
    readonly int[] left = { 0, 1, 2, 3 };
    readonly int[] right = { 0, 1, 2, 3 };
    public int Selected { get; private set; } = -1;
    public override BreakdownType Type => BreakdownType.Wire;
    public override int Target => 4;
    public int WireAt(int index) => index < 0 || index >= 8 ? -1 : index < 4 ? left[index] : right[index - 4];
    public override void Begin()
    {
        base.Begin(); Selected = -1; System.Array.Clear(connected, 0, 4);
        Shuffle(left); Shuffle(right);
    }
    static void Shuffle(int[] items)
    {
        for (int i = items.Length - 1; i > 0; i--)
        {
            int other = Random.Range(0, i + 1);
            int value = items[i]; items[i] = items[other]; items[other] = value;
        }
    }
    public override bool Act(int index)
    {
        int wire = WireAt(index);
        if (wire < 0 || connected[wire]) return false;
        if (index < 4) { Selected = index; return false; }
        if (Selected < 0 || WireAt(Selected) != wire) { Selected = -1; return false; }
        connected[wire] = true; Selected = -1; Completed++; return true;
    }
    public bool Connected(int index) { int wire = WireAt(index); return wire >= 0 && connected[wire]; }
}
