using UnityEngine;
public abstract class MinigameBase : MonoBehaviour
{
    public abstract BreakdownType Type { get; }
    public int Completed { get; protected set; }
    public abstract int Target { get; }
    public bool Solved => Completed >= Target;
    public virtual void Begin() { Completed = 0; }
    public abstract bool Act(int index);
}
