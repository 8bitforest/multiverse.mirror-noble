using Mirror;
using Multiverse.LibraryInterfaces;
using NobleConnect.Mirror;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    [RequireComponent(typeof(Transport), typeof(MirrorNobleMvLibraryMatchmaker))]
    public class MirrorNobleMvLibrary : NobleNetworkManager, IMvLibrary
    {
        private MirrorNobleMvLibraryMatchmaker _matchmaker;
        private MirrorNobleMvLibraryHost _host;
        private MirrorNobleMvLibraryServer _server;
        private MirrorNobleMvLibraryClient _client;

        public override void Awake()
        {
            transport = GetComponent<Transport>();
            autoCreatePlayer = false;
            // TODO
            // disconnectInactiveConnections = true;
            // NetworkServer.disconnectInactiveConnections = true;
            base.Awake();
        }
        
        public IMvLibraryHost GetHost()
        {
            return _host = gameObject.AddComponent<MirrorNobleMvLibraryHost>();
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
            Destroy(_host);
            Destroy(_server);
            Destroy(_client);
            _host = null;
            _server = null;
            _client = null;
        }

        public void SetTimeout(float seconds)
        {
            // TODO
            // disconnectInactiveTimeout = seconds;
            // NetworkServer.disconnectInactiveTimeout = seconds;
        }

        public override void OnServerPrepared(string hostAddress, ushort hostPort)
        {
            // Server: Server started
            base.OnServerPrepared(hostAddress, hostPort);
            _matchmaker.OnServerPrepared(hostAddress, hostPort);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            // Client: Connected to server
            base.OnClientConnect(conn);
            _matchmaker.OnClientConnect();
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            // Client: Disconnected from server
            base.OnClientDisconnect(conn);
            _client.OnClientDisconnect();
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            // Server: Client connected
            base.OnServerConnect(conn);
            _server.OnServerConnect(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            // Server: Client disconnected
            base.OnServerDisconnect(conn);
            _server.OnServerDisconnect(conn);
        }
    }
}