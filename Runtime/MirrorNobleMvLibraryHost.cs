using Multiverse.LibraryInterfaces;
using NobleConnect.Mirror;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    public class MirrorNobleMvLibraryHost : MonoBehaviour, IMvLibraryHost
    {
        public int HostLibId => NobleServer.localConnection.connectionId;
    }
}