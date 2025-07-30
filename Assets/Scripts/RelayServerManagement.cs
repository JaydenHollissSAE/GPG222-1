using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RelayServerManagement : MonoBehaviour
{
    [SerializeField] private GameObject relayUI;
    [SerializeField] private GameObject createRoomSettingsUI;
    [SerializeField] private GameObject joinRoomSettingsUI;
    [SerializeField] private TMP_InputField userCount;
    [SerializeField] private TMP_InputField joinCode;

    private async void Start()
    {   
        // Initialize Unity Services
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed In As " + AuthenticationService.Instance.PlayerId);
        };

        // Sign in anonymously
        await AuthenticationService.Instance.SignInAnonymouslyAsync();



    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            Debug.Log("~ pressed");
            joinRoomSettingsUI.SetActive(false);
            createRoomSettingsUI.SetActive(false);
            relayUI.SetActive(!relayUI.activeSelf);
        }
    }

    public void MenuBack()
    {
        relayUI.SetActive(false);
        joinRoomSettingsUI.SetActive(false);
        relayUI.SetActive(true);
    }
    public void CloseMenu()
    {
        relayUI.SetActive(false);
        joinRoomSettingsUI.SetActive(false);
        createRoomSettingsUI.SetActive(false);
    }

    public void CreateRoomUI()
    {
        relayUI.SetActive(false);
        createRoomSettingsUI.SetActive(true);
    }
    public void JoinRoomUI()
    {
        relayUI.SetActive(false);
        joinRoomSettingsUI.SetActive(true);
    }

    public async void CreateRoom()
    {
        await PurgeRoom();
        int count = 60;
        try
        {
            count = int.Parse(userCount.text.Replace(" ", ""));
        }
        finally
        {
            if (count > 60) count = 60;
            bool status = await StartHostWithRelay(count);
            Debug.Log(status);
            if (status) CloseMenu();
        }

    }

    public async void JoinRoom()
    {
        await PurgeRoom();
        bool status = await StartClientWithRelay(joinCode.text);
        Debug.Log(status);
        if (status) CloseMenu();
    }


    private async Task PurgeRoom()
    {
        try
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.Shutdown();
                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.DisconnectLocalClient();
            }

        }
        finally { }
        return;
    }



    public async Task<bool> StartHostWithRelay(int maxConnections)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code: " + joinCode);

            // Set the relay server data for the host
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port, allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData);

            // Start hosting
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started with Relay.");
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay Create Error: " + e);
            return false;
        }
    }

    public async Task<bool> StartClientWithRelay(string joinCode)
    {
        try
        {
            Debug.Log("Trying to join Relay with code: " + joinCode);


            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(joinAllocation.RelayServer.IpV4, (ushort)joinAllocation.RelayServer.Port, joinAllocation.AllocationIdBytes, joinAllocation.Key, joinAllocation.ConnectionData, joinAllocation.HostConnectionData);

            // Start the client connection
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client joined Relay.");
            return true;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Error: " + e);
            return false;
        }
    }


}
