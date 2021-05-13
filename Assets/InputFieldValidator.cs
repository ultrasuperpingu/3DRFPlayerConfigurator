using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldValidator : MonoBehaviour
{
	public string regex;
	private Regex r;
	string lastValidField;
	private TMP_InputField input;
	public UnityEvent<string> onEndEdit;
	private void Start()
	{
		r = new Regex(regex);
		input = GetComponent<TMP_InputField>();
		lastValidField = input.text;
		input.onEndEdit.AddListener((s) =>
		{
			if(!r.IsMatch(input.text))
			{
				input.text = lastValidField;
			}
			else
			{
				lastValidField = input.text;
				onEndEdit.Invoke(lastValidField);
			}
		});
	}
}
