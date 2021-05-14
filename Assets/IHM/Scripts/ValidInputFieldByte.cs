using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class ValidInputFieldByte : MonoBehaviour
{
	TMP_InputField field;
	string oldVal = "";
	private void Awake()
	{
		field = GetComponent<TMP_InputField>();
		field.onEndEdit.AddListener(s => {
			byte b;
			if (!string.IsNullOrWhiteSpace(s) && !byte.TryParse(s, out b))
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
