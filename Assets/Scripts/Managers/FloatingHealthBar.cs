using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    // Update is called once per frame
    public void Update(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
    }
}
