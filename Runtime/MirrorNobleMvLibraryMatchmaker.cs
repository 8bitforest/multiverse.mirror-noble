using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchUp;
using Mirror;
using Multiverse.LibraryInterfaces;
using Multiverse.Utils;
using NobleConnect.Mirror;
using Reaction;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    [RequireComponent(typeof(Matchmaker))]
    public class MirrorNobleMvLibraryMatchmaker : Matchmaker, IMvLibraryMatchmaker
    {
        public bool Connected => IsReady;
        public RxnEvent OnDisconnected { get; } = new RxnEvent();

        private NobleNetworkManager _networkManager;

        private TaskCompletionSource _joinMatchTask;
        private TaskCompletionSource _createMatchTask;
        private string _matchName;
        private int _maxPlayers;

        private void Awake()
        {
            onLostConnectionToMatchmakingServer = e => OnDisconnected.AsOwner.Invoke();
        }

        private void Start()
        {
            _networkManager = (NobleNetworkManager) NetworkManager.singleton;
        }

        public async Task Connect()
        {
            if (IsReady)
                return;

            var connectTask = new TaskCompletionSource();
            StartCoroutine(ConnectCoroutine(connectTask));
            await connectTask.Task;
        }

        private IEnumerator ConnectCoroutine(TaskCompletionSource connectTask)
        {
            yield return ConnectToMatchmaker();
            yield return new WaitUntilTimeout(() => IsReady);
            connectTask.SetResult();
        }

        public new async Task Disconnect()
        {
            if (!IsReady)
                return;

            var disconnectTask = new TaskCompletionSource();
            StartCoroutine(DisconnectCoroutine(disconnectTask));
            await disconnectTask.Task;
            OnDisconnected.AsOwner.Invoke();
        }

        private IEnumerator DisconnectCoroutine(TaskCompletionSource disconnectTask)
        {
            base.Disconnect();
            yield return new WaitUntilTimeout(() => !IsReady);
            disconnectTask.SetResult();
        }

        public async Task CreateMatch(string matchName, int maxPlayers)
        {
            _createMatchTask = new TaskCompletionSource();
            _matchName = matchName;
            _maxPlayers = maxPlayers;
            _networkManager.StartHost();
            await _createMatchTask.Task;
        }

        internal void OnServerPrepared(string hostAddress, ushort hostPort)
        {
            CreateMatch(_maxPlayers, new Dictionary<string, MatchData>
            {
                {"Name", _matchName},
                {"MaxPlayers", _maxPlayers},
                {"HostAddress", hostAddress},
                {"HostPort", (int) hostPort}
            }, (success, _) =>
            {
                if (success)
                    _createMatchTask.SetResult();
                else
                    _createMatchTask.SetException(new MvException());
            });
        }

        public async Task JoinMatch(MvMatch match)
        {
            _joinMatchTask = new TaskCompletionSource();
            JoinMatch(new Match(Convert.ToInt64(match.Id)), (success, match) =>
            {
                if (success)
                {
                    _networkManager.networkAddress = match.matchData["HostAddress"];
                    _networkManager.networkPort = match.matchData["HostPort"];
                    _networkManager.StartClient();
                }
                else
                    _joinMatchTask.SetException(new MvException());
            });
            await _joinMatchTask.Task;
        }

        internal void OnClientConnect()
        {
            _joinMatchTask?.SetResult();
            _joinMatchTask = null;
        }

        public async Task<IEnumerable<MvMatch>> GetMatchList()
        {
            var task = new TaskCompletionSource<IEnumerable<MvMatch>>();
            GetMatchList((success, matches) =>
            {
                if (success)
                    task.SetResult(matches.Select(m => new MvMatch(
                        m.matchData["Name"].stringValue, m.id.ToString(), m.matchData["MaxPlayers"].intValue)));
                else
                    task.SetException(new MvException());
            });
            return await task.Task;
        }
    }
}