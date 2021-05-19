using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SendCommand : MonoBehaviour
{
	public RFPlayerConnection rfplayer;
	public TMPro.TMP_Dropdown command;
	public TMPro.TMP_Dropdown protocol;
	public TMPro.TMP_InputField id;
	public TMPro.TMP_InputField dim;
	public TMPro.TMP_InputField burst;
	public TMPro.TMP_InputField qualifier;
	private void Start()
	{
		GetComponent<Button>().onClick.AddListener(() =>
		{
			var scommand = command.options[command.value].text + " ID " + id.text;
			scommand += " " + protocol.options[protocol.value].text;
			scommand += (string.IsNullOrWhiteSpace(dim.text) ? "" : " %" + dim.text);
			scommand += (string.IsNullOrWhiteSpace(burst.text) ? "" : " BURST" + burst.text);
			scommand += (string.IsNullOrWhiteSpace(qualifier.text) ? "" : " QUALIFIER" + qualifier.text);
			rfplayer.SendCommand(scommand);
		});
	}
}