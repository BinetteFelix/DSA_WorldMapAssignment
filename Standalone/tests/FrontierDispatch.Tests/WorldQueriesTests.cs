using NUnit.Framework;
using FrontierDispatch.Core;

namespace FrontierDispatch.Tests
{
    public sealed class WorldQueriesTests
    {
        [Test]
        public void FindLocationById_ReturnsMatchingLocation()
        {
            var queries = new WorldQueries(WorldDatasetFactory.CreateLarge());
            Location? location = queries.FindLocationByIdInstant(4);
            Assert.That(location, Is.Not.Null);
            Assert.That(location!.Name, Is.EqualTo("Outpost 4"));
        }

        [Test]
        public void FindLocationById_ReturnsNullWhenMissing()
        {
            var queries = new WorldQueries(WorldDatasetFactory.CreateSmall());
            Assert.That(queries.FindLocationByIdInstant(9999), Is.Null);
        }

        [Test]
        public void FindOutgoingConnections_ReturnsOnlyMatchingOrigin()
        {
            var queries = new WorldQueries(WorldDatasetFactory.CreateSmall());
            var outgoing = queries.FindOutgoingConnections(2);
            Assert.That(outgoing.Count, Is.EqualTo(3));
            for (int i = 0; i < outgoing.Count; ++i)
            {
                Assert.That(outgoing[i].FromLocationId, Is.EqualTo(2));
            }
        }
    }
}
