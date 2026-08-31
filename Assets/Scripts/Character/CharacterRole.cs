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

    public PlayerData PlayerData => playerData;
    public TeamSide TeamSide => playerData != null ? playerData.TeamSide : teamSide;
    public POSITION Position => usesSelectedPosition ? selectedPosition : GetDefaultPosition();
    public PlayerStats Stats => playerData != null ? playerData.Stats : fallbackStats;

    private void Awake()
    {
        selectedPosition = GetDefaultPosition();
        usesSelectedPosition = playerData != null
            && playerData.TeamSide == global::TeamSide.Player
            && GetComponent<Player>() != null;

        if (usesSelectedPosition && playerData.Stats != null)
        {
            defaultStats = playerData.Stats.Clone();
            PlayerSaveManager.LoadPlayerStats(playerData);
            selectedPosition = PlayerSaveManager.LoadSelectedPosition(playerData.Position);
        }
    }

    public void ResetSavedStatsToDefault(POSITION newPosition)
    {
        if (!usesSelectedPosition || playerData == null || defaultStats == null)
        {
            return;
        }

        PlayerSaveManager.ResetPlayerStats(playerData);
        playerData.Stats.CopyFrom(defaultStats);
        selectedPosition = newPosition;
    }

    private POSITION GetDefaultPosition()
    {
        return playerData != null ? playerData.Position : position;
    }
}
