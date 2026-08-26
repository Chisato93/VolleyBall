public enum POSITION
{
    OutsideHitter,              // OH
    OppositeHitter,             // OP
    MiddleBlocker,              // MB
    Setter,                     // S
    Libero,                     // L
    DefensiveSpecialist         // DS
}

public enum MatchState
{
    None,
    Ready,
    ServeReady,
    Rally,
    PointScored,
    MatchOver
}

public enum TeamSide
{
    Player,
    Opponent
}

public enum BallTouchAction
{
    Serve,
    Receive,
    Set,
    Spike,
    Block
}
