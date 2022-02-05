using System;
using Mirror;

namespace Multiverse.MirrorNoble
{
    public struct MvNetworkMessage : NetworkMessage
    {
        public ArraySegment<byte> Data;

        public MvNetworkMessage(ArraySegment<byte> data)
        {
            Data = data;
        }
    }
}