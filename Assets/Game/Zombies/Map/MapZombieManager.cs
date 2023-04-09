using Mapbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class MapZombieManager : MonoBehaviour
{

    private static MapZombieManager instance;
    public static MapZombieManager Instance => instance;

    private static G G => G.Instance;
    private static PopulationDensityMapper PopulationDensityMapper => PopulationDensityMapper.Instance;

    [SerializeField] private Transform container;
    [SerializeField] private GeoRelativeGrid grid;

    [SerializeField] private GameObject MapZombiePrefab;
    [SerializeField] private float densityDivision;

    [SerializeField] private float cooldownToClearMapZombies;
    [SerializeField] private Vector2 distanceFromPlayerToClearMapZombies;
    [System.NonSerialized] private float cooldownCount;

    [System.NonSerialized] private readonly List<MapZombie> zombies = new();
    [System.NonSerialized] private readonly HashSet<Vector2Int> cellsSpawned = new();


    private void Awake()
    {
        instance = this;

        zombies.Clear();
        cellsSpawned.Clear();
    }

    private void Start()
    {

        G.Mapbox.OnUpdated += MapUpdated;
    }

    private void Update()
    {
        UpdateClearMapZombies();
    }

    public MapZombie GetRandomZombie()
    {
        return zombies[UnityEngine.Random.Range(0, zombies.Count)];
    }

    public int GetChasingZombiesCountWith(MapZombie tappedZombie, out int unawareZombies)
    {
        int chasingZombies = 0;
        unawareZombies = 0;
        if (tappedZombie != null)
        {
            if (tappedZombie.isChasingPlayer)
            {
                chasingZombies += tappedZombie.zombiesCount;
            }
            else
            {
                unawareZombies = tappedZombie.zombiesCount;
            }
        }
        for (int i = 0; i < zombies.Count; i++)
        {
            if (zombies[i] == tappedZombie)
            {
                continue;
            }
            if (zombies[i].isChasingPlayer)
            {
                chasingZombies += zombies[i].zombiesCount;
            }
        }
        return chasingZombies;
    }

    private void UpdateClearMapZombies()
    {
        cooldownCount -= Time.deltaTime;
        if (cooldownCount > 0)
        {
            return;
        }
        cooldownCount = cooldownToClearMapZombies;
        for (int i = 0; i < zombies.Count; i++)
        {
            MapZombie zombie = zombies[i];
            Vector3 pos = zombie.ThisTR.localPosition;
            if (Mathf.Abs(pos.x) > distanceFromPlayerToClearMapZombies.x && Mathf.Abs(pos.z) > distanceFromPlayerToClearMapZombies.y)
            {
                zombie.gameObject.Destroy();
                zombies.RemoveAt(i);
                i--;
            }
        }
    }

    public void PlayerUpdate(Vector2Int currentCell)
    {
        SpawnZombiesAroundCell(currentCell);
    }

    private void SpawnZombiesAroundCell(Vector2Int playerCell)
    {
        float density = PopulationDensityMapper.GetDensity(G.Location);
        float spawnRate = Mathf.Max(density / densityDivision, 1);  //At least 1 zombie

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int cell = new(playerCell.x + x, playerCell.y + y);
                if (!cellsSpawned.Add(cell))
                {
                    continue;
                }
                SpawnZombiesInCell(cell, spawnRate);
            }
        }

    }

    private void SpawnZombiesInCell(Vector2Int cell, float spawnRate)
    {
        Vector2 cellSize = grid.CellSize;
        Vector2 min = grid.CellToWorld(cell);
        Vector2 max = min + cellSize;

        for (int i = 0; i < spawnRate; i++)
        {
            Vector2 spawnPos = G.RandomPosition(min, max);
            Vector3 pos = new(spawnPos.x, 0, spawnPos.y);

            //Vector3 raycastOrigin = pos;
            //raycastOrigin.y = 200;
            //if (Physics.Raycast(raycastOrigin, Vector3.down))
            //{
            //    continue;
            //}
            if (spawnPos.sqrMagnitude < GameSettings.Instance.MapZombieDistanceToDetectPlayerSqr)
            {
                continue;
            }

            MapZombie mapZombie = Instantiate(MapZombiePrefab, pos, Quaternion.Euler(0, UnityEngine.Random.value * 360, 0), container).GetComponent<MapZombie>();
            mapZombie.cellPos = cell;
            mapZombie.GeoLocation = G.WorldToGeo(pos);
            mapZombie.zombiesCount = UnityEngine.Random.Range(1, Convert.ToInt32(Player.Instance.Level * 1.5f));

            zombies.Add(mapZombie);
        }
    }



    private void MapUpdated()
    {
        grid.MapUpdated();
        foreach (MapZombie zombie in zombies)
        {
            zombie.MapUpdated();
        }
    }

    public Vector2Int GeoToCell(Vector2d geo)
    {
        return grid.GeoToCell(geo);
    }


}
