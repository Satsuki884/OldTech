public class TapeMinigame : MinigameBase
{
    public override BreakdownType Type => BreakdownType.Tape;
    public override int Target => 18;
    public override bool Act(int index) { if (Solved) return false; Completed++; return true; }
}
