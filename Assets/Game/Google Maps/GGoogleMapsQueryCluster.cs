using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Consists of 4 GGoogleMapsQueryLocation. This will be used to determine what is displayed and what is not
/// </summary>
public class GGoogleMapsQueryCluster
{
    private static G G => G.Instance;
    private static GGoogleMapsService GGoogleMapsService => GGoogleMapsService.Instance;

    public GGoogleMapsQueryLocation Location1;
    public GGoogleMapsQueryLocation Location2;
    public GGoogleMapsQueryLocation Location3;
    public GGoogleMapsQueryLocation Location4;

    public Vector2d[] QueryLocations;

    public IEnumerable<GGoogleMapsQueryLocation> Locations()
    {
        yield return Location1;
        yield return Location2;
        yield return Location3;
        yield return Location4;
    }

    private void SetLocation(int i, GGoogleMapsQueryLocation location)
    {
        switch (i)
        {
            case 0: Location1 = location; return;
            case 1: Location2 = location; return;
            case 2: Location3 = location; return;
            case 3: Location4 = location; return;
        }
    }

    private void ClearCluster()
    {
        Location1 = null;
        Location2 = null;
        Location3 = null;
        Location4 = null;
    }

    public void PrepareQueryCluster(Vector2d[] queryLocations)
    {
        ClearCluster();
        QueryLocations = queryLocations;
    }

    public void QueryAllLocations(System.Action<GGoogleMapsQueryCluster> clusterCompleteAction = null)
    {
        Task[] tasks = new Task[QueryLocations.Length];
        for (int i = 0; i < QueryLocations.Length; i++)
        {
            int ii = i;
            tasks[i] = GGoogleMapsService.StartNearbyQueryLocation(QueryLocations[i], (queryLocation) =>
            {
                SetLocation(ii, queryLocation);
            });
        }
        G.StartCoroutine(QueryAllLocationsCoroutine(tasks, clusterCompleteAction));
    }

    private IEnumerator QueryAllLocationsCoroutine(Task[] tasks, System.Action<GGoogleMapsQueryCluster> clusterCompleteAction)
    {
        for (int i = 0; i < tasks.Length; i++)
        {
            yield return new WaitUntil(() => tasks[i].IsCompleted);
        }
        clusterCompleteAction?.Invoke(this);
    }

}
