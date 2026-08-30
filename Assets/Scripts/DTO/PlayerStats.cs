[System.Serializable]
public class PlayerStats
{
    public int Power;
    public int Jump;
    public int Receive;
    public int Set;
    public int Block;
    public int Serve;
    public int Speed;
    public int Stamina;
    public int Mental;

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

        switch (statType)
        {
            case PlayerStatType.Power:
                Power = nextValue;
                break;
            case PlayerStatType.Jump:
                Jump = nextValue;
                break;
            case PlayerStatType.Receive:
                Receive = nextValue;
                break;
            case PlayerStatType.Set:
                Set = nextValue;
                break;
            case PlayerStatType.Block:
                Block = nextValue;
                break;
            case PlayerStatType.Serve:
                Serve = nextValue;
                break;
            case PlayerStatType.Speed:
                Speed = nextValue;
                break;
            case PlayerStatType.Stamina:
                Stamina = nextValue;
                break;
            case PlayerStatType.Mental:
                Mental = nextValue;
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
