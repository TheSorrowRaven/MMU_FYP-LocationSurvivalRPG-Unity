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
    [JsonProperty("weapon")]
    [DefaultValue(0)]
    public int Weapon { get; set; }



}
