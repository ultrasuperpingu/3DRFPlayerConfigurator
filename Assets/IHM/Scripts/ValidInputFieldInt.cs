using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class ValidInputFieldInt: MonoBehaviour
{
	TMP_InputField field;
	string oldVal = "";
	private void Awake()
	{
		field = GetComponent<TMP_InputField>();
		field.onEndEdit.AddListener(s => {
			int b;
			if (!string.IsNullOrWhiteSpace(s) && !int.TryParse(s, out b))
			{
				oldVal = s;
			}
			else
			{
				s = oldVal;
			}
		});
	}
}
