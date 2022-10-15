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
    }
    
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

    public void Set()
    {
        
    }


}
