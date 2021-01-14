using System;
using Mirror;
using NobleConnect.Mirror;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    [RequireComponent(typeof(Transport), typeof(MirrorNobleMvLibraryMatchmaker))]
    public class MirrorNobleMvLibrary : NobleNetworkManager, IMvLibrary
    {
        public override void Awake()
        {
            transport = GetComponent<Transport>();
            base.Awake();
        }

        public IMvServer GetServer()
        {
            throw new System.NotImplementedException();
        }

        public IMvClient GetClient()
        {
            throw new System.NotImplementedException();
        }

        public IMvLibraryMatchmaker GetMatchmaker()
        {
            return GetComponent<MirrorNobleMvLibraryMatchmaker>();
        }

        public override void OnServerPrepared(string hostAddress, ushort hostPort)
        {
            GetComponent<MirrorNobleMvLibraryMatchmaker>().OnServerPrepared(hostAddress, hostPort);
            base.OnServerPrepared(hostAddress, hostPort);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            // Client: Connected to server
            GetComponent<MirrorNobleMvLibraryMatchmaker>().OnClientConnect();
            base.OnClientConnect(conn);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            // Client: Disconnected from server
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
            base.OnStopServer();
        }
    }
}