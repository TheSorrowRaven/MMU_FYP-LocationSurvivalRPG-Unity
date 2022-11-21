using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GGoogleMapsService
{
    public static readonly GGoogleMapsService Instance = new();

    private static G G => G.Instance;

    private const string APIKey = "AIzaSyBb9FmWLtnQwQu2IAfvVsSOUkqadHZTeMk";

    private GGoogleMapsService()
    {
        BuildLinks();
    }

    private void BuildLinks()
    {
        BuildPlaceDetailsLink();
    }


    #region Place Details
    //Place Details
    //https://developers.google.com/maps/documentation/places/web-service/details

    [System.NonSerialized] private readonly string[] PlaceDetailsAttributes = new string[]
    {
        "name", "geometry", "types", "photos", "rating", "user_ratings_total", "reviews", "formatted_address", "url", "address_components"
    };
    private string PlaceDetailsLink;

    private void BuildPlaceDetailsLink()
    {
        PlaceDetailsLink = "https://maps.googleapis.com/maps/api/place/details/json?fields=";
        int i = 0;
        Debug.Log(PlaceDetailsAttributes);
        for (; i < PlaceDetailsAttributes.Length - 1; i++)
        {
            PlaceDetailsLink += PlaceDetailsAttributes[i] + "%2C";
        }
        PlaceDetailsLink += PlaceDetailsAttributes[i] + "&place_id=";
    }

    public void MakePlaceDetailsRequest(string placeID)
    {
        string url = PlaceDetailsLink + $"{placeID}&key={APIKey}";
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


    public void Set()
    {
        Debug.Log("Making Request...");
        MakeRequest("https://catfact.ninja/fact");  //API test
    }


}
