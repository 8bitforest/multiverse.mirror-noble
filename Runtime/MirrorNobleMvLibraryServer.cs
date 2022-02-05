using System;
using System.Collections;
using Mirror;
using Multiverse.LibraryInterfaces;
using Multiverse.Utils;
using NobleConnect.Mirror;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    public class MirrorNobleMvLibraryServer : MonoBehaviour, IMvLibraryServer
    {
        public Disconnected Disconnected { get; set; }
        public ServerByteMessageReceiver MessageReceiver { get; set; }
        public PlayerConnected PlayerConnected { get; set; }
        public PlayerDisconnected PlayerDisconnected { get; set; }

        private NobleNetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = (NobleNetworkManager) NetworkManager.singleton;
            NetworkServer.RegisterHandler<MvNetworkMessage>((connection, message) =>
                MessageReceiver(connection.connectionId, message.Data));
        }

        public void Disconnect()
        {
            StartCoroutine(DisconnectCoroutine());
        }

        public void SendMessageToPlayer(int libId, ArraySegment<byte> message, bool reliable)
        {
            NobleServer.connections[libId].Send(new MvNetworkMessage(message),
                reliable ? Channels.Reliable : Channels.Unreliable);
        }

        public void SendMessageToAll(ArraySegment<byte> message, bool reliable)
        {
            NobleServer.SendToAll(new MvNetworkMessage(message),
                reliable ? Channels.Reliable : Channels.Unreliable);
        }

        private IEnumerator DisconnectCoroutine()
        {
            _networkManager.StopServer();
            yield return new WaitUntilTimeout(() => !NobleServer.active);
            Disconnected();
        }

        internal void OnServerConnect(NetworkConnection networkConnection)
        {
            PlayerConnected(networkConnection.connectionId);
        }

        internal void OnServerDisconnect(NetworkConnection networkConnection)
        {
            PlayerDisconnected(networkConnection.connectionId);
        }
    }
}