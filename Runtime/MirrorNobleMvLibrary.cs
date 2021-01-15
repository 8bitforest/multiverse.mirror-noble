using Mirror;
using NobleConnect.Mirror;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    [RequireComponent(typeof(Transport), typeof(MirrorNobleMvLibraryMatchmaker))] public class MirrorNobleMvLibrary : NobleNetworkManager, IMvLibrary
    {
        private MirrorNobleMvLibraryMatchmaker _matchmaker;
        private MirrorNobleMvLibraryServer _server;
        private MirrorNobleMvLibraryClient _client;
        
        public override void Awake()
        {
            transport = GetComponent<Transport>();
            base.Awake();
        }

        public IMvLibraryServer GetServer()
        {
            return _server = gameObject.AddComponent<MirrorNobleMvLibraryServer>();
        }

        public IMvLibraryClient GetClient()
        {
            return _client = gameObject.AddComponent<MirrorNobleMvLibraryClient>();
        }

        public IMvLibraryMatchmaker GetMatchmaker()
        {
            return _matchmaker = GetComponent<MirrorNobleMvLibraryMatchmaker>();
        }

        public void CleanupAfterDisconnect()
        {
            _server = null;
            _client = null;
        }

        public override void OnServerPrepared(string hostAddress, ushort hostPort)
        {
            _matchmaker.OnServerPrepared(hostAddress, hostPort);
            base.OnServerPrepared(hostAddress, hostPort);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            // Client: Connected to server
            _matchmaker.OnClientConnect();
            base.OnClientConnect(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            // Client: Disconnected from server
            _client.OnClientDisconnect();
            base.OnClientDisconnect(conn);
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            // Server: Client connected
            base.OnServerConnect(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            // Server: Client disconnected
            base.OnServerDisconnect(conn);
        }

        public override void OnStopServer()
        {
            // Server: Server stopped
            _server.OnStopServer();
            base.OnStopServer();
        }
    }
}