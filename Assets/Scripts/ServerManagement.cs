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
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport.Relay;
using System.Collections.Concurrent;
using System.Linq;

public class ServerManagement : MonoBehaviour
{
    [SerializeField] private GameObject relayUI;
    [SerializeField] private GameObject createRoomSettingsUI;
    [SerializeField] private GameObject joinRoomSettingsUI;
    [SerializeField] private TMP_InputField userCount;
    [SerializeField] private TMP_InputField joinCode;
    [SerializeField] private TextMeshProUGUI joinCodeDisplay;
    private string lobbyId;
    ConcurrentQueue<string> createdLobbyIds = new ConcurrentQueue<string>();

    private async void Start()
    {   
        try
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

        finally 
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient) CloseMenu();
        }


    }






    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tilde) || Input.GetKeyDown(KeyCode.BackQuote))
        {
            Debug.Log("~ pressed");
            joinRoomSettingsUI.SetActive(false);
            createRoomSettingsUI.SetActive(false);
            Cursor.visible = !relayUI.activeSelf;
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
        Cursor.visible = false;
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
            try
            {
                await CreateLobby("Lobby", count);
                CloseMenu();
            }
            finally { }

        }

    }

    public async void JoinRoom()
    {
        
        try
        {
            await PurgeRoom();
        }
        finally
        {
            try
            {
                Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode.text);
                string relayJoinCode = joinedLobby.Data["JoinCode"].Value;
                await StartClientWithRelay(relayJoinCode);
                joinCodeDisplay.text = "Join Code: " + joinCode.text;
                CloseMenu();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }

        //bool status = await StartClientWithRelay(joinCode.text);
        //Debug.Log(status);
        //if (status) 
    }


    private async Task PurgeRoom()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            try
            {
                string playerId = AuthenticationService.Instance.PlayerId;
                await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
            }
            finally { }
            try
            {
                RemoveLobby();
            }
            finally { }
            try
            {
                NetworkManager.Singleton.Shutdown();
            }
            finally { }
            try
            {
                UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.DisconnectLocalClient();
            }
            finally { }
            joinCodeDisplay.text = "Join Code: ";
        }
        
        return;
    }



    public async Task<bool> StartHostWithRelay(Allocation allocation)
    {
        try
        {
            //Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);

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


    async Task<Lobby> CreateLobby(string lobbyName="Lobby", int maxPlayers=60, bool isPrivate=false)
    {

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = isPrivate;

        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        options.Data = new Dictionary<string, DataObject>
            {
                {
                    "JoinCode",
                    new DataObject(
                        DataObject.VisibilityOptions.Member, // only players in the lobby can see this key
                        joinCode                             // the actual relay join code for this lobby
                    )
                }
            };


        // Lobby parameters code goes here...
        // See 'Creating a Lobby' for example parameters
        var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        // Heartbeat the lobby every 15 seconds.
        await StartHostWithRelay(allocation);
        lobbyId = lobby.Id;
        joinCodeDisplay.text = "Join Code: "+ lobby.LobbyCode;
        createdLobbyIds.Enqueue(lobby.Id);
        StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
        return lobby;
    }

    IEnumerator HeartbeatLobbyCoroutine(string lobbyIdIn, float waitTimeSeconds)
    {
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyIdIn);
            yield return new WaitForSecondsRealtime(waitTimeSeconds);
        }
    }


    void RemoveLobby()
    {
        while (createdLobbyIds.TryDequeue(out var lobbyIdOut))
        {
            LobbyService.Instance.DeleteLobbyAsync(lobbyIdOut);
        }
        return;
    }
    void OnApplicationQuit()
    {
        RemoveLobby();
    }



}
