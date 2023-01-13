using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TestCar.Core;
using UnityEngine;
using UnityEngine.AI;

namespace TestCar
{
    public class ZombieSpawner : MonoBehaviour
    {
        [SerializeField] private float _spawnRadius;
        [SerializeField] private GameObject _zombiePrefab;
        [SerializeField] private float _spawnRate = 5f;
        [SerializeField] private int _maxZombies = 15;

        private readonly List<Zombie> _spawnedZombies = new ();

        private IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForSeconds(_spawnRate);
                if (VehiclesSelector.SelectedVehicle == null)
                    continue;

                if (_spawnedZombies.Sum(z => z.Anim.enabled ? 1 : 0) >= _maxZombies)
                    continue;

                var spawnPoint = transform.position + Random.insideUnitSphere * _spawnRadius;
                if (!NavMesh.SamplePosition(spawnPoint, out var hit, float.MaxValue, Physics.AllLayers))
                    continue;
                spawnPoint = hit.position;

                var isInitDone = false;
                for (var i = 0; i < _spawnedZombies.Count; i++)
                {
                    if (_spawnedZombies[i].Anim.enabled)
                        continue;

                    _spawnedZombies[i].Init();
                    isInitDone = true;
                    break;
                }
                if (isInitDone)
                    continue;

                var tempZombieGO = Instantiate(_zombiePrefab, spawnPoint, Quaternion.identity);
                tempZombieGO.SetActive(true);
                _spawnedZombies.Add(tempZombieGO.GetComponent<Zombie>());
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _spawnRadius);
        }
    }
}
