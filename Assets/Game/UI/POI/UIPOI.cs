using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem.UI;
using Unity.VisualScripting;
using UnityEngine.UI;

public class UIPOI : MonoBehaviour
{
    private static UIPOI instance;
    public static UIPOI Instance => instance;

    private static GGoogleMapsService GGoogleMapsService => GGoogleMapsService.Instance;

    [SerializeField] private GameObject UIPOIObject;

    [SerializeField] private RawImage POIPhotoRawImage;
    [SerializeField] private TextMeshProUGUI POIName;
    [SerializeField] private TextMeshProUGUI POITypes;

    [System.NonSerialized] private GGoogleMapsPOI gPOI;

    private void Awake()
    {
        instance = this;
    }

    public void SetGPOI(GGoogleMapsPOI gPOI)
    {
        this.gPOI = gPOI;

        POIName.SetText(gPOI.Name);
        POITypes.SetText(BuildTypeTextFromGPOI(gPOI));
        RequestGPOIImage(gPOI);

        UIPOIObject.SetActive(true);
    }

    private string BuildTypeTextFromGPOI(GGoogleMapsPOI gPOI)
    {
        List<string> types = gPOI.Types;
        if (types.Count == 0)
        {
            return "";
        }
        string text = FirstCharacterToUpper(types[0]);
        for (int i = 1; i < types.Count; i++)
        {
            string typeName = types[i];
            if (typeName.Length == 0)
            {
                continue;
            }
            text += ", " + FirstCharacterToUpper(typeName);
        }
        return text;
    }

    private string FirstCharacterToUpper(string text)
    {
        if (text == null)
        {
            return null;
        }
        if (text.Length == 0)
        {
            return text;
        }
        return char.ToUpper(text[0]) + text[1..];
    }

    public void RequestGPOIImage(GGoogleMapsPOI gPOI)
    {
        List<GGoogleMapsPOI.Photo> photos = gPOI.Photos;
        if (photos == null)
        {
            Debug.Log("Photos Null");
            return;
        }
        if (photos.Count == 0)
        {
            Debug.Log("Photos Empty");
            return;
        }
        GGoogleMapsService.StartPlacePhotosQuery(photos[0], PhotoFetchedAction);
    }

    private void PhotoFetchedAction(Texture texture)
    {
        POIPhotoRawImage.texture = texture;
    }

    public void ButtonClickExitUIPOI()
    {
        UIPOIObject.SetActive(false);
    }

}
