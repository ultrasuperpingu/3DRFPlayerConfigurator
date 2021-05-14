using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ID2X10PseudoAddressSync : MonoBehaviour
{
	public TMP_InputField ID;
	public TMP_Dropdown letterDropDown;
	public TMP_Dropdown numberDropDown;
	public string oldVal;
	private void Start()
	{
		ID.onEndEdit.AddListener(s =>
		{
			byte b;
			if (!string.IsNullOrWhiteSpace(s) && byte.TryParse(s, out b))
			{
				oldVal = s;
				numberDropDown.SetValueWithoutNotify(b % 16);
				letterDropDown.SetValueWithoutNotify(b / 16);
			}
			else
			{
				ID.SetTextWithoutNotify(oldVal);
			}
		});
		numberDropDown.onValueChanged.AddListener(v =>
		{
			ID.SetTextWithoutNotify("" + (numberDropDown.value + letterDropDown.value * 16));
		});
		letterDropDown.onValueChanged.AddListener(v =>
		{
			ID.SetTextWithoutNotify("" + (numberDropDown.value + letterDropDown.value * 16));
		});
	}
}
