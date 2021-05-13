using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SliderValueDisplay : MonoBehaviour
{
	public Slider slider;
	private void Start()
	{
		slider.onValueChanged.AddListener(f => GetComponent<TextMeshProUGUI>().text = f.ToString("F0"));
		GetComponent<TextMeshProUGUI>().text = slider.value.ToString("F0");
	}
}
