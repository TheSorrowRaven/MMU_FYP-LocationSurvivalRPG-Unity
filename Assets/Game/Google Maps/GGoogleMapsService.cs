using Mapbox.Utils;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

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
        BuildLinks();
        Cache = new();
    }

    private void BuildLinks()
    {
        BuildPlaceDetailsLink();
        BuildNearbyPlacesLink();
    }


    #region Place Details
    //Place Details
    //https://developers.google.com/maps/documentation/places/web-service/details

    [System.NonSerialized] private readonly string[] PlaceDetailsAttributes = new string[]
    {
        "photos", "reviews", "url", "address_components"
    };
    private string PlaceDetailsLink;

    private void BuildPlaceDetailsLink()
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

    private string NearbyPlacesLink;
    private string NearbyPlacesNextPageLink;

    private void BuildNearbyPlacesLink()
    {
        NearbyPlacesLink = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?key={APIKey}&radius=";
        NearbyPlacesNextPageLink = $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?key={APIKey}&pagetoken=";
    }

    public void MakeNearbyPlacesRequest(double latitude, double longitude)
    {
        string url = NearbyPlacesLink + $"{GameSettings.POIRadius}&location={latitude}%2C{longitude}";
        Debug.Log($"> NearbySearch: {url}");
        G.StartCoroutine(RequestNearbyPlaces(url));
    }

    private void MakeNearbyPlacesNextPageRequest(string nextPageToken)
    {
        string url = NearbyPlacesNextPageLink + nextPageToken;
        Debug.Log($"> NearbySearch_NextPage: {url}");
        G.StartCoroutine(RequestNearbyPlacesDelayed(url));
    }

    private IEnumerator RequestNearbyPlacesDelayed(string url, int retryCount = 0)
    {
        yield return new WaitForSecondsRealtime(GameSettings.GoogleNearbyPlacesNextPageRequestDelay + retryCount * GameSettings.WebRequestFailLinearDelay);
        yield return RequestNearbyPlaces(url, retryCount);
    }

    private IEnumerator RequestNearbyPlaces(string url, int retryCount = 0)
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            //Network Error
            Debug.Log($"Network Error: {req.result} (URL: {url})");
            NearbyPlacesRetry(url, retryCount);
            yield break;
        }
        string res = req.downloadHandler.text;
        GGoogleMapsResponses.NearbySearchResponse nearbySearchResponse = JsonConvert.DeserializeObject<GGoogleMapsResponses.NearbySearchResponse>(res);
        string status = nearbySearchResponse.Status;
        if (status != "OK")
        {
            Debug.LogWarning($"NearbyPlaces returned Status:{status} for URL: {url}, Retry Count:{retryCount}");
            NearbyPlacesRetry(url, retryCount);
            yield break;
        }

        Debug.Log($"NearbyPlaces Success with Status OK (URL: {url}) - {res}");

        Cache.PopulateWithNearbySearchResponse(nearbySearchResponse);

        //Next page
        if (nearbySearchResponse.NextPageToken != null)
        {
            MakeNearbyPlacesNextPageRequest(nearbySearchResponse.NextPageToken);
        }
    }

    private void NearbyPlacesRetry(string url, int retryCount)
    {
        if (retryCount <= GameSettings.WebRequestRetryMax)
        {
            //If within the max retries limit, make request again
            G.StartCoroutine(RequestNearbyPlacesDelayed(url, retryCount + 1));
            return;
        }
        Debug.LogError($"NearbyPlaces failed to retrieve!!! URL: {url}");
    }


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
