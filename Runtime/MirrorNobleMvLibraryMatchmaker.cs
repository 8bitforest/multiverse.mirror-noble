using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MatchUp;
using Mirror;
using Multiverse.LibraryInterfaces;
using Multiverse.Utils;
using NobleConnect.Mirror;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    [RequireComponent(typeof(Matchmaker))]
    public class MirrorNobleMvLibraryMatchmaker : Matchmaker, IMvLibraryMatchmaker
    {
        public Connected Connected { get; set; }
        public Disconnected Disconnected { get; set; }
        public Connected ConnectedToMatch { get; set; }

        public ErrorHandler HostMatchError { get; set; }
        public ErrorHandler JoinMatchError { get; set; }

        public MatchesUpdated MatchesUpdated { get; set; }

        private NobleNetworkManager _networkManager;

        private byte[] _matchData;

        private Dictionary<int, Match> _matches = new Dictionary<int, Match>();

        private void Awake()
        {
            onLostConnectionToMatchmakingServer = e => Disconnected();
        }

        private void Start()
        {
            _networkManager = (NobleNetworkManager) NetworkManager.singleton;
        }

        public void Connect()
        {
            if (IsReady)
            {
                Connected();
                return;
            }

            StartCoroutine(ConnectCoroutine());
        }

        private IEnumerator ConnectCoroutine()
        {
            yield return ConnectToMatchmaker();
            yield return new WaitUntilTimeout(() => IsReady);
            Connected();
        }

        void IMvLibraryMatchmaker.Disconnect()
        {
            if (!IsReady)
            {
                Disconnected();
                return;
            }

            StartCoroutine(DisconnectCoroutine());
        }

        private IEnumerator DisconnectCoroutine()
        {
            base.Disconnect();
            yield return new WaitUntilTimeout(() => !IsReady);
            Disconnected();
        }

        public void HostMatch(byte[] data)
        {
            _matchData = data;
            _networkManager.StartHost();
        }

        internal void OnServerPrepared(string hostAddress, ushort hostPort)
        {
            var matchData = new Dictionary<string, MatchData>
            {
                {"HostAddress", hostAddress},
                {"HostPort", (int) hostPort},
                {"MultiverseData", Base64Encode(_matchData)}
            };
            CreateMatch(int.MaxValue, matchData, (success, _) =>
            {
                if (success)
                    ConnectedToMatch();
                else
                    HostMatchError("");
            });
        }

        public void JoinMatch(int libId)
        {
            JoinMatch(_matches[libId], (success, match) =>
            {
                if (success)
                {
                    _networkManager.networkAddress = match.matchData["HostAddress"];
                    _networkManager.networkPort = match.matchData["HostPort"];
                    _networkManager.StartClient();
                }
                else
                    JoinMatchError("");
            });
        }

        internal void OnClientConnect()
        {
            if (NetworkManager.singleton.mode != NetworkManagerMode.Host)
                ConnectedToMatch();
        }

        public void UpdateMatchList()
        {
            GetMatchList((success, matches) =>
            {
                if (success)
                    UpdateMatchList(matches);
                else
                    Debug.LogWarning("Could not update match list!");
            });
        }

        private void UpdateMatchList(Match[] matches)
        {
            _matches = matches.ToDictionary(r => r.GetHashCode());
            MatchesUpdated(matches.Select(m
                => (m.GetHashCode(), Base64Decode(m.matchData["MultiverseData"].stringValue))));
        }

        private string Base64Encode(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes);
        }

        private byte[] Base64Decode(string str)
        {
            return System.Convert.FromBase64String(str);
        }
    }
}