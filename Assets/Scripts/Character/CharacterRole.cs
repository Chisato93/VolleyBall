using UnityEngine;

public class CharacterRole : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private TeamSide teamSide = TeamSide.Player;
    [SerializeField] private POSITION position = POSITION.OutsideHitter;
    [SerializeField] private PlayerStats fallbackStats = new PlayerStats();

    private PlayerStats defaultStats;
    private POSITION selectedPosition;
    private bool usesSelectedPosition;
    private bool roleInitialized;

    public PlayerData PlayerData => playerData;
    public TeamSide TeamSide => playerData != null ? playerData.TeamSide : teamSide;
    public POSITION Position => usesSelectedPosition ? selectedPosition : GetDefaultPosition();
    private PlayerStats effectiveStats;
    private int effectiveStatsVersion = -1;
    public bool IsCareerPlayer
    {
        get { EnsureRoleInitialized(); return usesSelectedPosition; }
    }

    public PlayerStats Stats
    {
        get
        {
            EnsureRoleInitialized();
            if (!usesSelectedPosition) return playerData != null ? playerData.Stats : fallbackStats;
            if (effectiveStats == null || effectiveStatsVersion != PlayerSaveManager.StateVersion)
            {
                PlayerStats baseStats = defaultStats != null ? defaultStats.Clone() : playerData.Stats.Clone();
                PlayerSaveManager.LoadPlayerStats(playerData.PlayerName, baseStats);
                PlayerStats calculated = EquipmentStatsService.GetEffectiveStats(baseStats);
                if (effectiveStats == null) effectiveStats = calculated;
                else effectiveStats.CopyFrom(calculated);
                effectiveStatsVersion = PlayerSaveManager.StateVersion;
            }
            return effectiveStats;
        }
    }

    private void Awake() => EnsureRoleInitialized();

    private void EnsureRoleInitialized()
    {
        if (roleInitialized) return;
        roleInitialized = true;
        selectedPosition = GetDefaultPosition();
        usesSelectedPosition = playerData != null
            && playerData.TeamSide == global::TeamSide.Player
            && GetComponent<Player>() != null;

        if (usesSelectedPosition && playerData.Stats != null)
        {
            defaultStats = playerData.Stats.Clone();
            selectedPosition = PlayerSaveManager.LoadSelectedPosition(playerData.Position);
        }
    }

    public void ResetSavedStatsToDefault(POSITION newPosition)
    {
        EnsureRoleInitialized();
        if (!usesSelectedPosition || playerData == null || defaultStats == null)
        {
            return;
        }

        PlayerSaveManager.ResetPlayerStats(playerData);
        playerData.Stats.CopyFrom(defaultStats);
        selectedPosition = newPosition;
    }

    public void ResetSavedStatsToNewCareer(POSITION newPosition, PlayerStats startingStats)
    {
        EnsureRoleInitialized();
        if (!usesSelectedPosition || playerData == null || playerData.Stats == null || startingStats == null)
        {
            return;
        }

        PlayerSaveManager.ResetPlayerStats(playerData);
        playerData.Stats.CopyFrom(startingStats);
        PlayerSaveManager.SavePlayerStats(playerData);
        selectedPosition = newPosition;
    }

    private POSITION GetDefaultPosition()
    {
        return playerData != null ? playerData.Position : position;
    }
}
