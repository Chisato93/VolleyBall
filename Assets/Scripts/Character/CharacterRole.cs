using UnityEngine;

public class CharacterRole : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private TeamSide teamSide = TeamSide.Player;
    [SerializeField] private POSITION position = POSITION.OutsideHitter;
    [SerializeField] private PlayerStats fallbackStats = new PlayerStats();

    private PlayerStats defaultStats;

    public PlayerData PlayerData => playerData;
    public TeamSide TeamSide => playerData != null ? playerData.TeamSide : teamSide;
    public POSITION Position => playerData != null ? playerData.Position : position;
    public PlayerStats Stats => playerData != null ? playerData.Stats : fallbackStats;

    private void Awake()
    {
        if (playerData != null && playerData.TeamSide == global::TeamSide.Player && playerData.Stats != null)
        {
            defaultStats = playerData.Stats.Clone();
            PlayerSaveManager.LoadPlayerStats(playerData);
        }
    }

    public void ResetSavedStatsToDefault()
    {
        if (playerData == null || playerData.TeamSide != global::TeamSide.Player || defaultStats == null)
        {
            return;
        }

        PlayerSaveManager.ResetPlayerStats(playerData);
        playerData.Stats.CopyFrom(defaultStats);
    }
}
