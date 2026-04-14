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
    private void LateUpdate()
    {
        // Make the health bar face the camera
        transform.LookAt(transform.position + camera.transform.forward);
    }
}
