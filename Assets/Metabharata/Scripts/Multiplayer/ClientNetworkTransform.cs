using UnityEngine;
using Unity.Netcode.Components;

public class ClientNetworkTransform : NetworkTransform
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        CanCommitToTransform = IsOwner;
    }

    public override void OnUpdate()
    {
        CanCommitToTransform = IsOwner;
        base.OnUpdate();
        //if (NetworkManager != null)
        //{
        //    if (NetworkManager.IsConnectedClient || NetworkManager.IsListening)
        //    {
        //        if (CanCommitToTransform)
        //        {
        //            TryCommitTransformToServer(transform, NetworkManager.LocalTime.Time);
        //        }
        //    }
        //}
    }

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
