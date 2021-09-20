using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

namespace MultiTest
{
    public class ParticleSpawner : NetworkBehaviour
    {
        [SerializeField] private GameObject particlePrefab;
        private void Update()
        {
            //if (!IsOwner)
            //{
            //    return;
            //}
            //if(!Input.GetKeyDown(KeyCode.Space))
            //{
            //    return;
            //}
            //SpawnParticleServerRpc();
            //SpawnParticle();

        }
        [ServerRpc(Delivery = RpcDelivery.Unreliable)]
        private void SpawnParticleServerRpc()
        {
            SpawnParticleClientRpc();
        }
        [ClientRpc(Delivery = RpcDelivery.Unreliable)]
        private void SpawnParticleClientRpc()
        {
            if(!IsOwner)
            {
                return;
            }
            SpawnParticle();
        }
        private void SpawnParticle()
        {
            Instantiate(particlePrefab, transform.position, transform.rotation);
        }
    }
}
