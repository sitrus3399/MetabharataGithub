using Unity.Netcode;

namespace Metabharata.Multiplayer.Network.LobbySystem
{
    /// <summary>
    /// Handles serialization, deserialization, and network sync of LobbyDataModel.
    /// </summary>
    public class LobbyDataSyncService
    {
        private readonly NetworkManager _networkManager;

        public LobbyDataSyncService(NetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        public byte[] SerializeLobbyData(LobbyDataModel model)
        {
            return LobbyDataSerializer.Serialize(model);
        }

        public LobbyDataModel DeserializeLobbyData(string data)
        {
            return LobbyDataSerializer.Deserialize(data);
        }

        public void SendLobbyDataToClients(LobbyDataModel model, ulong localClientId, string messageName)
        {
            var data = SerializeLobbyData(model);
            using var writer = new FastBufferWriter(data.Length, Unity.Collections.Allocator.Temp);
            writer.WriteBytesSafe(data, data.Length);

            foreach (var clientId in _networkManager.ConnectedClientsIds)
            {
                if (clientId == localClientId) continue;
                _networkManager.CustomMessagingManager.SendNamedMessage(messageName, clientId, writer);
            }
        }
    }
}