using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CombatZombieManager : MonoBehaviour
{

    private static CombatZombieManager instance;
    public static CombatZombieManager Instance => instance;

    private static G G => G.Instance;

    public Transform TR;
    public GameObject CombatZombiePrefab;

    public MeshRenderer boundsMesh;

    public Bounds ZombieSpawnBounds;
    public int zombiesCount;
    public bool zombieInitiatedCombat;

    private readonly List<CombatZombie> ActiveCombatZombies = new();
    private readonly List<CombatZombie> ZombieChasingPlayer = new();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ZombieSpawnBounds = boundsMesh.bounds;
        if (G != null)
        {
            zombiesCount = G.zombiesCount;
            zombieInitiatedCombat = G.zombieInitiatedCombat;
        }
        else
        {

        }
        SpawnZombies();
    }

    public void CombatZombieDetectedPlayer(CombatZombie zombie)
    {
        ZombieChasingPlayer.Add(zombie);
        Player.Instance.SetEscapeActive(false);
    }

    public void CombatZombieDied(CombatZombie zombie)
    {
        ActiveCombatZombies.Remove(zombie);
        ZombieChasingPlayer.Remove(zombie);

        Debug.Log(ZombieChasingPlayer.Count);
        if (ZombieChasingPlayer.Count == 0)
        {
            Player.Instance.SetEscapeActive(true);
        }
        if (ActiveCombatZombies.Count == 0)
        {
            Player.Instance.NoMoreZombiesLeaveCombat();
        }
    }



    private void SpawnZombies()
    {
        ActiveCombatZombies.Clear();
        ZombieChasingPlayer.Clear();

        Player.Instance.SetEscapeActive(true);

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

            if (zombieInitiatedCombat)
            {
                Vector3 playerPos = CombatPlayer.Instance.TR.localPosition;
                TR.LookAt(new Vector3(playerPos.x, pos.y, playerPos.z), Vector3.up);
            }
            combatZombie.RB.isKinematic = true;
            ActiveCombatZombies.Add(combatZombie);
        }
    }

}
