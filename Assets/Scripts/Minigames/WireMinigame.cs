public class WireMinigame : MinigameBase
{
    readonly bool[] connected = new bool[4];
    public int Selected { get; private set; } = -1;
    public override BreakdownType Type => BreakdownType.Wire;
    public override int Target => 4;
    public override void Begin() { base.Begin(); Selected = -1; System.Array.Clear(connected, 0, 4); }
    public override bool Act(int index)
    {
        if (index < 0 || index >= 8) return false;
        int wire = index % 4;
        if (connected[wire]) return false;
        if (index < 4) { Selected = wire; return false; }
        if (Selected != wire) { Selected = -1; return false; }
        connected[wire] = true; Selected = -1; Completed++; return true;
    }
    public bool Connected(int wire) => connected[wire];
}
