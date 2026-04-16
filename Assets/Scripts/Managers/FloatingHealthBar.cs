using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Camera camera;

    private void Awake()
    {
        if (camera == null)
            camera = Camera.main;
    }

    public void SliderUpdate(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
    }

    private void LateUpdate()
    {
        // Re-grab camera if it went missing (e.g. scene change)
        if (camera == null)
            camera = Camera.main;

        if (camera == null) return;

        transform.LookAt(transform.position + camera.transform.forward);
    }
}
