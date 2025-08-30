using UnityEngine;
using UnityEngine.Events;

public class DayNightCycle : MonoBehaviour
{
    public float dayLengthInMinutes = 10f; // 1 ngày game = 10 phút thật
    public float timeOfDay = 6f; // Bắt đầu lúc 6h sáng
    public UnityEvent onSunrise;
    public UnityEvent onSunset;

    void Update()
    {
        // Tăng thời gian
        timeOfDay += (24f / (dayLengthInMinutes * 60f)) * Time.deltaTime;

        // Reset về 0 khi qua ngày
        if (timeOfDay >= 24f) timeOfDay -= 24f;

        // Trigger sự kiện (ví dụ)
        if (Mathf.Abs(timeOfDay - 6f) < 0.01f) onSunrise.Invoke();
        if (Mathf.Abs(timeOfDay - 18f) < 0.01f) onSunset.Invoke();
    }
}
