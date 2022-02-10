using IgnoranceTransport;
using Multiverse.LibraryInterfaces;
using Multiverse.Tests.Backend;
using UnityEngine;

namespace Multiverse.MirrorNoble.Tests
{
    public class MirrorNobleMvLibraryTestSuite : IMvLibraryTestSuite
    {
        public string Name => "MirrorNoble";

        public IMvLibrary AddLibrary(GameObject gameObject)
        {
            gameObject.AddComponent<Ignorance>().Channels = new[]
            {
                IgnoranceChannelTypes.Reliable,
                IgnoranceChannelTypes.Unreliable
            };
            return gameObject.AddComponent<MirrorNobleMvLibrary>();
        }
    }
}