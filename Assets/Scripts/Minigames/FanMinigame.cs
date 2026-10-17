public class FanMinigame : MinigameBase
{
    readonly bool[] squashed = new bool[8];
    public override BreakdownType Type => BreakdownType.Fan;
    public override int Target => 8;
    public override void Begin() { base.Begin(); System.Array.Clear(squashed, 0, squashed.Length); }
    public override bool Act(int index)
    {
        if (index < 0 || index >= Target || squashed[index]) return false;
        squashed[index] = true; Completed++; return true;
    }
}
