using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<int> ColorIndex = new NetworkVariable<int>();
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        private readonly Color[] presetColors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan };
        private Renderer m_Renderer;

        private void Awake()
        {
            m_Renderer = GetComponent<Renderer>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                ChangeColor();
                Move();
                AssignUniqueColor(isSpawning: true);
            }
        }

        public void ChangeColor()
        {
            SubmitColorRequestRpc();
        }

        public void Move()
        {
            SubmitPositionRequestRpc();
        }

        [Rpc(SendTo.Server)]
        private void SubmitPositionRequestRpc(RpcParams rpcParams = default)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
            
        }

        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }

        [Rpc(SendTo.Server)]
        private void SubmitColorRequestRpc(RpcParams rpcParams = default)
        {
            AssignUniqueColor(isSpawning: false);
        }

        private void AssignUniqueColor(bool isSpawning)
        {
            // Creamos un conjunto para registrar qué índices de colores ya están ocupados por OTROS jugadores
            HashSet<int> occupiedColors = new HashSet<int>();

            foreach (var client in NetworkManager.Singleton.ConnectedClients.Values)
            {
                if (client.PlayerObject != null)
                {
                    var otherPlayer = client.PlayerObject.GetComponent<HelloWorldPlayer>();
                    // Almacenamos el color de los demás jugadores
                    if (otherPlayer != null)
                    {
                        occupiedColors.Add(otherPlayer.ColorIndex.Value);
                    }
                }
            }

            // Creamos una lista con los índices de colores que están totalmente libres
            List<int> freeColors = new List<int>();
            for (int i = 0; i < presetColors.Length; i++)
            {
                // Si nadie más lo usa...
                if (!occupiedColors.Contains(i))
                {
                    // Si estamos cambiando de color (no spawneando), intentamos que tampoco sea nuestro color actual
                    if (!isSpawning && i == ColorIndex.Value)
                    {
                        continue; 
                    }
                    freeColors.Add(i);
                }
            }

            // Si hay colores disponibles que cumplan las condiciones, asignamos uno al azar
            if (freeColors.Count > 0)
            {
                ColorIndex.Value = freeColors[Random.Range(0, freeColors.Count)];
            }
            // NOTA: Si la partida está llena (6/6 jugadores) y todos tienen un color, 
            // freeColors se quedará vacío al intentar cambiar de color. En ese caso, 
            // el jugador simplemente mantendrá su color actual de forma segura para no romper la regla.
        }

        private void Update()
        {
            transform.position = Position.Value;
            if (m_Renderer != null && ColorIndex.Value >= 0 && ColorIndex.Value < presetColors.Length)
            {
                m_Renderer.material.color = presetColors[ColorIndex.Value];
            }
        }
    }
}