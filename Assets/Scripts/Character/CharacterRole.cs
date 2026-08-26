using UnityEngine;

public class CharacterRole : MonoBehaviour
{
    [SerializeField] private TeamSide teamSide = TeamSide.Player;
    [SerializeField] private POSITION position = POSITION.OutsideHitter;

    public TeamSide TeamSide => teamSide;
    public POSITION Position => position;
}
