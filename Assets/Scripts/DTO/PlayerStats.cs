[System.Serializable]
public class PlayerStats
{
    public const int MaxValue = 299;

    public int Power;
    public int Jump;
    public int Receive;
    public int Set;
    public int Block;
    public int Serve;
    public int Speed;
    public int Stamina;
    public int Mental;

    public PlayerStats Clone()
    {
        PlayerStats clone = new PlayerStats();
        clone.CopyFrom(this);
        return clone;
    }

    public void CopyFrom(PlayerStats other)
    {
        if (other == null)
        {
            return;
        }

        Power = other.Power;
        Jump = other.Jump;
        Receive = other.Receive;
        Set = other.Set;
        Block = other.Block;
        Serve = other.Serve;
        Speed = other.Speed;
        Stamina = other.Stamina;
        Mental = other.Mental;
    }

    public int GetValue(PlayerStatType statType)
    {
        switch (statType)
        {
            case PlayerStatType.Power:
                return Power;
            case PlayerStatType.Jump:
                return Jump;
            case PlayerStatType.Receive:
                return Receive;
            case PlayerStatType.Set:
                return Set;
            case PlayerStatType.Block:
                return Block;
            case PlayerStatType.Serve:
                return Serve;
            case PlayerStatType.Speed:
                return Speed;
            case PlayerStatType.Stamina:
                return Stamina;
            case PlayerStatType.Mental:
                return Mental;
            default:
                return 0;
        }
    }

    public void AddValue(PlayerStatType statType, int amount, int maxValue)
    {
        int nextValue = ClampStat(GetValue(statType) + amount, maxValue);
        SetValue(statType, nextValue);
    }

    public void SetValue(PlayerStatType statType, int value)
    {
        switch (statType)
        {
            case PlayerStatType.Power:
                Power = value;
                break;
            case PlayerStatType.Jump:
                Jump = value;
                break;
            case PlayerStatType.Receive:
                Receive = value;
                break;
            case PlayerStatType.Set:
                Set = value;
                break;
            case PlayerStatType.Block:
                Block = value;
                break;
            case PlayerStatType.Serve:
                Serve = value;
                break;
            case PlayerStatType.Speed:
                Speed = value;
                break;
            case PlayerStatType.Stamina:
                Stamina = value;
                break;
            case PlayerStatType.Mental:
                Mental = value;
                break;
        }
    }

    private static int ClampStat(int value, int maxValue)
    {
        if (value < 0)
        {
            return 0;
        }

        if (value > maxValue)
        {
            return maxValue;
        }

        return value;
    }
}

public enum PlayerStatType
{
    Power,
    Jump,
    Receive,
    Set,
    Block,
    Serve,
    Speed,
    Stamina,
    Mental
}
