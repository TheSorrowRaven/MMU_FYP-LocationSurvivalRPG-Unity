using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CombatZombieManager : MonoBehaviour
{
    private static G G => G.Instance;

    public Transform TR;
    public GameObject CombatZombiePrefab;

    public MeshRenderer boundsMesh;

    public Bounds ZombieSpawnBounds;
    public int zombiesCount;


    private void Start()
    {
        ZombieSpawnBounds = boundsMesh.bounds;
        if (G != null)
        {
            zombiesCount = G.zombiesCount;
        }
        else
        {

        }
        SpawnZombies();
    }


    private void SpawnZombies()
    {

        for (int i = 0; i < zombiesCount; i++)
        {
            Vector3 pos = G.RandomPointInBounds(ZombieSpawnBounds);
            pos.y = 0;

            Vector3 rotCheck = pos;
            rotCheck.y = 100;

            if (!Physics.Raycast(rotCheck, Vector3.down, out RaycastHit hit, 200f, 1 << 11))
            {
                throw new System.Exception("Cannot raycast??");
            }
            pos = hit.point;
            float yRot = Random.value * 360f;
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(0, yRot, 0);
            CombatZombie combatZombie = Instantiate(CombatZombiePrefab, pos, rotation, TR).GetComponent<CombatZombie>();
            combatZombie.RB.isKinematic = true;
        }
    }

}
