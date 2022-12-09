//using Mapbox.Utils;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Assertions.Comparers;

//// This is using GGoogleMapsPOI to filter the POIs before they are spawned, so Unity Units are not used
//public class SpatialHashPOI
//{
//    private static GameSettings GameSettings => GameSettings.Instance;
//    private readonly Dictionary<float, List<GGoogleMapsPOI>> hashTable;
//    private float CellSize => GameSettings.UnityUnitsPOISpatialHashCellSize;

//    public SpatialHashPOI()
//    {
//        hashTable = new(new FloatComparer());
//    }

//    public void Add(GGoogleMapsPOI gPOI)
//    {
//        float hash = GetHash(gPOI.UnityPosition);
//        if (!hashTable.ContainsKey(hash))
//        {
//            hashTable[hash] = new List<GGoogleMapsPOI>();
//        }

//        hashTable[hash].Add(gPOI);
//    }

//    public List<GGoogleMapsPOI> Query(Vector2 position, float radius)
//    {
//        List<GGoogleMapsPOI> potentialCollisions = new();

//        // Get the hash of the cell at the given position
//        float hash = GetHash(position);

//        // If the hash table contains objects at the given hash, add them to the list of potential collisions
//        if (hashTable.ContainsKey(hash))
//        {
//            potentialCollisions.AddRange(hashTable[hash]);
//        }

//        // Check the neighboring cells (within the given radius) to see if they contain any objects
//        // that may be within range of the given position
//        for (float x = -radius; x <= radius; x += CellSize)
//        {
//            for (float y = -radius; y <= radius; y += CellSize)
//            {
//                float neighborHash = GetHash(position + new Vector2(x, y));
//                if (hashTable.ContainsKey(neighborHash))
//                {
//                    potentialCollisions.AddRange(hashTable[neighborHash]);
//                }
//            }
//        }

//        return potentialCollisions;
//    }

//    private float GetHash(Vector2 position)
//    {
//        float x = (position.x / CellSize);
//        float y = (position.y / CellSize);

//        return x + (y * hashTable.Count);
//    }
//}
