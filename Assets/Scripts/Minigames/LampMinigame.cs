public class LampMinigame : MinigameBase
{
    readonly bool[] replaced = new bool[6];
    public override BreakdownType Type => BreakdownType.Lamp;
    public override int Target => 6;
    public override void Begin() { base.Begin(); System.Array.Clear(replaced, 0, 6); }
    public override bool Act(int index)
    {
        if (index < 0 || index >= Target || replaced[index]) return false;
        replaced[index] = true; Completed++; return true;
    }
}
