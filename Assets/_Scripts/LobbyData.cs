using Unity.Netcode;

public struct LobbyData : INetworkSerializable
{
    public int WinScore;
    public bool AllowUpgrades;
    public int MapIndex;

    public LobbyData(int winScore, bool allowUpgrades, int mapIndex)
    {
        WinScore = winScore;
        AllowUpgrades = allowUpgrades;
        MapIndex = mapIndex;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref WinScore);
        serializer.SerializeValue(ref AllowUpgrades);
        serializer.SerializeValue(ref MapIndex);
    }
}
