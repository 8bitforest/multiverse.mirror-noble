using System;
using System.Collections;
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
    public class MirrorNobleMvLibraryClient : MonoBehaviour, IMvLibraryClient
    {
        public MvConnection LocalConnection { get; private set; }
        public RxnDictionary<int, MvConnection> Connections { get; } = new RxnDictionary<int, MvConnection>();

        public RxnEvent OnDisconnected { get; } = new RxnEvent();

        private NobleNetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = (NobleNetworkManager) NetworkManager.singleton;

            LocalConnection = MvNetworkManager.I.IsHost
                ? NewConnection(NobleServer.localConnection.connectionId, true, true)
                : NewConnection(-1, true, false);

            Connections.AsOwner[LocalConnection.Id] = LocalConnection;

            _networkManager.client?.RegisterHandler<ClientConnectedMessage>(OnClientConnectedMessage);
            _networkManager.client?.RegisterHandler<ClientDisconnectedMessage>(OnClientDisconnectedMessage);
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
            NetworkClient.RegisterHandler<MvNetworkMessage>((connection, message) =>
                receiver(Connections[connection.connectionId], message.Data));
        }

        public void SendMessageToServer(ArraySegment<byte> message, bool reliable)
        {
            NetworkClient.Send(new MvNetworkMessage(message),
                reliable ? Channels.DefaultReliable : Channels.DefaultUnreliable);
        }

        private IEnumerator DisconnectCoroutine(TaskCompletionSource disconnectTask)
        {
            if (MvNetworkManager.I.IsHost)
            {
                NetworkClient.DisconnectLocalServer();
                _networkManager.StopClient();
            }
            else
            {
                _networkManager.client.connection.Disconnect();
                _networkManager.client.connection.InvokeHandler(new DisconnectMessage(), -1);
            }

            yield return new WaitUntilTimeout(() => !NetworkClient.active && !NetworkClient.isConnected);
            disconnectTask.SetResult();
        }

        internal void OnClientDisconnect()
        {
            // For some reason this doesn't get called on the host for the host's client
            OnDisconnected.AsOwner.Invoke();
        }

        internal void OnServerConnect(NetworkConnection networkConnection)
        {
            if (networkConnection.connectionId == LocalConnection.Id)
                return;

            Connections.AsOwner[networkConnection.connectionId] =
                NewConnection(networkConnection.connectionId, false, false);
        }

        internal void OnServerDisconnect(NetworkConnection networkConnection)
        {
            Connections.AsOwner.Remove(networkConnection.connectionId);
        }

        private void OnClientConnectedMessage(ClientConnectedMessage msg)
        {
            Connections.AsOwner[msg.Id] = NewConnection(msg.Id, false, msg.IsHost);
        }

        private void OnClientDisconnectedMessage(ClientDisconnectedMessage msg)
        {
            Connections.AsOwner.Remove(msg.Id);
        }

        private static MvConnection NewConnection(int id, bool isLocal, bool isHost)
        {
            return new MvConnection(null, id, isHost, isLocal);
        }
    }
}