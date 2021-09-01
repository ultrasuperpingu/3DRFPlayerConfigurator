using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SendRFLink : MonoBehaviour
{
	public RFPlayerConnection rfplayer;
	public TMPro.TMP_InputField id;
	public TMPro.TMP_InputField command;
	public TMPro.TMP_InputField pulseTime;
	private void Start()
	{
		GetComponent<Button>().onClick.AddListener(() =>
		{
			int trameLength = 22 + 49 + 2;
			//trameLength -= 5;
			byte[] bin = new byte[trameLength];
			int currentIndex = 0;
			bin[currentIndex++] = (byte)'Z';
			bin[currentIndex++] = (byte)'I';
			bin[currentIndex++] = 1;
			ushort len = (ushort)trameLength;
			bin[currentIndex++] = (byte)(len & 0xFF);
			bin[currentIndex++] = (byte)((len >> 8) & 0xFF);
			// Frame type
			bin[currentIndex++] = 1;
			//test
			//cluster
			//bin[currentIndex++] = 0;
			//dataflag
			//bin[currentIndex++] = 0;
			//frequency
			//bin[currentIndex++] = 0;
			//Array.Copy(BitConverter.GetBytes(868350), 0, bin, currentIndex, 4);
			Array.Copy(BitConverter.GetBytes(43920), 0, bin, currentIndex, 4);
			currentIndex += 4;
			//RFLevel
			bin[currentIndex++] = (byte)0;
			//floorNoise
			bin[currentIndex++] = (byte)0;
			//pulseElementSize
			bin[currentIndex++] = 1;
			//number
			Array.Copy(BitConverter.GetBytes(49), 0, bin, currentIndex, 2);
			currentIndex += 2;
			//repeat
			bin[currentIndex++] = 0;
			//delay
			bin[currentIndex++] = 0;
			//multiply
			byte multiply = 40;
			bin[currentIndex++] = multiply;
			//time
			Array.Copy(BitConverter.GetBytes(0), 0, bin, currentIndex, 4);
			currentIndex += 4;
			int vid=int.Parse(id.text);
			int vpulseTime = int.Parse(pulseTime.text);
			currentIndex += 1; // first 0
			for (int i = 0; i < 20; i++)
			{
				if (((vid >> (19 - i)) & 1) == 1)
				{
					bin[currentIndex + 2 * i] = (byte)(vpulseTime / multiply * 3);
					bin[currentIndex + 2 * i + 1] = (byte)(vpulseTime / multiply);
				}
				else
				{
					bin[currentIndex + 2 * i] = (byte)(vpulseTime / multiply);
					bin[currentIndex + 2 * i + 1] = (byte)(vpulseTime / multiply * 3);
				}
			}
			currentIndex += 40;
			int vcommand = int.Parse(command.text);
			for (int i = 0; i < 4; i++)
			{
				if (((vcommand >> (3 - i)) & 1) == 1)
				{
					bin[currentIndex + 2 * i] = (byte)(vpulseTime / multiply * 3);
					bin[currentIndex + 2 * i + 1] = (byte)(vpulseTime / multiply);
				}
				else
				{
					bin[currentIndex + 2 * i] = (byte)(vpulseTime / multiply);
					bin[currentIndex + 2 * i + 1] = (byte)(vpulseTime / multiply * 3);
				}
			}
			currentIndex += 8;
			bin[currentIndex] = (byte)(vpulseTime / multiply);
			RFPMessage test = new RFPMessage(RFPMessage.MessageType.BINARY, bin, 0, bin.Length);
			Debug.Log(test.ToString());
			rfplayer.SendBinaryCommand(bin);
		});
	}

}
