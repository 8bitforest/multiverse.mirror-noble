using kcp2k;
using Multiverse.Tests;
using NUnit.Framework;
using UnityEngine;

namespace Multiverse.MirrorNoble.Tests
{
    [SetUpFixture]
    public class MirrorNobleTestSetUp : MultiverseTestSetUp<MirrorNobleLibraryAdder> { }
    
    [TestFixture]
    public class MirrorNobleMatchmakerTests : MatchmakerTests { }
    
    [TestFixture]
    public class MirrorNobleMatchmakerClientTests : MatchmakerClientTests { }

    public class MirrorNobleLibraryAdder : IMvTestLibraryAdder
    {
        public void AddLibrary(GameObject gameObject)
        {
            gameObject.AddComponent<KcpTransport>();
            gameObject.AddComponent<MirrorNobleMvLibrary>();
        }
    }
}