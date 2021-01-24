using System;
using Mirror;

namespace Multiverse.MirrorNoble
{
    public struct ClientConnectedMessage : NetworkMessage
    {
        public int Id;
        public bool IsHost;
    }

    public struct ClientDisconnectedMessage : NetworkMessage
    {
        public int Id;
    }

    public struct MvNetworkMessage : NetworkMessage
    {
        public ArraySegment<byte> Data;

        public MvNetworkMessage(ArraySegment<byte> data)
        {
            Data = data;
        }
    }
}