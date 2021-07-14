using System;
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
	public bool sendBinary = false;
	private void Start()
	{
		GetComponent<Button>().onClick.AddListener(() =>
		{
			if (!sendBinary)
			{
				var scommand = command.options[command.value].text + " ID " + id.text;
				scommand += " " + protocol.options[protocol.value].text;
				scommand += (string.IsNullOrWhiteSpace(dim.text) ? "" : " %" + dim.text);
				scommand += (string.IsNullOrWhiteSpace(burst.text) ? "" : " BURST" + burst.text);
				scommand += (string.IsNullOrWhiteSpace(qualifier.text) ? "" : " QUALIFIER" + qualifier.text);
				rfplayer.SendCommand(scommand);
			}
			else
			{
				byte[] bcommand = new byte[17];
				bcommand[0] = (byte)'Z';
				bcommand[1] = (byte)'I';
				bcommand[2] = 1;
				ushort len = 12;
				bcommand[3] = (byte)(len & 0xFF);
				bcommand[4] = (byte)((len >> 8) & 0xFF);
				// Frame type
				bcommand[5] = 0;
				// Cluster
				bcommand[6] = 0;
				// Protocol
				var textProtocol = protocol.options[protocol.value].text.Replace(" ", "_");

				bcommand[7] = GetProtocol(textProtocol);
				// action
				bcommand[8] = (byte)Enum.Parse(typeof(RFPMessage.Action), command.options[command.value].text.Replace(" ", "_"));
				// Device ID
				bcommand[9] = byte.Parse(id.text);
				bcommand[10] = 0;
				bcommand[11] = 0;
				bcommand[12] = 0;
				// Dim Value
				bcommand[13] = (string.IsNullOrWhiteSpace(dim.text) ? (byte)0 : byte.Parse(dim.text));
				// Burst
				bcommand[14] = (string.IsNullOrWhiteSpace(burst.text) ? (byte)0 : byte.Parse(burst.text));
				// Qualifier
				bcommand[15] = (string.IsNullOrWhiteSpace(qualifier.text) ? (byte)0 : byte.Parse(qualifier.text));
				// Reserved2
				bcommand[16] = 0;
				rfplayer.SendBinaryCommand(bcommand);
			}
		});
	}
	static Dictionary<string, byte> textToProtocol = new Dictionary<string, byte>();
	static SendCommand()
	{
		textToProtocol.Add("VISONIC433", (byte)RFPMessage.SendProtocol.VISONIC_433);
		textToProtocol.Add("VISONIC868", (byte)RFPMessage.SendProtocol.VISONIC_868);
		textToProtocol.Add("CHACON", (byte)RFPMessage.SendProtocol.CHACON_433);
		textToProtocol.Add("DOMIA", (byte)RFPMessage.SendProtocol.DOMIA_433);
		textToProtocol.Add("X10", (byte)RFPMessage.SendProtocol.X10_433);
		textToProtocol.Add("X2D433", (byte)RFPMessage.SendProtocol.X2D_433);
		textToProtocol.Add("X2D868", (byte)RFPMessage.SendProtocol.X2D_868);
		textToProtocol.Add("X2DSHUTTER", (byte)RFPMessage.SendProtocol.X2D_SHUTTER_868);
		textToProtocol.Add("X2DELEC", (byte)RFPMessage.SendProtocol.X2D_HA_ELEC_868);
		textToProtocol.Add("X2DGAS", (byte)RFPMessage.SendProtocol.X2D_HA_GAS_868);
		textToProtocol.Add("RTS", (byte)RFPMessage.SendProtocol.SOMFY_RTS_433);
		textToProtocol.Add("BLYSS", (byte)RFPMessage.SendProtocol.BLYSS_433);
		textToProtocol.Add("PARROT", (byte)RFPMessage.SendProtocol.PARROT_433_OR_868);
		textToProtocol.Add("KD101", (byte)RFPMessage.SendProtocol.KD101_433);
		textToProtocol.Add("FS20", (byte)RFPMessage.SendProtocol.FS20_868);
		textToProtocol.Add("EDISIO", (byte)RFPMessage.SendProtocol.EDISIO);
	}
	private static byte GetProtocol(string textProtocol)
	{
		if(textToProtocol.ContainsKey(textProtocol))
		{
			return textToProtocol[textProtocol];
		}
		return 0;
	}
}
