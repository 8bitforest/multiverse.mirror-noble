using System;
using System.Collections;
using Mirror;
using Multiverse.LibraryInterfaces;
using Multiverse.Utils;
using NobleConnect.Mirror;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    public class MirrorNobleMvLibraryClient : MonoBehaviour, IMvLibraryClient
    {
        public Disconnected Disconnected { get; set; }
        public ClientByteMessageReceiver MessageReceiver { get; set; }

        private NobleNetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = (NobleNetworkManager) NetworkManager.singleton;
            NetworkClient.RegisterHandler<MvNetworkMessage>(message => MessageReceiver(message.Data));
        }

        public void Disconnect()
        {
            StartCoroutine(DisconnectCoroutine());
        }

        public void SendMessageToServer(ArraySegment<byte> message, bool reliable)
        {
            NetworkClient.Send(new MvNetworkMessage(message), reliable ? Channels.Reliable : Channels.Unreliable);
        }

        private IEnumerator DisconnectCoroutine()
        {
            _networkManager.StopClient();
            yield return new WaitUntilTimeout(() => !NetworkClient.active && !NetworkClient.isConnected);
            Disconnected();
        }

        internal void OnClientDisconnect()
        {
            // For some reason this doesn't get called on the host for the host's client
            Disconnected();
        }
    }
}