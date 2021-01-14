using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchUp;
using Mirror;
using NobleConnect.Mirror;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    [RequireComponent(typeof(Matchmaker))]
    public class MirrorNobleMvLibraryMatchmaker : MonoBehaviour, IMvLibraryMatchmaker
    {
        public bool Connected => _matchmaker.IsReady;
        
        private NobleNetworkManager _networkManager;
        private Matchmaker _matchmaker;

        private TaskCompletionSource _joinMatchTask;
        private TaskCompletionSource _createMatchTask;
        private string _matchName;
        private int _maxPlayers;

        private void Awake()
        {
            _matchmaker = GetComponent<Matchmaker>();
        }

        private void Start()
        {
            _networkManager = (NobleNetworkManager) NetworkManager.singleton;
        }

        public async Task Connect()
        {
            if (_matchmaker.IsReady)
                return;

            var connectTask = new TaskCompletionSource();
            StartCoroutine(ConnectCoroutine(connectTask));
            await connectTask.Task;
        }

        private IEnumerator ConnectCoroutine(TaskCompletionSource connectTask)
        {
            yield return _matchmaker.ConnectToMatchmaker();
            yield return new WaitUntilTimeout(() => _matchmaker.IsReady, 5);
            connectTask.SetResult();
        }

        public async Task Disconnect()
        {
            if (!_matchmaker.IsReady)
                return;

            var disconnectTask = new TaskCompletionSource();
            StartCoroutine(DisconnectCoroutine(disconnectTask));
            await disconnectTask.Task;
        }

        private IEnumerator DisconnectCoroutine(TaskCompletionSource disconnectTask)
        {
            _matchmaker.Disconnect();
            yield return new WaitUntilTimeout(() => !_matchmaker.IsReady, 5);
            disconnectTask.SetResult();
        }

        public async Task CreateMatch(string matchName, int maxPlayers)
        {
            _createMatchTask = new TaskCompletionSource();
            _matchName = matchName;
            _maxPlayers = maxPlayers;
            _networkManager.StartServer();
            await _createMatchTask.Task;
        }

        internal void OnServerPrepared(string hostAddress, ushort hostPort)
        {
            _matchmaker.CreateMatch(_maxPlayers, new Dictionary<string, MatchData>
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

        public async Task JoinMatch(IMvMatch match)
        {
            _joinMatchTask = new TaskCompletionSource();
            _matchmaker.JoinMatch(new Match(Convert.ToInt64(match.Id)), (success, match) =>
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
            _joinMatchTask.SetResult();
            _joinMatchTask = null;
        }

        public async Task<IEnumerable<IMvMatch>> GetMatchList()
        {
            var task = new TaskCompletionSource<IEnumerable<IMvMatch>>();
            _matchmaker.GetMatchList((success, matches) =>
            {
                if (success)
                    task.SetResult(matches.Select(m => new DefaultMvMatch
                    {
                        Id = m.id.ToString(),
                        Name = m.matchData["Name"].stringValue,
                        MaxPlayers = m.matchData["MaxPlayers"].intValue
                    }));
                else
                    task.SetException(new MvException());
            });
            return await task.Task;
        }
    }
}