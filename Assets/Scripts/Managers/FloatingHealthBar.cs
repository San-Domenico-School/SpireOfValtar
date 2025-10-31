using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Camera camera;

    // Update is called once per frame
    public void SliderUpdate(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
    }
}
