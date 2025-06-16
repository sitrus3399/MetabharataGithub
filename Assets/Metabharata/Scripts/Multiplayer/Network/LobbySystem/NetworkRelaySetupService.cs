using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

namespace Metabharata.Multiplayer.Network.LobbySystem
{
    public class NetworkRelaySetupService
    {
        private readonly NetworkManager _networkManager;
        private readonly LobbyMessageHandler _messageHandler;

        public NetworkRelaySetupService(NetworkManager networkManager, LobbyMessageHandler messageHandler)
        {
            _networkManager = networkManager;
            _messageHandler = messageHandler;
        }

        public async Task<(Allocation allocation, string joinCode)> CreateRelayAllocationAsync(int maxPlayers)
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return (allocation, joinCode);
        }

        public void SetupHostTransport(Allocation allocation)
        {
            var relayServerData = allocation.ToRelayServerData("dtls");
            _networkManager.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            _networkManager.OnConnectionEvent -= _messageHandler.HandleConnectionEvent;
            _networkManager.OnConnectionEvent += _messageHandler.HandleConnectionEvent;
            _networkManager.OnPreShutdown -= _messageHandler.HandlePreShutdown;
            _networkManager.OnPreShutdown += _messageHandler.HandlePreShutdown;
        }

        public async Task<JoinAllocation> JoinRelayAsync(string joinCode)
        {
            return await RelayService.Instance.JoinAllocationAsync(joinCode);
        }

        public void SetupClientTransport(JoinAllocation joinAllocation)
        {
            var relayServerData = joinAllocation.ToRelayServerData("dtls");
            _networkManager.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            _networkManager.OnConnectionEvent -= _messageHandler.HandleConnectionEvent;
            _networkManager.OnConnectionEvent += _messageHandler.HandleConnectionEvent;
            _networkManager.OnPreShutdown -= _messageHandler.HandlePreShutdown;
            _networkManager.OnPreShutdown += _messageHandler.HandlePreShutdown;
        }

        public bool StartHost()
        {
            return _networkManager.StartHost();
        }

        public bool StartClient()
        {
            return _networkManager.StartClient();
        }
    }
}
