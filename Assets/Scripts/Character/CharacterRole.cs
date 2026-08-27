using UnityEngine;

public class CharacterRole : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField] private TeamSide teamSide = TeamSide.Player;
    [SerializeField] private POSITION position = POSITION.OutsideHitter;
    [SerializeField] private PlayerStats fallbackStats = new PlayerStats();

    public PlayerData PlayerData => playerData;
    public TeamSide TeamSide => playerData != null ? playerData.TeamSide : teamSide;
    public POSITION Position => playerData != null ? playerData.Position : position;
    public PlayerStats Stats => playerData != null ? playerData.Stats : fallbackStats;
}
