using UnityEngine;

public class OwnerProfile : MonoBehaviour
{
    [Header("Identity")]
    public string ownerName;

    [Header("Stats")]
    [Min(0)] public int wealth = 0; // "money"
    [Header("Houses owned by this person (drag all here)")]
    public House[] houses;

    public int HouseCount => houses != null ? houses.Length : 0;

    // Bank rule: rich vs poor
    // Rich = wealth >= houses
    public bool IsRich() => wealth >= HouseCount;

    // City Registry: "too many houses for their money"
    public bool Overextended() => wealth < HouseCount;
}
