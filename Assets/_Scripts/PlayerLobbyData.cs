using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerLobbyData : INetworkSerializable, IEquatable<PlayerLobbyData>
{
    public ulong ClientId;
    public FixedString32Bytes Username;
    public int ColorSelectionIndex;
    public int TurretIndex;
    public int TankBodyIndex;
    public bool IsReady;

    public PlayerLobbyData(ulong clientId, FixedString32Bytes username = default, int colorSelectionIndex = 0, int turretIndex = 0, int tankBodyIndex = 0, bool isReady = false)
    {
        ClientId = clientId;
        Username = username;
        ColorSelectionIndex = colorSelectionIndex;
        TurretIndex = turretIndex;
        TankBodyIndex = tankBodyIndex;
        IsReady = isReady;
    }

    public bool Equals(PlayerLobbyData other)
    {
        return ClientId == other.ClientId && Username == other.Username && ColorSelectionIndex == other.ColorSelectionIndex && TurretIndex == other.TurretIndex && TankBodyIndex == other.TankBodyIndex && IsReady == other.IsReady;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref Username);
        serializer.SerializeValue(ref ColorSelectionIndex);
        serializer.SerializeValue(ref TurretIndex);
        serializer.SerializeValue(ref TankBodyIndex);
        serializer.SerializeValue(ref IsReady);
    }
}
