using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerGameData : INetworkSerializable, IEquatable<PlayerGameData>
{
    public ulong ClientId;
    public FixedString32Bytes Username;
    public int ColorSelectionIndex;
    public int TurretIndex;
    public int TankBodyIndex;
    public int Score;
    public int WinPoints;
    public float DamageMuliplier;
    public float SpeedMultiplier;
    public bool ShowIndicator;

    public PlayerGameData(ulong clientId, FixedString32Bytes username, int colorSelectionIndex, int turretIndex, int tankBodyIndex, int score = 0, int winPoints = 0, float damageMuliplier = 1, float speedMultiplier = 1, bool showIndicator = false)
    {
        ClientId = clientId;
        Username = username;
        ColorSelectionIndex = colorSelectionIndex;
        TurretIndex = turretIndex;
        TankBodyIndex = tankBodyIndex;
        Score = score;
        WinPoints = winPoints;
        DamageMuliplier = damageMuliplier;
        SpeedMultiplier = speedMultiplier;
        ShowIndicator = showIndicator;
    }

    public bool Equals(PlayerGameData other)
    {
        return ClientId == other.ClientId && Username == other.Username && ColorSelectionIndex == other.ColorSelectionIndex && TurretIndex == other.TurretIndex && TankBodyIndex == other.TankBodyIndex && Score == other.Score && WinPoints == other.WinPoints && DamageMuliplier == other.DamageMuliplier && SpeedMultiplier == other.SpeedMultiplier && ShowIndicator == other.ShowIndicator;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Username);
        serializer.SerializeValue(ref ColorSelectionIndex);
        serializer.SerializeValue(ref TurretIndex);
        serializer.SerializeValue(ref TankBodyIndex);
        serializer.SerializeValue(ref Score);
        serializer.SerializeValue(ref WinPoints);
        serializer.SerializeValue(ref DamageMuliplier);
        serializer.SerializeValue(ref SpeedMultiplier);
        serializer.SerializeValue(ref ShowIndicator);
    }
}
