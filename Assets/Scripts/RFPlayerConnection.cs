using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO.Ports;
using System.Management;
using System.Text;
using System;
using System.Xml;
using System.Linq;
using System.IO;

public class RFPlayerConnection : MonoBehaviour
{
	public UnityEvent onConnected;
	public UnityEvent onDisconnected;
	public UnityEvent<RFPMessage> onMessageReceived;
	public UnityEvent<string> onCommandSent;
	
	private SerialPort s_serial;
	private const int RECEIVE_BUFFER_SIZE = 8192;
	private const int UPDATE_FIRMWARE_PACKET_SIZE = 12000;
	private byte[] alreadyRead = new byte[RECEIVE_BUFFER_SIZE];
	private int nbRead = 0;
	private BoyerMoore ziSearch = new BoyerMoore(new byte[] { (byte)'Z', (byte)'I' });
	

	public bool DeviceConnected
	{
		get { return checkPortCoroutine == null && s_serial != null && s_serial.IsOpen; }
	}

	#region RFPlayer Parameters and Infos
	private string _Firmware;
	public string Firmware
	{
		get => _Firmware;
		protected set
		{
			if (_Firmware != value)
			{
				_Firmware = value;
				onFirmwareChanged.Invoke(_Firmware);
			}
		}
	}
	public UnityEvent<string> onFirmwareChanged;

	public enum FormatModeType : int
	{
		TEXT,
		JSON,
		XML,
		HEXA,
		HEXA_FIXED,
		BINARY,
		OFF
	}
	private FormatModeType _FormatMode;
	public FormatModeType FormatMode
	{
		get => _FormatMode;
		set
		{
			if (_FormatMode != value)
			{
				SendCommand("FORMAT " + value.ToString().Replace('_', ' '));
				_FormatMode = value;
				onFormatModeChanged.Invoke((int)value);
			}
		}
	}
	public int FormatModeInt
	{
		get => (int)_FormatMode;
		set => FormatMode = (FormatModeType)value;
	}
	public UnityEvent<int> onFormatModeChanged;

	public enum FrequencyLType : int
	{
		F433420,
		F433920,
		OFF
	}
	private FrequencyLType _FrequencyL;
	public FrequencyLType FrequencyL
	{
		get => _FrequencyL;
		set
		{
			if (_FrequencyL != value)
			{
				if (!_updating)
					SendCommand("FREQ L " + (value.ToString().StartsWith("F")?value.ToString().Replace("F", ""):"0"));
				_FrequencyL = value;
				onFrequencyLChanged.Invoke((int)value);
			}
		}
	}

	public int FrequencyLInt
	{
		get => (int)_FrequencyL;
		set => FrequencyL = (FrequencyLType)value;
	}
	public UnityEvent<int> onFrequencyLChanged;

	public enum FrequencyHType : int
	{
		F868350,
		F868950,
		OFF
	}
	private FrequencyHType _FrequencyH;
	public FrequencyHType FrequencyH
	{
		get => _FrequencyH;
		set
		{
			if (_FrequencyH != value)
			{
				if (!_updating)
					SendCommand("FREQ H " + (value.ToString().StartsWith("F") ? value.ToString().Replace("F", "") : "0"));
				_FrequencyH = value;
				onFrequencyHChanged.Invoke((int)value);
			}
		}
	}
	public int FrequencyHInt
	{
		get => (int)_FrequencyH;
		set => FrequencyH = (FrequencyHType)value;
	}
	public UnityEvent<int> onFrequencyHChanged;
	private int _SelectivityL = 0;
	public int SelectivityL
	{
		get => _SelectivityL;
		set
		{
			if (value != _SelectivityL && value >= 0 && value <= 5)
			{
				if (!_updating)
					SendCommand("SELECTIVITY L " + value);
				_SelectivityL = value;
				onSelectivityLChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<int> onSelectivityLChanged;
	private int _SelectivityH = 0;
	public int SelectivityH
	{
		get => _SelectivityH;
		set
		{
			if (value != _SelectivityH && value >= 0 && value <= 5)
			{
				if (!_updating)
					SendCommand("SELECTIVITY H " + value);
				_SelectivityH = value;
				onSelectivityHChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<int> onSelectivityHChanged;
	private int _SensitivityL = 0;
	public int SensitivityL
	{
		get => _SensitivityL;
		set
		{
			if (value != _SensitivityL && value >= 0 && value <= 4)
			{
				if (!_updating)
					SendCommand("SENSITIVITY L " + value);
				_SensitivityL = value;
				onSensitivityLChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<int> onSensitivityLChanged;
	private int _SensitivityH = 0;
	public int SensitivityH
	{
		get => _SensitivityH;
		set
		{
			if (value != _SensitivityH && value >= 0 && value <= 4)
			{
				if (!_updating)
					SendCommand("SENSITIVITY H " + value);
				_SensitivityH = value;
				onSensitivityHChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<int> onSensitivityHChanged;
	private int _DSPTriggerL = 0;
	public int DSPTriggerL
	{
		get => _DSPTriggerL;
		set
		{
			if (value != _DSPTriggerL && value >= 0 && value <= 20)
			{
				_DSPTriggerL = value;
				if (!_updating)
					SendCommand("DSPTRIGGER L " + value);
				onDSPTriggerLChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<int> onDSPTriggerLChanged;
	private int _DSPTriggerH = 0;
	public int DSPTriggerH
	{
		get => _DSPTriggerH;
		set
		{
			if (value != _DSPTriggerH && value >= 0 && value <= 20)
			{
				_DSPTriggerH = value;
				if (!_updating)
					SendCommand("DSPTRIGGER H " + value);
				onDSPTriggerHChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<int> onDSPTriggerHChanged;
	private int _RFLinkTriggerL = 0;
	public int RFLinkTriggerL
	{
		get => _RFLinkTriggerL;
		set
		{
			if (value != _RFLinkTriggerL && value >= 0 && value <= 20)
			{
				_RFLinkTriggerL = value;
				if (!_updating)
					SendCommand("RFLINKTRIGGER L " + value);
				onRFLinkTriggerLChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<int> onRFLinkTriggerLChanged;
	private int _RFLinkTriggerH = 0;
	public int RFLinkTriggerH
	{
		get => _RFLinkTriggerH;
		set
		{
			if (value != _RFLinkTriggerH && value >= 0 && value <= 20)
			{
				_RFLinkTriggerH = value;
				if (!_updating)
					SendCommand("RFLINKTRIGGER H " + value);
				onRFLinkTriggerHChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<int> onRFLinkTriggerHChanged;
	private int _LBT = 0;
	public int LBT
	{
		get => _LBT;
		set
		{
			if (value != _LBT && value >= 0 && value <= 30)
			{
				_LBT = value;
				if (!_updating)
					SendCommand("LBT " + value);
				onLBTChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<int> onLBTChanged;
	private int _Jamming = 0;
	public int Jamming
	{
		get => _Jamming;
		set
		{
			if (value != _Jamming && value >= 0 && value <= 10)
			{
				_Jamming = value;
				if(!_updating)
					SendCommand("JAMMING " + value);
				onJammingChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<int> onJammingChanged;
	
	private string _MacAddress;
	public string MacAddress
	{
		get => _MacAddress;
		set
		{
			if (_MacAddress != value)
			{
				if (!_updating)
					SendCommand("SETMAC " + value);
				_MacAddress = value;
				onMACAddressChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<string> onMACAddressChanged;

	bool _LEDActivity = true;
	public bool LEDActivity
	{
		get => _LEDActivity;
		set
		{
			if (!_updating)
				SendCommand("LEDACTIVITY " + (value ? 1 : 0));
			_LEDActivity = value;
			onLEDActivityChanged.Invoke(value);
		}
	}
	public UnityEvent<bool> onLEDActivityChanged;

	bool _RFlinkEnable = true;
	public bool RFlinkEnable
	{
		get => _RFlinkEnable;
		set
		{
			if (value != _RFlinkEnable)
			{
				if (!_updating)
					SendCommand("FORMAT RFLINK " + (value ? "BINARY" : "OFF"));
				_RFlinkEnable = value;
				onRFLinkEnableChanged.Invoke(value);
			}
		}
	}
	public UnityEvent<bool> onRFLinkEnableChanged;

	private string[] transmitterProtocolsAvailable = new string[] {
		"VISONIC433", "VISONIC868", "CHACON", "DOMIA", "X10", "X2D433", "X2D868", "X2DSHUTTER",
		"X2DELEC", "X2DGAS", "RTS", "BLYSS", "PARROT", "KD101", "FS20", "EDISIO"
	};
	public string[] TransmitterProtocolsAvailable
	{
		get { return transmitterProtocolsAvailable; }
		protected set
		{
			transmitterProtocolsAvailable = value;
			//onTransmitterProtocolsAvailableChanged.Invoke(value);
		}
	}
	//public UnityEvent<string[]> onTransmitterProtocolsAvailableChanged;

	private string[] _RepeaterProtocolsAvailable = new string[] {
		"X10", "RTS", "VISONIC", "BLYSS", "CHACON", "OREGONV1", "OREGONV2", "OREGONV3/OWL",
		"DOMIA", "X2D", "KD101", "PARROT", "TIC", "FS20", "EDISIO"
	};
	public string[] RepeaterProtocolsAvailable
	{
		get { return _RepeaterProtocolsAvailable; }
		protected set
		{
			_RepeaterProtocolsAvailable = value;
			onRepeaterProtocolsAvailableChanged.Invoke(value);
		}
	}
	public UnityEvent<string[]> onRepeaterProtocolsAvailableChanged;
	
	private string[] _ReceiverProtocolsAvailable = new string[]{
		"X10", "RTS", "VISONIC", "BLYSS", "CHACON", "OREGONV1", "OREGONV2", "OREGONV3/OWL",
		"DOMIA", "X2D", "KD101", "PARROT", "TIC", "FS20", "JAMMING", "EDISIO"
	};
	public string[] ReceiverProtocolsAvailable
	{
		get { return _ReceiverProtocolsAvailable; }
		protected set
		{
			_ReceiverProtocolsAvailable = value;
			onReceiverProtocolsAvailableChanged.Invoke(value);
		}
	}
	public UnityEvent<string[]> onReceiverProtocolsAvailableChanged;

	private bool[] _RepeaterProtocolsEnabled = new bool[0];
	public bool[] RepeaterProtocolsEnabled
	{
		get { return _RepeaterProtocolsEnabled; }
		protected set
		{
			_RepeaterProtocolsEnabled = value;
			onRepeaterProtocolsEnabledChanged.Invoke(value);
		}
	}
	public UnityEvent<bool[]> onRepeaterProtocolsEnabledChanged;

	private bool[] _ReceiverProtocolsEnabled = new bool[0];
	public bool[] ReceiverProtocolsEnabled
	{
		get { return _ReceiverProtocolsEnabled; }
		protected set
		{
			_ReceiverProtocolsEnabled = value;
			onReceiverProtocolsEnabledChanged.Invoke(value);
		}
	}
	public UnityEvent<bool[]> onReceiverProtocolsEnabledChanged;

	public UnityEvent<float> onFirmwareUpdateProgress;
	public UnityEvent onFirmwareUpdateEnded;

	#endregion

	public void FactoryReset(bool all)
	{
		SendCommand("FACTORYRESET" + (all ? "ALL" : ""));
	}

	public void InitLB()
	{
		SendCommand("INITLB");
	}

	public void UpdateFirmware(string file)
	{
		if(File.Exists(file))
		{
			StartCoroutine(_UpdateFirmware(file));
		}
	}

	private IEnumerator _UpdateFirmware(string file)
	{
		_updating = true;
		var content = File.ReadAllBytes(file);
		int written = 0;
		s_serial.WriteTimeout = 1200;
		onFirmwareUpdateProgress.Invoke(0);
		for (int i = 0; i < content.Length; i += UPDATE_FIRMWARE_PACKET_SIZE)
		{
			var remaining = content.Length - written;
			var toWrite = Mathf.Min(UPDATE_FIRMWARE_PACKET_SIZE, remaining);
			try
			{
				// I guess we still need to read messages (not done in update since _updating == true) but not sure
				RFPMessage message;
				while((message = ReadMessage()) != null)
				while((message = ReadMessage()) != null)
					onMessageReceived.Invoke(message);
				s_serial.Write(content, written, toWrite);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				_updating = false;
				onFirmwareUpdateEnded.Invoke();
				s_serial.WriteTimeout = 200;
				yield break;
			}
			yield return null;
			written += toWrite;
			onFirmwareUpdateProgress.Invoke(written / (float)content.Length);
		}
		onFirmwareUpdateEnded.Invoke();
		_updating = false;
		s_serial.WriteTimeout = 200;
	}

	private void Start()
	{
		checkPortCoroutine = StartCoroutine(CheckPortsForDevice());
	}

	private void Update()
	{
		if (!_updating && DeviceConnected && s_serial.BytesToRead > 0)
		{
			var m = ReadMessage();
			if (m != null)
			{
				onMessageReceived.Invoke(m);
			}
		}
	}
	void OnApplicationQuit()
	{
		DisposeSerial();
	}

	private bool _isMessagePending = false;
	private RFPMessage toReemit;
	public void Reemit()
	{
		if(DeviceConnected)
		{
			Debug.Log(toReemit.BINARY);
			var copy = toReemit.BINARY.ToArray();
			//copy[1] = 0;
			s_serial.Write(copy,0, copy.Length);
		}
	}
	private RFPMessage ReadMessage()
	{
		//Debug.Log("toread: " + s_serial.BytesToRead);
		var nbReadNow = s_serial.Read(alreadyRead, nbRead, s_serial.BytesToRead);
		nbRead += nbReadNow;
		//var message3 = Encoding.ASCII.GetString(alreadyRead, 0, nbRead);
		//Debug.Log(message3);
		if (nbRead < 3)
			return null;
		if (alreadyRead[0] != 'Z' && alreadyRead[1] != 'I')
		{
			Debug.LogError("Message is not starting with ZI");
			var message = Encoding.ASCII.GetString(alreadyRead, 0, nbRead);
			Debug.LogError("ASCII: " + message);
			message = BitConverter.ToString(alreadyRead, 0, nbRead).Replace("-", "");
			Debug.LogError("HEXA: " + message);
			nbRead = 0;
			_isMessagePending = false;
			return null;
		}
		if (alreadyRead[2] >= 0x41) // ascii message
		{
			var message = Encoding.ASCII.GetString(alreadyRead, 0, nbRead);
			//Debug.Log(message);
			// TODO: find another message begining to see if there 2 messages in a row
			if (alreadyRead[nbRead - 1] == '\0' || alreadyRead[nbRead - 1] == '\r') //ascii message end
			{
				//onTextualMessageReceived.Invoke(message);
				//Debug.Log(message);
				var m = new RFPMessage(RFPMessage.MessageType.ASCII, alreadyRead, 0, nbRead);
				onMessageReceived.Invoke(m);
				nbRead = 0;
				_isMessagePending = false;
				return m;
			}
			// else message not complete
			else
			{
				Debug.Log("not finished " + message + "\r\nend:" + alreadyRead[nbRead - 1]);
			}
		}
		else if (alreadyRead[2] <= 0x0A) // binary message
		{
			int binarySize = alreadyRead[3] + alreadyRead[4] * 256;
			if (nbRead >= binarySize + 5) // message is complete
			{
				var m = new RFPMessage(RFPMessage.MessageType.BINARY, alreadyRead, 0, nbRead);
				if(m.IsRFLink)
				{
					toReemit = m;
				}
				var message = BitConverter.ToString(alreadyRead, 0, nbRead).Replace("-", "");
				Debug.Log("Binary received: size=" + nbRead + " read size: " + binarySize + " m:" + message);
				onMessageReceived.Invoke(m);
				//onTextualMessageReceived.Invoke(message);
				if (nbRead > binarySize + 5) // a second message arrived
				{
					// copy to the message to begining of the array
					Array.Copy(alreadyRead, binarySize + 5, alreadyRead, 0, nbRead - binarySize + 5);
					nbRead -= binarySize + 5;
					_isMessagePending = true;
				}
				else
				{
					nbRead = 0;
					_isMessagePending = false;
				}
			}
			//else message not complete.
			else
			{
				Debug.Log("not finished (size:" + (binarySize + 5) + ", read:" + nbRead + ")");
				_isMessagePending = true;
			}
		}
		else  // unknown message
		{
			var message = BitConverter.ToString(alreadyRead, 0, nbRead).Replace("-", "");
			Debug.LogError("Unknown message: " + message);
			// try to search a message beginning
			int index = ziSearch.Search(alreadyRead);
			if (index > 0)
			{
				Array.Copy(alreadyRead, index, alreadyRead, 0, nbRead - index);
				nbRead -= index;
				if(nbRead > 0)
					_isMessagePending = true;
			}
			else
			{
				nbRead = 0;
				_isMessagePending = false;
			}
		}
		return null;
	}

	public void SendCommand(string command)
	{
		SendCommandWithoutLog(command);
		onCommandSent.Invoke(command);
	}
	public void SendCommandWithoutLog(string command)
	{
		if (s_serial == null || !s_serial.IsOpen)
		{
			Debug.LogError("Can't send command because connection is not established");
			return;
		}
		command = "ZIA++" + command;
		Debug.Log("Sending command: " + command);
		s_serial.WriteLine(command);
	}
	public void SendBinaryCommand(byte[] bcommand)
	{
		if (s_serial == null || !s_serial.IsOpen)
		{
			Debug.LogError("Can't send command because connection is not established");
			return;
		}
		s_serial.Write(bcommand, 0, bcommand.Length);
	}


	#region Update requests (STATUS Messages sending and parsing)
	private bool _updating = false;
	public void UpdateSystemStatus()
	{
		if(!DeviceConnected)
		{
			Debug.LogError("Unable to update because device is not connected");
			return;
		}
		StartCoroutine("_UpdateSystemStatus");
	}

	IEnumerator _UpdateSystemStatus()
	{
		Debug.Log("UpdateSystemStatus");
		if(_updating)
		{
			Debug.Log("Already updating");
			yield break;
		}
		while (_isMessagePending)
			yield return null;
		_updating = true;
		Debug.Log("UpdateSystemStatus reading");
		SendCommand("STATUS SYSTEM XML");
		yield return new WaitForSeconds(0.5f);

		RFPMessage message;
		do
		{
			message = ReadMessage();
			if (message == null)
				yield return null;
		} while (_isMessagePending);
		if(message == null)
		{
			Debug.LogError("No response");
			_updating = false;
			yield break;
		}
		try
		{
			XmlDocument doc = new XmlDocument();
			Debug.Log(message.ASCII);
			doc.LoadXml(message.ASCII.Substring(5));
			var nodes = doc.SelectNodes("/systemStatus/i");
			foreach (XmlNode n in nodes)
			{
				var name = n.SelectSingleNode("n").InnerText;
				var val = n.SelectSingleNode("v").InnerText;
				if (name == "Version")
				{
					this.Firmware = val;
				}
				else if (name == "Mac")
				{
					MacAddress = val;
				}
				else if (name == "LBT")
				{
					LBT = int.Parse(val);
				}
				else if (name == "Jamming")
				{
					Jamming = int.Parse(val);
				}
				//Debug.Log(n.InnerXml);
			}
			// There is a bug in the firmaware. Available protocols sent by module is not consistent.
			onRepeaterProtocolsAvailableChanged.Invoke(_RepeaterProtocolsAvailable);
			//nodes = doc.SelectNodes("/systemStatus/repeater/available/p");
			//List<string> repeaters = new List<string>();
			//foreach (XmlNode n in nodes)
			//{
			//	repeaters.Add(n.InnerText);
			//}
			//RepeaterProtocolsAvailable = repeaters.ToArray();
			nodes = doc.SelectNodes("/systemStatus/repeater/enabled/p");
			bool[] repeatersEnabled = new bool[_RepeaterProtocolsAvailable.Length];
			foreach (XmlNode n in nodes)
			{
				var index = Array.IndexOf(_RepeaterProtocolsAvailable, n.InnerText);
				if(index < 0)
				{
					Debug.LogError("Protocol "+n.InnerText+" is enabled but unavailable");
					continue;
				}
				repeatersEnabled[index] = true;
			}
			RepeaterProtocolsEnabled = repeatersEnabled;
			// There is a bug in the firmaware. Available protocols sent by module is not consistent.
			onReceiverProtocolsAvailableChanged.Invoke(_ReceiverProtocolsAvailable);
			//nodes = doc.SelectNodes("/systemStatus/receiver/available/p");
			//List<string> receivers = new List<string>();
			//foreach (XmlNode n in nodes)
			//{
			//	receivers.Add(n.InnerText);
			//}
			//ReceiverProtocolsAvailable = repeaters.ToArray();
			nodes = doc.SelectNodes("/systemStatus/receiver/enabled/p");
			bool[] receiversEnabled = new bool[_ReceiverProtocolsAvailable.Length];
			foreach (XmlNode n in nodes)
			{
				var index = Array.IndexOf(_ReceiverProtocolsAvailable, n.InnerText);
				if(index < 0)
				{
					Debug.LogError("Receiver protocol " + n.InnerText + " is set to enabled but not in availables one.");
					continue;
				}
				receiversEnabled[index] = true;
			}
			ReceiverProtocolsEnabled = receiversEnabled;
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
		_updating = false;
	}
	public void UpdateRadioStatus()
	{
		if (!DeviceConnected)
		{
			Debug.LogError("Unable to update because device is not connected");
			return;
		}
		StartCoroutine("_UpdateRadioStatus");
	}
	IEnumerator _UpdateRadioStatus()
	{
		Debug.Log("UpdateRadioStatus");
		if (_updating)
		{
			Debug.Log("Already updating");
			yield break;
		}
		while (_isMessagePending)
			yield return null;
		_updating = true;
		SendCommand("STATUS RADIO XML");
		yield return new WaitForSeconds(0.5f);
		RFPMessage message;
		do
		{
			message = ReadMessage();
			if(message == null)
				yield return null;
		} while (_isMessagePending);
		if (message == null)
		{
			Debug.LogError("No response");
			_updating = false;
			yield break;
		}
		try
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(message.ASCII.Substring(5));
			var bands = doc.SelectNodes("/radioStatus/band");
			bool freq868 = false; // first band is presume to be 433MHz, second 868
			foreach (XmlNode band in bands)
			{
				var nodes = band.SelectNodes("i");
				
				foreach (XmlNode n in nodes)
				{
					var name = n.SelectSingleNode("n").InnerText;
					var val = n.SelectSingleNode("v").InnerText;
					if (name == "Frequency")
					{
						if (freq868)
						{
							if(val == "868350")
								FrequencyH = FrequencyHType.F868350;
							else if (val == "868950")
								FrequencyH = FrequencyHType.F868950;
							else // 0
								FrequencyH = FrequencyHType.OFF;
						}
						else
						{
							if (val == "433420")
								FrequencyL = FrequencyLType.F433420;
							else if (val == "433920")
								FrequencyL = FrequencyLType.F433920;
							else // 0
								FrequencyL = FrequencyLType.OFF;
						}
					}
					else if (name == "RFlinkTrigger")
					{
						if (freq868)
							RFLinkTriggerH = int.Parse(val);
						else
							RFLinkTriggerL = int.Parse(val);
					}
					else if (name == "DspTrigger")
					{
						if (freq868)
							DSPTriggerH = int.Parse(val);
						else
							DSPTriggerL = int.Parse(val);
					}
					else if (name == "Selectivity")
					{
						if (freq868)
							SelectivityH = int.Parse(val);
						else
							SelectivityL = int.Parse(val);
					}
					else if (name == "Selectivity")
					{
						if (freq868)
							SensitivityH = int.Parse(val);
						else
							SensitivityL = int.Parse(val);
					}
					else if (name == "RFlink")
					{
						RFlinkEnable = int.Parse(val) == 1;
					}
					//Debug.Log(n.InnerXml);
				}
				freq868 = true;
			}
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
		_updating = false;
	}
	public void UpdateTranscoderStatus()
	{
		if (!DeviceConnected)
		{
			Debug.LogError("Unable to update because device is not connected");
			return;
		}
		StartCoroutine("_UpdateTranscoderStatus");
	}
	IEnumerator _UpdateTranscoderStatus()
	{
		Debug.Log("UpdateTranscoderStatus");
		if (_updating)
		{
			Debug.Log("Already updating");
			yield break;
		}
		while (_isMessagePending)
			yield return null;
		_updating = true;
		SendCommand("STATUS TRANSCODER XML");
		yield return new WaitForSeconds(0.5f);
		RFPMessage message;
		do
		{
			message = ReadMessage();
			if (message == null)
				yield return null;
		} while (_isMessagePending);
		if (message == null)
		{
			Debug.LogError("No response");
			yield break;
		}
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(message.ASCII.Substring(5));
		_updating = false;
	}
	public void UpdateParrotStatus()
	{
		if (!DeviceConnected)
		{
			Debug.LogError("Unable to update because device is not connected");
			return;
		}
		StartCoroutine("_UpdateParrotStatus");
	}
	IEnumerator _UpdateParrotStatus()
	{
		Debug.Log("UpdateParrotStatus");
		if (_updating)
		{
			Debug.Log("Already updating");
			yield break;
		} while (_isMessagePending)
			yield return null;
		_updating = true;
		SendCommand("STATUS PARROT XML");
		yield return new WaitForSeconds(0.5f);
		RFPMessage message;
		do
		{
			message = ReadMessage();
			if (message == null)
				yield return null;
		} while (_isMessagePending);
		XmlDocument doc = new XmlDocument();
		doc.LoadXml(message.ASCII.Substring(5));
		_updating = false;
	}
	#endregion
	
	private void DisposeSerial()
	{
		if (s_serial != null)
		{
			if (s_serial.IsOpen)
			{
				Debug.Log("Closing serial port");
				s_serial.Close();
			}
			s_serial.DataReceived -= serial_DataReceived;
			s_serial.ErrorReceived -= serial_ErrorReceived;
			s_serial.PinChanged -= serial_PinChanged;
			s_serial = null;
		}
	}

	void OpenPort(string port)
	{
		s_serial = new SerialPort(port, 115200, Parity.None, 8, StopBits.One);
		s_serial.DataReceived += serial_DataReceived;
		s_serial.ErrorReceived += serial_ErrorReceived;
		s_serial.PinChanged += serial_PinChanged;
		s_serial.WriteTimeout = 200;
		s_serial.ReadTimeout = 200;
		s_serial.Open();
	}

	private void serial_PinChanged(object sender, SerialPinChangedEventArgs e)
	{
		Debug.Log(e.EventType);
		if((e.EventType & SerialPinChange.DsrChanged) != 0 && !s_serial.DsrHolding)
		{
			DisposeSerial();
			checkPortCoroutine = StartCoroutine(CheckPortsForDevice());
		}
	}


	Coroutine checkPortCoroutine;
	IEnumerator CheckPortsForDevice()
	{
		var wait = new WaitForSeconds(1);
		while(!DeviceConnected)
		{
			var ports = SerialPort.GetPortNames();
			if (ports != null)
			{
				foreach (var p in ports)
				{
					Debug.Log(p);
					if (s_serial != null && s_serial.PortName == p)
						continue;
					OpenPort(p);
					yield return new WaitForSeconds(0.2f);
					SendCommand("HELLO");
					yield return new WaitForSeconds(0.2f);
					var line = s_serial.ReadExisting();
					if (!line.StartsWith("ZIA--"))
					{
						DisposeSerial();
					}
					else
					{
						SendCommand("FORMAT TEXT");
						checkPortCoroutine = null;
						onConnected.Invoke();
						break;
					}
				}
			}
			yield return wait;
		}
		Debug.Log("Device connected");
		checkPortCoroutine = null;
	}

	private void serial_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
	{
		Debug.Log(e.EventType);
	}

	private void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		Debug.Log(e.EventType);
	}

}
