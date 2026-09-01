internal sealed class RallyState
{
    public TeamSide CurrentTouchTeam { get; private set; }
    public TeamSide ReceivingTeam { get; private set; } = TeamSide.Opponent;
    public int CurrentTouchCount { get; private set; }
    public TeamSide LastAttackTeam { get; private set; }
    public CharacterRole LastAttackActor { get; private set; }
    public bool HasLastAttackTeam { get; private set; }

    public void ResetForServe(TeamSide servingTeam, TeamSide receivingTeam)
    {
        CurrentTouchTeam = servingTeam;
        ReceivingTeam = receivingTeam;
        CurrentTouchCount = 0;
        ClearLastAttack();
    }

    public void RecordServe(TeamSide servingTeam, TeamSide receivingTeam)
    {
        CurrentTouchTeam = servingTeam;
        ReceivingTeam = receivingTeam;
        CurrentTouchCount = 1;
    }

    public void RecordTouch(TeamSide team)
    {
        if (team != CurrentTouchTeam)
        {
            CurrentTouchTeam = team;
            CurrentTouchCount = 0;
        }

        CurrentTouchCount++;
    }

    public void RecordAttack(TeamSide team, CharacterRole actor)
    {
        LastAttackTeam = team;
        LastAttackActor = actor;
        HasLastAttackTeam = true;
    }

    public void SetReceivingTeam(TeamSide team)
    {
        ReceivingTeam = team;
    }

    private void ClearLastAttack()
    {
        LastAttackActor = null;
        HasLastAttackTeam = false;
    }
}
