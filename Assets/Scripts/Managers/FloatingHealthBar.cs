using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Camera camera;

    public void SliderUpdate(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
    }

    private void LateUpdate()
    {
        if (camera == null) return;
        transform.LookAt(transform.position + camera.transform.forward);
    }
}
