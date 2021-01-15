using Mirror;
using NobleConnect.Mirror;
using Reaction;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    public class MirrorNobleMvLibraryClient : MonoBehaviour, IMvLibraryClient
    {
        public RxnEvent OnDisconnected { get; } = new RxnEvent();

        private NobleNetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = (NobleNetworkManager) NetworkManager.singleton;
        }

        public void Disconnect()
        {
            _networkManager.client.connection.Disconnect();
            _networkManager.client.connection.InvokeHandler(new DisconnectMessage(), -1);
        }

        internal void OnClientDisconnect()
        {
            OnDisconnected.AsOwner.Invoke();
        }
    }
}