using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Volleyball/Player Data")]
public class PlayerData : ScriptableObject
{
    [SerializeField] private string playerName;
    [SerializeField] private int number;
    [SerializeField] private TeamSide teamSide = TeamSide.Player;
    [SerializeField] private POSITION position = POSITION.OutsideHitter;
    [SerializeField] private PlayerStats stats = new PlayerStats();

    public string PlayerName => playerName;
    public int Number => number;
    public TeamSide TeamSide => teamSide;
    public POSITION Position => position;
    public PlayerStats Stats => stats;
}
