using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderValueSetter : MonoBehaviour
{
	public void SetValue(System.Int32 val)
	{
		GetComponent<Slider>().value = val;
	}
}
