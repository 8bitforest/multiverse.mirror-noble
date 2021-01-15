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

        public void Disconnect()
        {
            _networkManager.StopHost();
        }

        internal void OnStopServer()
        {
            OnDisconnected.AsOwner.Invoke();
        }
    }
}