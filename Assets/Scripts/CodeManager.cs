using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {
        private NetworkManager m_NetworkManager;
        private const int MaxPlayers = 6;

        private void Awake()
        {
            m_NetworkManager = GetComponent<NetworkManager>();
            m_NetworkManager.NetworkConfig.ConnectionApproval = true;
            m_NetworkManager.ConnectionApprovalCallback = ApprovalCheck;
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!m_NetworkManager.IsClient && !m_NetworkManager.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();
                SubmitNewPosition();
                SubmitColorChange();
            }

            GUILayout.EndArea();
        }

        private void StartButtons()
        {
            if (GUILayout.Button("Host")) m_NetworkManager.StartHost();
            if (GUILayout.Button("Client")) m_NetworkManager.StartClient();
            if (GUILayout.Button("Server")) m_NetworkManager.StartServer();
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            // Comprobamos si el número de clientes conectados ya llegó al límite
            if (m_NetworkManager.ConnectedClientsIds.Count >= MaxPlayers)
            {
                response.Approved = false;
                response.Reason = "El servidor está lleno (Máximo 6 jugadores).";
                Debug.LogWarning("Conexión rechazada: Servidor lleno.");
            }
            else
            {
                response.Approved = true;
                response.CreatePlayerObject = true; // Permite que aparezca su Prefab de jugador
                Debug.Log($"Conexión aprobada. Jugadores actuales: {m_NetworkManager.ConnectedClientsIds.Count + 1}/{MaxPlayers}");
            }

            // Indicamos que la decisión ya está tomada y no está pendiente
            response.Pending = false;
        }

        private void StatusLabels()
        {
            var mode = m_NetworkManager.IsHost ?
                "Host" : m_NetworkManager.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " + m_NetworkManager.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
            GUILayout.Label($"Jugadores: {m_NetworkManager.ConnectedClientsIds.Count}/{MaxPlayers}");
        }

        private void SubmitColorChange()
        {
            if (GUILayout.Button(m_NetworkManager.IsServer ? "Change color" : "Request color change"))
            {
                if (m_NetworkManager.IsServer && !m_NetworkManager.IsClient)
                {
                    foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
                        m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().ChangeColor();
                }
                else
                {
                    var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<HelloWorldPlayer>();
                    player.ChangeColor();
                }
            }
        }

        private void SubmitNewPosition()
        {
            if (GUILayout.Button(m_NetworkManager.IsServer ? "Move" : "Request Position Change"))
            {
                if (m_NetworkManager.IsServer && !m_NetworkManager.IsClient)
                {
                    foreach (ulong uid in m_NetworkManager.ConnectedClientsIds)
                        m_NetworkManager.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().Move();
                }
                else
                {
                    var playerObject = m_NetworkManager.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<HelloWorldPlayer>();
                    player.Move();
                }
            }
        }
    }
}