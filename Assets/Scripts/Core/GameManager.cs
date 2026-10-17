using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameBalanceConfig balance;
    public ProjectManager Project { get; private set; }
    public EfficiencyManager Efficiency { get; private set; }
    public BreakdownManager Breakdowns { get; private set; }
    public MinigameController Minigames { get; private set; }
    public float RemainingSeconds { get; private set; }
    public bool Finished { get; private set; }
    public bool Won { get; private set; }
    void Awake()
    {
        if (!balance) balance = ScriptableObject.CreateInstance<GameBalanceConfig>();
        // Managers are siblings within GameSystems.
        Project = transform.parent.GetComponentInChildren<ProjectManager>();
        Efficiency = transform.parent.GetComponentInChildren<EfficiencyManager>();
        Breakdowns = transform.parent.GetComponentInChildren<BreakdownManager>();
        Minigames = transform.parent.GetComponentInChildren<MinigameController>();
        RemainingSeconds = balance.deadlineSeconds;
    }
    void Update()
    {
        if (Finished) return;
        RemainingSeconds = Mathf.Max(0, RemainingSeconds - Time.deltaTime);
        Breakdowns.Tick(Time.deltaTime, balance);
        Efficiency.Recalculate(Breakdowns.ActiveCount, balance);
        Project.Tick(Time.deltaTime, Efficiency.Value, balance);
        if (Project.Progress >= 1 || RemainingSeconds <= 0)
        {
            Finished = true;
            Won = Project.Progress >= 1;
            Minigames.Close();
        }
    }
    public void Restart()
    {
        Minigames.Close();
        Project.ResetProgress();
        Breakdowns.ResetBreakdowns();
        Efficiency.Recalculate(0, balance);
        RemainingSeconds = balance.deadlineSeconds;
        Finished = Won = false;
    }
}
