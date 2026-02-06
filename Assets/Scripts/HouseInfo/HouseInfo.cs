using UnityEngine;

public class HouseInfo : MonoBehaviour
{
    [TextArea]
    public string houseInfo;

    public string GetHouseInfo()
    {
        return houseInfo;
    }
}
