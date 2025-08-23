using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightLighting : MonoBehaviour
{
    public Light2D globalLight;
    public DayNightCycle cycle;

    [Header("Thiết lập ánh sáng")]
    public float minIntensity = 0.2f;
    public float maxIntensity = 1.5f;

    [Tooltip("Giờ mặt trời mọc (0-24)")]
    public float sunriseHour = 6f;
    [Tooltip("Giờ mặt trời lặn (0-24)")]
    public float sunsetHour = 18f;

    void Update()
    {
        float currentHour = cycle.timeOfDay;

        // Tính tỉ lệ sáng dựa trên thời gian
        float targetIntensity;

        if (currentHour < sunriseHour) // Nửa đêm -> bình minh
        {
            float t = Mathf.InverseLerp(sunriseHour - 2f, sunriseHour, currentHour); // 2 giờ chuyển sáng
            targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, Mathf.Clamp01(t));
        }
        else if (currentHour < sunsetHour) // Ban ngày
        {
            targetIntensity = maxIntensity;
        }
        else // Hoàng hôn -> Nửa đêm
        {
            float t = Mathf.InverseLerp(sunsetHour, sunsetHour + 2f, currentHour); // 2 giờ chuyển tối
            targetIntensity = Mathf.Lerp(maxIntensity, minIntensity, Mathf.Clamp01(t));
        }

        globalLight.intensity = targetIntensity;
    }
}
