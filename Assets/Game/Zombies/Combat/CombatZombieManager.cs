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
    public int chasingZombies;
    public int unawareZombies;

    private readonly List<CombatZombie> ActiveCombatZombies = new();
    private readonly List<CombatZombie> ZombieChasingPlayer = new();

    public int zombieBaseHealth;
    public int zombieScaledHealth;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ZombieSpawnBounds = boundsMesh.bounds;
        if (G != null)
        {
            chasingZombies = G.chasingZombies;
            unawareZombies = G.unawareZombies;
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

        for (int i = 0; i < chasingZombies; i++)
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
            Quaternion rotation = Quaternion.identity;
            CombatZombie combatZombie = Instantiate(CombatZombiePrefab, pos, rotation, TR).GetComponent<CombatZombie>();

            Vector3 playerPos = CombatPlayer.Instance.TR.localPosition;
            TR.LookAt(new Vector3(playerPos.x, pos.y, playerPos.z), Vector3.up);

            //combatZombie.RB.isKinematic = true;
            combatZombie.ForceDetectPlayer();
            combatZombie.health = Random.Range(zombieBaseHealth, Player.Instance.Level * zombieScaledHealth);

            ActiveCombatZombies.Add(combatZombie);
        }

        for (int i = 0; i < unawareZombies; i++)
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

            //combatZombie.RB.isKinematic = true;
            Debug.Log("No forcing to detect player");
            combatZombie.health = Random.Range(zombieBaseHealth, Player.Instance.Level * zombieScaledHealth);

            ActiveCombatZombies.Add(combatZombie);
        }

    }

}
