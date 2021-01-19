using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Mirror;
using NobleConnect.Mirror;
using Reaction;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    public class MirrorNobleMvLibraryClient : MonoBehaviour, IMvLibraryClient
    {
        public IMvConnection LocalConnection { get; private set; }
        public RxnSet<IMvConnection> Connections { get; } = new RxnSet<IMvConnection>();

        public RxnEvent OnDisconnected { get; } = new RxnEvent();

        private NobleNetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = (NobleNetworkManager) NetworkManager.singleton;

            LocalConnection = MvNetworkManager.I.IsHost
                ? NewConnection(NobleServer.localConnection.connectionId, true, true)
                : NewConnection(-1, true, false);

            Connections.AsOwner.Add(LocalConnection);

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

            yield return new WaitUntilTimeout(() => !NetworkClient.active && !NetworkClient.isConnected, 5);
            disconnectTask.SetResult();
        }

        internal void OnClientDisconnect()
        {
            // For some reason this doesn't get called on the host for the host's client
            OnDisconnected.AsOwner.Invoke();
        }

        internal void OnServerConnect(NetworkConnection networkConnection)
        {
            if (networkConnection.connectionId != LocalConnection.Id)
                Connections.AsOwner.Add(NewConnection(networkConnection.connectionId, false, false));
        }

        internal void OnServerDisconnect(NetworkConnection networkConnection)
        {
            Connections.AsOwner.Remove(Connections.First(c => c.Id == networkConnection.connectionId));
        }

        private void OnClientConnectedMessage(ClientConnectedMessage msg)
        {
            Connections.AsOwner.Add(NewConnection(msg.Id, false, msg.IsHost));
        }

        private void OnClientDisconnectedMessage(ClientDisconnectedMessage msg)
        {
            Connections.AsOwner.Remove(Connections.First(c => c.Id == msg.Id));
        }

        private static IMvConnection NewConnection(int id, bool isLocal, bool isHost)
        {
            return new DefaultMvConnection
            {
                Name = null,
                Id = id,
                IsHost = isHost,
                IsLocal = isLocal
            };
        }
    }
}