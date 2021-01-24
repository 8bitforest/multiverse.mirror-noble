using Mirror;
using Multiverse.Tests;
using Multiverse.Tests.Utils;
using NUnit.Framework;
using UnityEngine;

namespace Multiverse.MirrorNoble.Tests
{
    [SetUpFixture]
    public class LibraryTestSetUp : MultiverseTestSetUp<MirrorNobleLibraryAdder> { }

    [TestFixture]
    public class LibraryMatchmakerTests : MatchmakerTests { }

    [TestFixture]
    public class LibraryClientNotJoinedTests : ClientNotJoinedTests { }
    
    [TestFixture]
    public class LibraryClientJoinedTests : ClientJoinedTests { }

    [TestFixture]
    public class LibraryServerNotCreatedTests : ServerNotCreatedTests { }
    
    [TestFixture]
    public class LibraryServerCreatedTests : ServerCreatedTests { }
    
    [TestFixture]
    public class LibraryClientMessageTests : ClientMessageTests { }
    
    [TestFixture]
    public class LibraryServerMessageTests : ServerMessageTests { }

    public class MirrorNobleLibraryAdder : IMvTestLibraryAdder
    {
        public void AddLibrary(GameObject gameObject)
        {
            gameObject.AddComponent<Ignorance>().Channels = new[]
            {
                IgnoranceChannelTypes.Reliable,
                IgnoranceChannelTypes.Unreliable
            };
            gameObject.AddComponent<MirrorNobleMvLibrary>();
        }
    }
}