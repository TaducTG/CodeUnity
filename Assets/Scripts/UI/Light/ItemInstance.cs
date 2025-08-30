using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ItemInstance : MonoBehaviour
{
    private Light2D light2D;
    public Placeable data;
    private float baseIntensity;

    void Awake()
    {
        data = (Placeable)gameObject.GetComponent<PickUpItem>().itemData;

        if (data != null)
        {
            Setup(data); // setup ngay khi object được tạo ra
        }
    }
    public void Setup(Placeable itemData)
    {
        data = itemData;

        if (data.emitsLight)
        {
            light2D = gameObject.AddComponent<Light2D>();
            light2D.lightType = Light2D.LightType.Point;
            light2D.color = data.lightColor;
            light2D.intensity = data.lightIntensity;
            light2D.pointLightOuterRadius = data.lightRadius;

            baseIntensity = data.lightIntensity;
        }
    }

    void Update()
    {
        if (data != null && data.emitsLight && data.scaleWithDayNight)
        {
            // Lấy tỉ lệ sáng từ globalLight (DayNightLighting đã set sẵn)
            float envFactor = FindObjectOfType<DayNightLighting>().globalLight.intensity;

            // Ban ngày (envFactor cao) thì giảm sáng, ban đêm (thấp) thì sáng mạnh hơn
            light2D.intensity = baseIntensity * (2f - envFactor);
        }
    }
}
