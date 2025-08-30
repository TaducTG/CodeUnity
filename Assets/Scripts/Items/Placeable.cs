using UnityEngine;
[CreateAssetMenu(fileName = "New Placeable", menuName = "Items/Placeable")]
public class Placeable : Items
{
    [Header("Light Settings")]
    public bool emitsLight;             // Có phát sáng không
    public Color lightColor = Color.white;
    public float lightIntensity = 1f;   // độ sáng
    public float lightRadius = 3f;      // bán kính sáng
    public bool scaleWithDayNight = true; // có muốn ánh sáng thay đổi theo ngày/đêm
}
