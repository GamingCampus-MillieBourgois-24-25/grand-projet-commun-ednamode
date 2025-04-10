using Unity.Collections;
using Unity.Netcode;

public struct ChatMessage : INetworkSerializable
{
    public FixedString64Bytes sender;
    public FixedString512Bytes content;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref sender);
        serializer.SerializeValue(ref content);
    }
}
