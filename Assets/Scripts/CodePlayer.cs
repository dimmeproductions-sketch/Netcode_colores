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
            int randomColorIndex = Random.Range(0, presetColors.Length);
            ColorIndex.Value = randomColorIndex;
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