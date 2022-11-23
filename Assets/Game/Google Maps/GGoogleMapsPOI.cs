using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Mapbox.Utils;
using Unity.VisualScripting;

[Serializable]
public class GGoogleMapsPOI
{

    #region Classes
    [Serializable]
    public class AddressComponent
    {
        [JsonProperty("long_name")]
        public string LongName { get; set; }

        [JsonProperty("short_name")]
        public string ShortName { get; set; }

        [JsonProperty("types")]
        public List<string> Types { get; set; }
    }

    [Serializable]
    public class Bounds
    {
        [Serializable]
        public class Coord
        {
            [JsonProperty("lat")]
            public double Latitude { get; set; }
            [JsonProperty("lng")]
            public double Longitude { get; set; }

            public static implicit operator Vector2d(Coord coord)
            {
                return new(coord.Latitude, coord.Longitude);
            }
        }
        [Serializable]
        public class CoordRect
        {
            [JsonProperty("northeast")]
            public Coord Northeast { get; set; }
            [JsonProperty("southwest")]
            public Coord Southeast { get; set; }
        }

        [JsonProperty("location")]
        public Coord Location { get; set; }

        [JsonProperty("viewport")]
        public CoordRect Viewport { get; set; }

    }

    [Serializable]
    public class OpenHours
    {
        [Serializable]
        public class Period
        {
            [Serializable]
            public class DayTime
            {
                [JsonProperty("day")]
                public int Day { get; set; }
                [JsonProperty("time")]  //"0900"
                public string Time { get; set; }
            }

            [JsonProperty("Open")]
            public DayTime Open { get; set; }

            [JsonProperty("close")]
            public DayTime Close { get; set; }

        }

        [JsonProperty("open_now")]
        public bool OpenNow { get; set; }

        [JsonProperty("periods")]
        public List<Period> Periods { get; set; }

        [JsonProperty("weekday_text")]
        public List<string> WeekdayText { get; set; }

    }

    [Serializable]
    public class Photo
    {
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("photo_reference")]
        public string PhotoReference { get; set; }

        [JsonProperty("html_attributions")]
        public List<string> HTMLAttributions { get; set; }
    }

    [Serializable]
    public class PlusCodes
    {
        [JsonProperty("compound_code")]
        public string CompoundCode { get; set; }

        [JsonProperty("global_code")]
        public string GlobalCode { get; set; }

    }

    [Serializable]
    public class Review
    {
        [JsonProperty("rating")]
        public int Rating { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("author_name")]
        public string Author { get; set; }
        [JsonProperty("author_url")]
        public string AuthorURL { get; set; }
        [JsonProperty("language")]  //"en"
        public string Language { get; set; }
        [JsonProperty("original_language")]  //"en"
        public string OriginalLanguage { get; set; }
        [JsonProperty("profile_photo_url")]
        public string ProfilePhotoURL { get; set; }
        [JsonProperty("relative_time_description")]  //"6 months ago"
        public string RelativeTimeDescription { get; set; }
        [JsonProperty("time")]
        public long Time { get; set; }
        [JsonProperty("translated")]
        public bool Translated { get; set; }

    }

    #endregion

    public bool IsDetailed = false;

    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("place_id")]
    public string PlaceID { get; set; }

    [JsonProperty("geometry")]
    public Bounds Geometry { get; set; }

    [JsonProperty("types")]
    public List<string> Types { get; set; }

    [JsonProperty("photos")]
    public List<Photo> Photos { get; set; }

    [JsonProperty("rating")]
    public decimal? Rating { get; set; }
    [JsonProperty("user_ratings_total")]
    public int? TotalUserRatings { get; set; }
    [JsonProperty("reviews")]
    public List<Review> Reviews { get; set; }



    [JsonProperty("url")]   //Google URL
    public string URL { get; set; }

    [JsonProperty("address_components")]
    public List<AddressComponent> AddressComponents { get; set; }


    //Below are properties requestable but currently ignored (There are more in https://developers.google.com/maps/documentation/places/web-service/details)
    
    //[JsonProperty("formatted_address")]
    //public string FullAddress { get; set; }

    //[JsonProperty("website")]   //POI's website
    //public string WebsiteURL { get; set; }

    //[JsonProperty("business_status")]
    //public string BusinessStatus { get; set; }

    //[JsonProperty("reference")]
    //public string Reference { get; set; }

    //[JsonProperty("adr_address")]
    //public string RawAddress { get; set; }

    //[JsonProperty("formatted_phone_number")]
    //public string PhoneNumber { get; set; }

    //[JsonProperty("international_phone_number")]
    //public string InternationalPhoneNumber { get; set; }

    //[JsonProperty("icon")]
    //public string IconURL { get; set; }

    //[JsonProperty("icon_background_color")] //"#7B9EB0"
    //public string IconBackgroundColor { get; set; }

    //[JsonProperty("icon_mask_base_uri")]
    //public string IconMaskBaseURL { get; set; }

    //[JsonProperty("opening_hours")]
    //public OpenHours OpeningHours { get; set; }



    //[JsonProperty("plus_code")]
    //public PlusCodes PluseCode { get; set; }



    //[JsonProperty("utc_offset")]
    //public int UtcOffset { get; set; }

    //[JsonProperty("vicinity")]
    //public string Vicinity { get; set; }

}
