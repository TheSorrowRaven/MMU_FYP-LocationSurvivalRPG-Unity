using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class POITypeDefinition
{

    [JsonProperty("ignored")]
    [DefaultValue(false)]
    public bool Ignored { get; set; }

    [JsonProperty("food")]
    [DefaultValue(0)]
    public int Food { get; set; }
    [JsonProperty("medical")]
    [DefaultValue(0)]
    public int Medical { get; set; }
    [JsonProperty("melee")]
    [DefaultValue(0)]
    public int Melee { get; set; }
    [JsonProperty("ranged")]
    [DefaultValue(0)]
    public int Ranged { get; set; }
    [JsonProperty("ammo")]
    [DefaultValue(0)]
    public int Ammo { get; set; }



}
