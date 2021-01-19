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
}