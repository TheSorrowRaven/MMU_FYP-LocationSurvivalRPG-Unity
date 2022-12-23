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
    private static POIManager POIManager => POIManager.Instance;

    [SerializeField] private GameObject UIPOIObject;

    [SerializeField] private RawImage POIPhotoRawImage;
    [SerializeField] private RectTransform POIPhotoRT;
    [SerializeField] private Texture PhotoNotFoundTexture;
    [SerializeField] private Texture PhotoLoadingTexture;

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
        if (photos == null || photos.Count == 0)
        {
            SetPhotoUnkown();
            return;
        }
        SetPhotoIsLoading();
        GGoogleMapsService.StartPlacePhotosQuery(photos[0].PhotoReference, (int)POIPhotoRT.rect.size.x, (int)POIPhotoRT.rect.size.y, PhotoFetched, SetPhotoUnkown);
    }

    private void PhotoFetched(Texture texture)
    {
        POIPhotoRawImage.texture = texture;
        CorrectPhotoUV();
    }
    private void SetPhotoUnkown()
    {
        POIPhotoRawImage.texture = PhotoNotFoundTexture;
        ResetPhotoUV();
    }
    private void SetPhotoIsLoading()
    {
        POIPhotoRawImage.texture = PhotoLoadingTexture;
        ResetPhotoUV();
    }

    private void ResetPhotoUV()
    {
        POIPhotoRawImage.uvRect = new(0, 0, 1, 1);
    }

    [ContextMenu("Correct Photo UV")]
    public void CorrectPhotoUV()
    {
        Texture texture = POIPhotoRawImage.texture;
        Vector2 rawImageSize = POIPhotoRT.rect.size;
        float rawImageAR = rawImageSize.x / rawImageSize.y;
        float photoAR = (float)texture.width / texture.height;
        Rect uvRect;
        if (photoAR > rawImageAR)
        {
            // Image is wider than the rectangle, so it will be cropped horizontally
            float uvWidth = rawImageAR / photoAR;
            float uvX = (1 - uvWidth) / 2;

            // Set the UV rect
            uvRect = new Rect(uvX, 0, uvWidth, 1);
        }
        else if (photoAR < rawImageAR)
        {
            // Image is taller than the rectangle, so it will be cropped vertically
            float uvHeight = photoAR / rawImageAR;
            float uvY = (1 - uvHeight) / 2;

            // Set the UV rect
            uvRect = new Rect(0, uvY, 1, uvHeight);
        }
        else
        {
            // Image and rectangle have the same aspect ratio, so no cropping is needed
            uvRect = new Rect(0, 0, 1, 1);
        }
        POIPhotoRawImage.uvRect = uvRect;
    }



    public void CalculateLoot()
    {
        CalculateLootFromTypes();
    }

    private void CalculateLootFromTypes()
    {
        List<string> types = gPOI.Types;
        if (types.Count == 0)
        {
            return;
        }
        for (int i = 0; i < types.Count; i++)
        {
            string type = types[i];
            if (POIManager.TryGetPOITypeDefinition(type, out POITypeDefinition definition))
            {
                CalculateLootFromFood(definition.Food);
                CalculateLootFromMedical(definition.Medical);
            }
        }

    }

    private void CalculateLootFromFood(int food)
    {
        if (food == 0)
        {
            return;
        }
    }

    private void CalculateLootFromMedical(int medical)
    {
        if (medical == 0)
        {
            return;
        }
    }








    public void ButtonClickExitUIPOI()
    {
        UIPOIObject.SetActive(false);
    }

}
