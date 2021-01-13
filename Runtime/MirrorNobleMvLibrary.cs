using Mirror;
using UnityEngine;

namespace Multiverse.MirrorNoble
{
    [RequireComponent(typeof(Transport), typeof(MirrorNobleMvLibraryMatchmaker))]
    public class MirrorNobleMvLibrary : MonoBehaviour, IMvLibrary
    {
        public IMvLibraryMatchmaker GetMatchmaker()
        {
            return GetComponent<MirrorNobleMvLibraryMatchmaker>();
        }
    }
}