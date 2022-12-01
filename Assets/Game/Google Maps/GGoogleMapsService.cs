using Mapbox.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;
using System.Net.Http;
using System.Net.Http.Headers;

using static GGoogleMapsResponses;
using System.Runtime.InteropServices.WindowsRuntime;

public class GGoogleMapsService
{
    public static readonly GGoogleMapsService Instance = new();

    private static GameSettings GameSettings => GameSettings.Instance;

    private static G G => G.Instance;

    private const string APIKey = "AIzaSyBb9FmWLtnQwQu2IAfvVsSOUkqadHZTeMk";

    private GGoogleMapsCache Cache = new();

    private GGoogleMapsService()
    {

    }

    public void Initialize()
    {
        Build();
        Cache = new();
    }

    private void Build()
    {
        BuildPlaceDetails();
        BuildNearbyPlaces();
    }


    #region Place Details
    //Place Details
    //https://developers.google.com/maps/documentation/places/web-service/details

    [System.NonSerialized] private readonly string[] PlaceDetailsAttributes = new string[]
    {
        "photos", "reviews", "url", "address_components"
    };
    private string PlaceDetailsLink;

    private void BuildPlaceDetails()
    {
        PlaceDetailsLink = "https://maps.googleapis.com/maps/api/place/details/json?fields=";
        int i = 0;
        for (; i < PlaceDetailsAttributes.Length - 1; i++)
        {
            PlaceDetailsLink += PlaceDetailsAttributes[i] + "%2C";
        }
        PlaceDetailsLink += PlaceDetailsAttributes[i] + $"&key={APIKey}&place_id=";
    }

    public void MakePlaceDetailsRequest(string placeID)
    {
        string url = PlaceDetailsLink + placeID;
    }

    #endregion

    #region Nearby Places
    //Nearby Search
    //https://developers.google.com/maps/documentation/places/web-service/search-nearby

    private HttpClient NearbyPlacesHttpClient;

    private const string NearbyPlacesURL = "https://maps.googleapis.com/maps/api/place/nearbysearch/json";
    private string NearbyPlacesURLParameters;
    private string NearbyPlacesNextPageURLParameters;

    private void BuildNearbyPlaces()
    {
        NearbyPlacesURLParameters = $"?key={APIKey}&radius=";
        NearbyPlacesNextPageURLParameters = $"?key={APIKey}&pagetoken=";

        NearbyPlacesHttpClient = new();
        NearbyPlacesHttpClient.BaseAddress = new Uri(NearbyPlacesURL);
        NearbyPlacesHttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task StartNearbyQueryLocation(Vector2ds location, Action<GGoogleMapsQueryLocation> queryLocationAction = null)
    {
        GGoogleMapsQueryLocation queryLocation = await NearbyQueryLocation(location);
        queryLocationAction?.Invoke(queryLocation);
    }

    //TASK
    /// <summary>
    /// Entry to query a single location
    /// </summary>
    private async Task<GGoogleMapsQueryLocation> NearbyQueryLocation(Vector2ds location)
    {
        try
        {
            //1. Check Cache
            GGoogleMapsQueryLocation queryLocation = await Cache.TryGetCachedGGoogleMapsQueryLocation(location);
            if (queryLocation != null)
            {
                //Cache hit
                return queryLocation;
            }
            queryLocation = new(location);
            //Cache miss
            //2. Create query task
            Task<GGoogleMapsQueryLocation> task = CreateNearbyQueryTask(queryLocation, location.x, location.y);
            //3. Add to pending cache
            Cache.AddQueryLocationTask(location, task);
            //4. Start task
            task.Start();
            queryLocation = await task;
            if (queryLocation.hasFailed)
            {
                //5. Remove from cache
                Cache.FinishQueryLocationTaskWithFailure(location);
                //6. Return null (MUST BE NULL) so that the request can be automatically made again
                //Disposes queryLocation
                return null;
            }
            //5. Add to cache
            Cache.FinishQueryLocationTask(location, queryLocation);
            //6. Return result
            return queryLocation;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw e;
        }
    }


    //TASK
    /// <summary>
    /// Synchronously creates a task (not started)
    /// </summary>
    private Task<GGoogleMapsQueryLocation> CreateNearbyQueryTask(GGoogleMapsQueryLocation queryLocation, double latitude, double longitude)
    {
        string urlParams = NearbyPlacesURLParameters + $"{GameSettings.POIRadius}&location={latitude}%2C{longitude}";
        Debug.Log($"TASK> NearbySearch: {NearbyPlacesURL}{urlParams}");
        Task<GGoogleMapsQueryLocation> task = new(() => RequestNearbyPlacesHandler(queryLocation, urlParams).Result);
        //Task is not started yet
        return task;
    }

    private enum NearbyPlacesStatus
    {
        Failed, //Continuously failed
        NetworkFailed,  //not response.IsSuccessStatusCode
        APIFailed,  //not "OK"
        ContinueNextPage,
        Completed,

        DelaySuccessful,
    }
    //TASK
    private async Task<GGoogleMapsQueryLocation> RequestNearbyPlacesHandler(GGoogleMapsQueryLocation queryLocation, string urlParams)
    {
        int retryCount = 0;
        NearbyPlacesStatus status = await RequestNearbyPlaces(urlParams, queryLocation, retryCount++);
        while (true)
        {
            bool breakOut = false;
            switch (status)
            {
                case NearbyPlacesStatus.NetworkFailed:
                case NearbyPlacesStatus.APIFailed:
                    status = await DelayRetryOrFail(urlParams, retryCount);
                    if (status == NearbyPlacesStatus.DelaySuccessful)
                    {
                        status = await RequestNearbyPlaces(urlParams, queryLocation, retryCount);
                        break;
                    }
                    continue;   //Failed
                case NearbyPlacesStatus.ContinueNextPage:
                    retryCount = 0;
                    urlParams = CreateNearbyPlacesNextPageRequestURL(queryLocation.nextPageToken);
                    status = await DelayRetryOrFail(urlParams, retryCount);
                    if (status == NearbyPlacesStatus.DelaySuccessful)
                    {
                        status = await RequestNearbyPlaces(urlParams, queryLocation, retryCount);
                        break;
                    }
                    continue;   //Failed
                case NearbyPlacesStatus.Completed:
                    breakOut = true;
                    break;
                case NearbyPlacesStatus.Failed:
                    queryLocation.hasFailed = true;
                    breakOut = true;
                    break;
            }
            if (breakOut)
            {
                break;
            }
        }
        return queryLocation;
    }

    //TASK
    private async Task<NearbyPlacesStatus> RequestNearbyPlaces(string urlParams, GGoogleMapsQueryLocation queryLocation, int retryCount)
    {
        HttpResponseMessage response = await NearbyPlacesHttpClient.GetAsync(urlParams);

        if (!response.IsSuccessStatusCode)
        {
            //Network Error
            Debug.Log($"Network Error: {response.ReasonPhrase} (URL: {response.RequestMessage.RequestUri})");
            return NearbyPlacesStatus.NetworkFailed;
        }
        string res = await response.Content.ReadAsStringAsync();
        NearbySearchResponse nearbySearchResponse = JsonConvert.DeserializeObject<NearbySearchResponse>(res);
        string status = nearbySearchResponse.Status;
        if (status != "OK")
        {
            Debug.LogWarning($"NearbyPlaces returned Status:{status} for URL: {response.RequestMessage.RequestUri}, Retry Count:{retryCount}");
            return NearbyPlacesStatus.APIFailed;
        }

        Debug.Log($"NearbyPlaces Success with Status OK (URL: {response.RequestMessage.RequestUri}) - {res}");

        //Fill queryLocation
        ProcessNearbySearchResponse(queryLocation, nearbySearchResponse);

        //Next page
        if (nearbySearchResponse.NextPageToken != null)
        {
            queryLocation.nextPageToken = nearbySearchResponse.NextPageToken;
            return NearbyPlacesStatus.ContinueNextPage;
        }
        queryLocation.nextPageToken = null;
        return NearbyPlacesStatus.Completed;
    }

    //TASK
    private void ProcessNearbySearchResponse(GGoogleMapsQueryLocation queryLocation, NearbySearchResponse nearbySearchResponse)
    {
        queryLocation.AddPOIsWithNearbySearchResponse(nearbySearchResponse);
        Cache.PopulatePOIsWithQueryLocation(queryLocation);
    }

    //TASK
    private async Task<NearbyPlacesStatus> DelayRetryOrFail(string urlParams, int retryCount)
    {
        if (retryCount <= GameSettings.WebRequestRetryMax)
        {
            //If within the max retries limit, make request again
            await Task.Delay((int)((GameSettings.GoogleNearbyPlacesNextPageRequestDelay + retryCount * GameSettings.WebRequestFailLinearDelay) * 1000));
            return NearbyPlacesStatus.DelaySuccessful;
        }
        Debug.LogError($"NearbyPlaces failed to retrieve!!! URL: {urlParams}");
        return NearbyPlacesStatus.Failed;
    }

    //TASK
    private string CreateNearbyPlacesNextPageRequestURL(string nextPageToken)
    {
        string urlParams = NearbyPlacesNextPageURLParameters + nextPageToken;
        Debug.Log($"TASK> NearbySearch_NextPage: {urlParams}");
        return urlParams;
    }

    //private IEnumerator RequestNearbyPlaces(string url, int retryCount = 0)
    //{
    //    UnityWebRequest req = UnityWebRequest.Get(url);
    //    yield return req.SendWebRequest();

    //    if (req.result != UnityWebRequest.Result.Success)
    //    {
    //        //Network Error
    //        Debug.Log($"Network Error: {req.result} (URL: {url})");
    //        NearbyPlacesRetry(url, retryCount);
    //        yield break;
    //    }
    //    string res = req.downloadHandler.text;
    //    GGoogleMapsResponses.NearbySearchResponse nearbySearchResponse = JsonConvert.DeserializeObject<GGoogleMapsResponses.NearbySearchResponse>(res);
    //    string status = nearbySearchResponse.Status;
    //    if (status != "OK")
    //    {
    //        Debug.LogWarning($"NearbyPlaces returned Status:{status} for URL: {url}, Retry Count:{retryCount}");
    //        NearbyPlacesRetry(url, retryCount);
    //        yield break;
    //    }

    //    Debug.Log($"NearbyPlaces Success with Status OK (URL: {url}) - {res}");

    //    Cache.PopulateWithNearbySearchResponse(nearbySearchResponse);

    //    //Next page
    //    if (nearbySearchResponse.NextPageToken != null)
    //    {
    //        MakeNearbyPlacesNextPageRequest(nearbySearchResponse.NextPageToken);
    //    }
    //}

    //private void NearbyPlacesRetry(string url, int retryCount)
    //{
    //    if (retryCount <= GameSettings.WebRequestRetryMax)
    //    {
    //        //If within the max retries limit, make request again
    //        G.StartCoroutine(RequestNearbyPlacesDelayed(url, retryCount + 1));
    //        return;
    //    }
    //    Debug.LogError($"NearbyPlaces failed to retrieve!!! URL: {url}");
    //}


    #endregion



    public void MakeRequest(string url)
    {
        G.StartCoroutine(Request(url));
    }

    private IEnumerator Request(string url)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"Response: {req.downloadHandler.text}");
        }
        else
        {
            Debug.Log($"Error: {req.result} (From: {url})");
        }
    }

    public void MakeImageRequest(string url)
    {
        G.StartCoroutine(ImageRequest(url));
    }

    private IEnumerator ImageRequest(string url)
    {
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        yield return req.SendWebRequest();

        bool error = false;
        if (req.result == UnityWebRequest.Result.Success)
        {
            Texture myTexture = DownloadHandlerTexture.GetContent(req);
            if (myTexture == null)
            {
                error = true;
            }
            else
            {
                //TODO
            }
        }
        else
        {
            error = true;
        }

        if (error)
        {
            Debug.Log($"(Image) Error: {req.result} (From: {url})");
        }

    }

}
