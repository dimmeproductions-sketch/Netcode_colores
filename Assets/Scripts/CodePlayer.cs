using Unity.Netcode;
using UnityEngine;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        public NetworkVariable<int> ColorIndex = new NetworkVariable<int>();
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
            }
        }

        public void ChangeColor()
        {
            SubmitColorRequestRpc();
        }

        [Rpc(SendTo.Server)]
        private void SubmitColorRequestRpc(RpcParams rpcParams = default)
        {
            int randomColorIndex = Random.Range(0, presetColors.Length);
            ColorIndex.Value = randomColorIndex;
        }

        private void Update()
        {
            if (m_Renderer != null && ColorIndex.Value >= 0 && ColorIndex.Value < presetColors.Length)
            {
                m_Renderer.material.color = presetColors[ColorIndex.Value];
            }
        }
    }
}