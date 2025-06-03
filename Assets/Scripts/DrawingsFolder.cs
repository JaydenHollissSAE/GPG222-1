using Unity.Netcode;
using UnityEngine;

public class DrawingsFolder : NetworkBehaviour
{
    static public DrawingsFolder instance;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        instance = this;
    }
}
