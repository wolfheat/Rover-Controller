using System;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public static SliderController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ReadSpeed() => RoverController.Instance.SetSpeed((int)(slider.value * 10));

    internal void SetDutyValue(int duty) => slider.SetValueWithoutNotify(duty / 10f);
}
