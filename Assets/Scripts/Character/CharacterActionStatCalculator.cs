using UnityEngine;

public static class CharacterActionStatCalculator
{
    private const float MinMoveSpeed = 4f;
    private const float MaxMoveSpeed = 6f;
    private const float MinJumpForce = 6f;
    private const float MaxJumpForce = 8.5f;
    private const float MinReceiveRange = 1.3f;
    private const float MaxReceiveRange = 2.4f;
    private const float MinReceiveHeight = 2f;
    private const float MaxReceiveHeight = 3.2f;
    private const float MinReceiveForwardSpeed = 4.2f;
    private const float MaxReceiveForwardSpeed = 6f;
    private const float MinSetUpSpeed = 7f;
    private const float MaxSetUpSpeed = 9f;
    private const float MinSpikeRange = 1.8f;
    private const float MaxSpikeRange = 2.7f;
    private const float MinSpikeHeight = 3f;
    private const float MaxSpikeHeight = 6f;
    private const float MinSpikeForwardSpeed = 6.5f;
    private const float MaxSpikeForwardSpeed = 10f;
    private const float MinSpikeDownSpeed = 2.2f;
    private const float MaxSpikeDownSpeed = 4.5f;
    private const float MinServeForwardSpeed = 6.5f;
    private const float MaxServeForwardSpeed = 9.5f;
    private const float MinServeUpSpeed = 3.2f;
    private const float MaxServeUpSpeed = 5f;

    public static float GetMoveSpeed(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Speed, fallback, MinMoveSpeed, MaxMoveSpeed);
    }

    public static float GetJumpForce(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Jump, fallback, MinJumpForce, MaxJumpForce);
    }

    public static float GetReceiveRange(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Receive, fallback, MinReceiveRange, MaxReceiveRange);
    }

    public static float GetReceiveMaxHeight(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Receive, fallback, MinReceiveHeight, MaxReceiveHeight);
    }

    public static float GetReceiveForwardSpeed(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Receive, fallback, MinReceiveForwardSpeed, MaxReceiveForwardSpeed);
    }

    public static float GetSetUpSpeed(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Set, fallback, MinSetUpSpeed, MaxSetUpSpeed);
    }

    public static float GetSpikeRange(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Jump, fallback, MinSpikeRange, MaxSpikeRange);
    }

    public static float GetSpikeMaxHeight(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Jump, fallback, MinSpikeHeight, MaxSpikeHeight);
    }

    public static float GetSpikeForwardSpeed(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Power, fallback, MinSpikeForwardSpeed, MaxSpikeForwardSpeed);
    }

    public static float GetSpikeDownSpeed(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Power, fallback, MinSpikeDownSpeed, MaxSpikeDownSpeed);
    }

    public static float GetServeForwardSpeed(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Serve, fallback, MinServeForwardSpeed, MaxServeForwardSpeed);
    }

    public static float GetServeUpSpeed(CharacterRole role, float fallback)
    {
        return GetScaledValue(role, PlayerStatType.Serve, fallback, MinServeUpSpeed, MaxServeUpSpeed);
    }

    private static float GetScaledValue(CharacterRole role, PlayerStatType statType, float fallback, float minValue, float maxValue)
    {
        if (role == null || role.Stats == null)
        {
            return fallback;
        }

        float ratio = Mathf.Clamp01(role.Stats.GetValue(statType) / 100f);
        return Mathf.Lerp(minValue, maxValue, ratio);
    }
}
