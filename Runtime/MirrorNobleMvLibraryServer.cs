using System.Collections;
using System.Threading.Tasks;
using Mirror;
using NobleConnect.Mirror;
using Reaction;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    public class MirrorNobleMvLibraryServer : MonoBehaviour, IMvLibraryServer
    {
        public RxnEvent OnDisconnected { get; } = new RxnEvent();

        private NobleNetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = (NobleNetworkManager) NetworkManager.singleton;
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
            _networkManager.StopServer();
            yield return new WaitUntilTimeout(() => !NobleServer.active, 5);
            disconnectTask.SetResult();
        }

        internal void OnServerConnect(NetworkConnection networkConnection)
        {
            Debug.Log("OnServerConnect has been called!");
            foreach (var conn in NobleServer.connections.Values)
            {
                if (conn.connectionId != networkConnection.connectionId)
                {
                    Debug.Log("Sending connector a connection!");
                    Debug.Log($"Is host? {conn.connectionId == NobleServer.localConnection.connectionId}");
                    Debug.Log($"{conn.connectionId} {NobleServer.localConnection.connectionId}");
                    networkConnection.Send(new ClientConnectedMessage
                    {
                        Id = conn.connectionId,
                        IsHost = conn.connectionId == NobleServer.localConnection.connectionId
                    });
            
                    Debug.Log("Sending existing player the new connection!");
                    conn.Send(new ClientConnectedMessage
                    {
                        Id = networkConnection.connectionId,
                        IsHost = networkConnection.connectionId == NobleServer.localConnection.connectionId
                    });
                }
            }
        }

        internal void OnServerDisconnect(NetworkConnection networkConnection)
        {
            NobleServer.SendToAll(new ClientDisconnectedMessage
            {
                Id = networkConnection.connectionId
            });
        }
    }
}