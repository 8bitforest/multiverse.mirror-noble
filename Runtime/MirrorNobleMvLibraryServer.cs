using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using Multiverse.LibraryInterfaces;
using Multiverse.Messaging;
using Multiverse.Utils;
using NobleConnect.Mirror;
using Reaction;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    public class MirrorNobleMvLibraryServer : MonoBehaviour, IMvLibraryServer
    {
        public RxnDictionary<int, MvConnection> Clients { get; } = new RxnDictionary<int, MvConnection>();
        
        public RxnEvent OnDisconnected { get; } = new RxnEvent();

        private NobleNetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = (NobleNetworkManager) NetworkManager.singleton;

            if (MvNetworkManager.I.IsHost)
                Clients.AsOwner[NobleServer.localConnection.connectionId] =
                    NewConnection(NobleServer.localConnection.connectionId, true, true);
        }

        public async Task Disconnect()
        {
            var disconnectTask = new TaskCompletionSource();
            StartCoroutine(DisconnectCoroutine(disconnectTask));
            await disconnectTask.Task;
            OnDisconnected.AsOwner.Invoke();
        }

        public void SetMessageReceiver(ByteMessageReceiver receiver)
        {
            NetworkServer.RegisterHandler<MvNetworkMessage>((connection, message) =>
                receiver(Clients[connection.connectionId], message.Data));
        }

        public void SendMessageToClient(MvConnection connection, ArraySegment<byte> message, bool reliable)
        {
            NobleServer.connections[connection.Id].Send(new MvNetworkMessage(message),
                reliable ? Channels.DefaultReliable : Channels.DefaultUnreliable);
        }

        public void SendMessageToAll(ArraySegment<byte> message, bool reliable)
        {
            NobleServer.SendToAll(new MvNetworkMessage(message),
                reliable ? Channels.DefaultReliable : Channels.DefaultUnreliable);
        }

        private IEnumerator DisconnectCoroutine(TaskCompletionSource disconnectTask)
        {
            _networkManager.StopServer();
            yield return new WaitUntilTimeout(() => !NobleServer.active);
            disconnectTask.SetResult();
        }

        internal void OnServerConnect(NetworkConnection networkConnection)
        {
            foreach (var conn in NobleServer.connections.Values)
            {
                if (conn.connectionId != networkConnection.connectionId)
                {
                    networkConnection.Send(new ClientConnectedMessage
                    {
                        Id = conn.connectionId,
                        IsHost = conn.connectionId == NobleServer.localConnection.connectionId
                    });

                    conn.Send(new ClientConnectedMessage
                    {
                        Id = networkConnection.connectionId,
                        IsHost = networkConnection.connectionId == NobleServer.localConnection.connectionId
                    });
                }
            }

            if (networkConnection.connectionId != NobleServer.localConnection.connectionId)
                Clients.AsOwner[networkConnection.connectionId] =
                    NewConnection(networkConnection.connectionId, false, false);
        }

        internal void OnServerDisconnect(NetworkConnection networkConnection)
        {
            NobleServer.SendToAll(new ClientDisconnectedMessage
            {
                Id = networkConnection.connectionId
            });
            Clients.AsOwner.Remove(networkConnection.connectionId);
        }

        private static MvConnection NewConnection(int id, bool isLocal, bool isHost)
        {
            return new MvConnection(null, id, isHost, isLocal);
        }
    }
}