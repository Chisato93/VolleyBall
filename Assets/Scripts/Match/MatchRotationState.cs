using System.Collections.Generic;
using UnityEngine;

internal sealed class MatchRotationState
{
    private readonly TeamRotation playerRotation = new TeamRotation(TeamSide.Player);
    private readonly TeamRotation opponentRotation = new TeamRotation(TeamSide.Opponent);

    public CharacterRole CurrentServer => GetServer(servingTeam);

    private TeamSide servingTeam = TeamSide.Player;

    public void Reset(TeamSide initialServingTeam)
    {
        servingTeam = initialServingTeam;
        playerRotation.Reset();
        opponentRotation.Reset();
    }

    public void ApplyPointResult(TeamSide previousServingTeam, TeamSide nextServingTeam)
    {
        servingTeam = nextServingTeam;

        if (previousServingTeam == nextServingTeam)
        {
            return;
        }

        GetTeamRotation(nextServingTeam).RotateClockwise();
    }

    public void RestoreFormation()
    {
        playerRotation.ApplyFormation();
        opponentRotation.ApplyFormation();
    }

    public bool CanServe(TeamSide team, CharacterRole actor)
    {
        if (team != servingTeam)
        {
            return false;
        }

        CharacterRole server = CurrentServer;
        return server == null || actor == null || actor == server;
    }

    private CharacterRole GetServer(TeamSide team)
    {
        return GetTeamRotation(team).Server;
    }

    private TeamRotation GetTeamRotation(TeamSide team)
    {
        return team == TeamSide.Player ? playerRotation : opponentRotation;
    }

    private sealed class TeamRotation
    {
        private const int SlotCount = 6;

        private readonly TeamSide teamSide;
        private readonly Vector3[] slotPositions = new Vector3[SlotCount];
        private readonly Quaternion[] slotRotations = new Quaternion[SlotCount];
        private readonly CharacterRole[] initialPlayersBySlot = new CharacterRole[SlotCount];
        private readonly CharacterRole[] playersBySlot = new CharacterRole[SlotCount];

        private bool hasFormation;

        public TeamRotation(TeamSide teamSide)
        {
            this.teamSide = teamSide;
        }

        public CharacterRole Server => hasFormation ? playersBySlot[0] : null;

        public void Reset()
        {
            if (!hasFormation)
            {
                CaptureFormation();
            }

            CopyInitialLineup();
            ApplyFormation();
        }

        public void RotateClockwise()
        {
            if (!hasFormation)
            {
                CaptureFormation();
            }

            if (!hasFormation)
            {
                return;
            }

            CharacterRole oldPositionOne = playersBySlot[0];
            playersBySlot[0] = playersBySlot[1];
            playersBySlot[1] = playersBySlot[2];
            playersBySlot[2] = playersBySlot[3];
            playersBySlot[3] = playersBySlot[4];
            playersBySlot[4] = playersBySlot[5];
            playersBySlot[5] = oldPositionOne;

            ApplyFormation();
        }

        private void CaptureFormation()
        {
            List<CharacterRole> roles = FindTeamRoles();
            if (roles.Count != SlotCount)
            {
                Debug.LogWarning($"Rotation skipped. {teamSide} team needs {SlotCount} players, but found {roles.Count}.");
                return;
            }

            List<CharacterRole> frontRow = GetRow(roles, true);
            List<CharacterRole> backRow = GetRow(roles, false);

            AssignInitialSlot(0, GetRight(backRow));
            AssignInitialSlot(1, GetRight(frontRow));
            AssignInitialSlot(2, frontRow[1]);
            AssignInitialSlot(3, GetLeft(frontRow));
            AssignInitialSlot(4, GetLeft(backRow));
            AssignInitialSlot(5, backRow[1]);

            hasFormation = true;
        }

        private List<CharacterRole> FindTeamRoles()
        {
            CharacterRole[] allRoles = Object.FindObjectsByType<CharacterRole>(FindObjectsSortMode.None);
            List<CharacterRole> teamRoles = new List<CharacterRole>();

            foreach (CharacterRole role in allRoles)
            {
                if (role.TeamSide == teamSide)
                {
                    teamRoles.Add(role);
                }
            }

            return teamRoles;
        }

        private List<CharacterRole> GetRow(List<CharacterRole> roles, bool front)
        {
            List<CharacterRole> sorted = new List<CharacterRole>(roles);
            sorted.Sort(CompareByNetDistance);

            List<CharacterRole> row = sorted.GetRange(front ? 0 : 3, 3);
            row.Sort(CompareByX);
            return row;
        }

        private int CompareByNetDistance(CharacterRole a, CharacterRole b)
        {
            float aScore = GetFrontScore(a.transform.position);
            float bScore = GetFrontScore(b.transform.position);
            return bScore.CompareTo(aScore);
        }

        private float GetFrontScore(Vector3 position)
        {
            return teamSide == TeamSide.Player ? position.z : -position.z;
        }

        private static int CompareByX(CharacterRole a, CharacterRole b)
        {
            return a.transform.position.x.CompareTo(b.transform.position.x);
        }

        private CharacterRole GetRight(List<CharacterRole> row)
        {
            return teamSide == TeamSide.Player ? row[2] : row[0];
        }

        private CharacterRole GetLeft(List<CharacterRole> row)
        {
            return teamSide == TeamSide.Player ? row[0] : row[2];
        }

        private void AssignInitialSlot(int slotIndex, CharacterRole role)
        {
            initialPlayersBySlot[slotIndex] = role;
            playersBySlot[slotIndex] = role;
            slotPositions[slotIndex] = role.transform.position;
            slotRotations[slotIndex] = role.transform.rotation;
        }

        private void CopyInitialLineup()
        {
            if (!hasFormation)
            {
                return;
            }

            for (int i = 0; i < SlotCount; i++)
            {
                playersBySlot[i] = initialPlayersBySlot[i];
            }
        }

        public void ApplyFormation()
        {
            if (!hasFormation)
            {
                return;
            }

            for (int i = 0; i < SlotCount; i++)
            {
                CharacterRole role = playersBySlot[i];
                if (role == null)
                {
                    continue;
                }

                role.transform.SetPositionAndRotation(slotPositions[i], slotRotations[i]);
                StopRole(role);
            }
        }

        private static void StopRole(CharacterRole role)
        {
            Rigidbody body = role.GetComponent<Rigidbody>();
            if (body == null)
            {
                return;
            }

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }
}
