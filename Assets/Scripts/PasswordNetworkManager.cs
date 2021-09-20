using UnityEngine;
using MLAPI;
using TMPro;
using System.Text;
using System.Collections.Generic;

namespace MultiTest
{
    public class PasswordNetworkManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private GameObject passwordEntryUI;

        private static Dictionary<ulong, PlayerData> clientData;
        

        private void Start()
        {
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }

        private void OnDestroy()
        {
            // Prevent error in the editor
            if (NetworkManager.Singleton == null) { return; }

            NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }

        public void Host()
        {
            clientData = new Dictionary<ulong, PlayerData>();
            clientData[NetworkManager.Singleton.LocalClientId] = new PlayerData(nameInputField.text);

            // Hook up password approval check
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.StartHost(new Vector3(0f, 0f, 0f), Quaternion.Euler(0f, 0f, 0f));

        }

        public void Client()
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                password = passwordInputField.text,
                playerName = nameInputField.text
            });

            byte[] payloadBytes = Encoding.ASCII.GetBytes(payload);

            // Set password ready to send to the server to validate
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            NetworkManager.Singleton.StartClient();
        }

        public void Leave()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.StopHost();
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.StopClient();
            }

            passwordEntryUI.SetActive(true);
        }

        public static PlayerData? GetPlayerData(ulong clientId)
        {
            if (clientData.TryGetValue(clientId, out PlayerData playerData))
            {
                return playerData;
            }

            return null;
        }

        private void HandleServerStarted()
        {
            // Temporary workaround to treat host as client
            if (NetworkManager.Singleton.IsHost)
            {
                HandleClientConnected(NetworkManager.Singleton.ServerClientId);
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            // Are we the client that is connecting?
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                passwordEntryUI.SetActive(false);
            }
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                clientData.Remove(clientId);
            }

            // Are we the client that is disconnecting?
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                passwordEntryUI.SetActive(true);
            }
        }

        private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
        {
            string payload = Encoding.ASCII.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
            Debug.Log("Payload Pass: " + connectionPayload.password);
            Debug.Log("Payload Bane: " + connectionPayload.playerName);
            Debug.Log("FieldText: " + passwordInputField.text);
            bool approveConnection = connectionPayload.password == passwordInputField.text;

            Vector3 spawnPos = Vector3.zero;
            Quaternion spawnRot = Quaternion.identity;

            if (approveConnection)
            {
                switch (NetworkManager.Singleton.ConnectedClients.Count)
                {
                    case 1:
                        spawnPos = new Vector3(0f, 0f, 0f);
                        spawnRot = Quaternion.Euler(0f, 0f, 0f);
                        break;
                    case 2:
                        spawnPos = new Vector3(0f, 0f, 0f);
                        spawnRot = Quaternion.Euler(0f, 0f, 0f);
                        break;
                }

                clientData[clientId] = new PlayerData(connectionPayload.playerName);
            }

            callback(true, null, approveConnection, spawnPos, spawnRot);
        }
    }
}