using System.Collections.Concurrent;
using TacMesh.Core.assets.routing.prophet;

namespace TacMesh.Tests;

public class UnitTest1
{
    [Fact]
    public void Test_PRoPHET_routing_exchange_serializer()
    {
        // Arrange
        ProphetExchangeSerializer serializer = new ProphetExchangeSerializer();

        ConcurrentDictionary<string, double> originalVector = new()
        {
            ["Node-01"] = 0.12,
            ["Node-02"] = 0.36,
            ["Node-03"] = 0.54,
            ["Node-04"] = 1.0,
            ["Node-05"] = 0.0,
        };

        // Act
        byte[] serializedBytes = serializer.Serialize(originalVector);
        ConcurrentDictionary<string, double> deserializedVector = serializer.Deserialize(serializedBytes);

        // Assert
        foreach (string nodeId in deserializedVector.Keys)
        {
            double expectedValue = originalVector[nodeId];
            double actualValue = deserializedVector[nodeId];

            // passes if | actual - expected | <= 0.002
            //
            Assert.Equal(expectedValue, actualValue, tolerance: 0.002);
        }
    }
}
