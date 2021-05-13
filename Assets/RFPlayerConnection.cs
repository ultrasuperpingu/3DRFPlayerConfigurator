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

public class RFPlayerConnection : MonoBehaviour
{
	public bool convertBinaryToHexaString = true;
	public UnityEvent onConnected;
	public UnityEvent onDisconnected;
	//public UnityEvent<byte[]> onMessageReceived;
	public UnityEvent<RFPMessage> onMessageReceived;
	//public UnityEvent<string> onTextualMessageReceived;
	public UnityEvent<string> onCommandSent;
	public UnityEvent<string> onFirmwareChanged;
	public UnityEvent<int> onFormatModeChanged;
	public UnityEvent<string> onMACAddressChanged;
	public UnityEvent<bool> onLEDActivityChanged;
	public UnityEvent<bool> onRFLinkEnableChanged;
	public UnityEvent<int> onJammingChanged;
	public UnityEvent<int> onLBTChanged;
	public UnityEvent<int> onFrequencyLChanged;
	public UnityEvent<int> onFrequencyHChanged;
	public UnityEvent<int> onSelectivityLChanged;
	public UnityEvent<int> onSelectivityHChanged;
	public UnityEvent<int> onSensitivityLChanged;
	public UnityEvent<int> onSensitivityHChanged;
	public UnityEvent<int> onDSPTriggerLChanged;
	public UnityEvent<int> onDSPTriggerHChanged;
	public UnityEvent<int> onRFLinkTriggerLChanged;
	public UnityEvent<int> onRFLinkTriggerHChanged;
	public UnityEvent<string[]> onRepeaterProtocolsAvailableChanged;
	public UnityEvent<string[]> onReceiverProtocolsAvailableChanged;
	public UnityEvent<bool[]> onRepeaterProtocolsEnabledChanged;
	public UnityEvent<bool[]> onReceiverProtocolsEnabledChanged;

	private SerialPort s_serial;
	private const int RECEIVE_BUFFER_SIZE = 8192;
	private byte[] alreadyRead = new byte[RECEIVE_BUFFER_SIZE];
	private int nbRead = 0;
	private BoyerMoore ziSearch = new BoyerMoore(new byte[] { (byte)'Z', (byte)'I' });
	//private string[] transmitterProtocolsAvailable = new string[0];
	private string[] receiverProtocolsAvailable = new string[0];
	private bool[] receiverProtocolsEnabled = new bool[0];
	private string[] repeaterProtocolsAvailable = new string[0];
	private bool[] repeaterProtocolsEnabled = new bool[0];
	public class StringList : List<string>
	{
		public StringList(string[] list) : base(list)
		{
		}
	}
	public class BoolList : List<bool>
	{
		public BoolList(bool[] list) : base(list)
		{
		}
	}
	public StringList RepeaterProtocolsAvailable
	{
		get { return new StringList(repeaterProtocolsAvailable); }
		protected set
		{
			repeaterProtocolsAvailable = value.ToArray();
			onRepeaterProtocolsAvailableChanged.Invoke(value.ToArray());
		}
	}
	public StringList ReceiverProtocolsAvailable
	{
		get { return new StringList(receiverProtocolsAvailable); }
		protected set
		{
			receiverProtocolsAvailable = value.ToArray();
			onReceiverProtocolsAvailableChanged.Invoke(value.ToArray());
		}
	}
	public BoolList RepeaterProtocolsEnabled
	{
		get { return new BoolList(repeaterProtocolsEnabled); }
		protected set
		{
			repeaterProtocolsEnabled = value.ToArray();
			onRepeaterProtocolsEnabledChanged.Invoke(value.ToArray());
		}
	}
	public BoolList ReceiverProtocolsEnabled
	{
		get { return new BoolList(receiverProtocolsEnabled); }
		protected set
		{
			receiverProtocolsEnabled = value.ToArray();
			onReceiverProtocolsEnabledChanged.Invoke(value.ToArray());
		}
	}


	public enum FormatModeType : int {
		TEXT,
		JSON,
		XML,
		HEXA,
		HEXA_FIXED,
		BINARY,
		OFF
	}
	
	public enum FrequencyLType : int
	{
		F433420,
		F433920,
		OFF
	}
	
	public enum FrequencyHType : int
	{
		F868350,
		F868950,
		OFF
	}

	public bool DeviceConnected
	{
		get { return checkPortCoroutine == null && s_serial != null && s_serial.IsOpen; }
	}

	#region Parameters
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
	private FrequencyLType _FrequencyL;
	public FrequencyLType FrequencyL
	{
		get => _FrequencyL;
		set
		{
			if (_FrequencyL != value)
			{
				SendCommand("FREQ L " + value.ToString().Replace('_', ' '));
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
	private FrequencyHType _FrequencyH;
	public FrequencyHType FrequencyH
	{
		get => _FrequencyH;
		set
		{
			if (_FrequencyH != value)
			{
				SendCommand("FREQ H " + value.ToString());
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
	private int _SelectivityL = 0;
	public int SelectivityL
	{
		get => _SelectivityL;
		set
		{
			if (value != _SelectivityL && value >= 0 && value <= 5)
			{
				SendCommand("SELECTIVITY L " + value);
				_SelectivityL = value;
				onSelectivityLChanged.Invoke(value);
			}
		}
	}
	private int _SelectivityH = 0;
	public int SelectivityH
	{
		get => _SelectivityH;
		set
		{
			if (value != _SelectivityH && value >= 0 && value <= 5)
			{
				SendCommand("SELECTIVITY H " + value);
				_SelectivityH = value;
				onSelectivityHChanged.Invoke(value);
			}
		}
	}
	private int _SensitivityL = 0;
	public int SensitivityL
	{
		get => _SensitivityL;
		set
		{
			if (value != _SensitivityL && value >= 0 && value <= 4)
			{
				SendCommand("SENSITIVITY L " + value);
				_SensitivityL = value;
				onSensitivityLChanged.Invoke(value);
			}
		}
	}
	private int _SensitivityH = 0;
	public int SensitivityH
	{
		get => _SensitivityH;
		set
		{
			if (value != _SensitivityH && value >= 0 && value <= 4)
			{
				SendCommand("SENSITIVITY H " + value);
				_SensitivityH = value;
				onSensitivityHChanged.Invoke(value);
			}
		}
	}
	private int _DSPTriggerL = 0;
	public int DSPTriggerL
	{
		get => _DSPTriggerL;
		set
		{
			if (value != _DSPTriggerL && value >= 0 && value <= 20)
			{
				_DSPTriggerL = value;
				SendCommand("DSPTRIGGER L " + value);
				onDSPTriggerLChanged.Invoke(value);
			}
		}
	}
	private int _DSPTriggerH = 0;
	public int DSPTriggerH
	{
		get => _DSPTriggerH;
		set
		{
			if (value != _DSPTriggerH && value >= 0 && value <= 20)
			{
				_DSPTriggerH = value;
				SendCommand("DSPTRIGGER H " + value);
				onDSPTriggerHChanged.Invoke(value);
			}
		}
	}
	private int _RFLinkTriggerL = 0;
	public int RFLinkTriggerL
	{
		get => _RFLinkTriggerL;
		set
		{
			if (value != _RFLinkTriggerL && value >= 0 && value <= 20)
			{
				_RFLinkTriggerL = value;
				SendCommand("RFLINKTRIGGER L " + value);
				onRFLinkTriggerLChanged.Invoke(value);
			}
		}
	}
	private int _RFLinkTriggerH = 0;
	public int RFLinkTriggerH
	{
		get => _RFLinkTriggerH;
		set
		{
			if (value != _RFLinkTriggerH && value >= 0 && value <= 20)
			{
				_RFLinkTriggerH = value;
				SendCommand("RFLINKTRIGGER H " + value);
				onRFLinkTriggerHChanged.Invoke(value);
			}
		}
	}
	private int _LBT = 0;
	public int LBT
	{
		get => _LBT;
		set
		{
			if (value != _LBT && value >= 0 && value <= 30)
			{
				_LBT = value;
				SendCommand("LBT " + value);
				onLBTChanged.Invoke(value);
			}
		}
	}
	private int _Jamming = 0;
	public int Jamming
	{
		get => _Jamming;
		set
		{
			if (value != _Jamming && value >= 0 && value <= 10)
			{
				_Jamming = value;
				SendCommand("JAMMING " + value);
				onJammingChanged.Invoke(value);
			}
		}
	}
	private string _MacAddress;
	public string MacAddress
	{
		get => _MacAddress;
		set
		{
			if (_MacAddress != value)
			{
				SendCommand("SETMAC " + value);
				_MacAddress = value;
				onMACAddressChanged.Invoke(value);
			}
		}
	}

	bool _LEDActivity = true;
	public bool LEDActivity
	{
		get => _LEDActivity;
		set
		{
			SendCommand("LEDACTIVITY " + (value ? 1 : 0));
			_LEDActivity = value;
			onLEDActivityChanged.Invoke(value);
		}
	}
	bool _RFlinkEnable = true;
	public bool RFlinkEnable
	{
		get => _RFlinkEnable;
		set
		{
			SendCommand("FORMAT RFLINK " + (value ? "BINARY" : "OFF"));
			_RFlinkEnable = value;
			onRFLinkEnableChanged.Invoke(value);
		}
	}
	// TODO INITLB
	#endregion

	public void FactoryReset(bool all)
	{
		SendCommand("FACTORYRESET" + (all ? "ALL" : ""));
	}

	public void InitLB()
	{
		SendCommand("INITLB");
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
	private bool _isMessagePending = false;
	private RFPMessage ReadMessage()
	{
		Debug.Log("toread: " + s_serial.BytesToRead);
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
	void OnApplicationQuit()
	{
		DisposeSerial();
	}

	public void SendCommand(string command)
	{
		SendCommandWithoutLog(command);
		onCommandSent.Invoke(command);
	}
	public void SendCommandWithoutLog(string command)
	{
		if (!DeviceConnected)
		{
			Debug.LogError("Can't send command because connection is not established");
			return;
		}
		command = "ZIA++" + command;
		s_serial.WriteLine(command);
	}
	private bool _updating = false;
	public void UpdateSystemStatus()
	{
		if(!DeviceConnected)
		{
			Debug.LogError("Unable to update because deviceis not connected");
			return;
		}
		StartCoroutine("_UpdateSystemStatus");
	}

	IEnumerator _UpdateSystemStatus()
	{
		Debug.Log("UpdateSystemStatus");
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
			doc.LoadXml(message.ASCII.Substring(5));
			var nodes = doc.SelectNodes("/systemStatus/i");
			foreach (XmlNode n in nodes)
			{
				var name = n.SelectSingleNode("n").InnerText;
				var val = n.SelectSingleNode("v").InnerText;
				if (name == "Version")
					this.Firmware = val;
				else if (name == "Mac")
					MacAddress = val;
				else if (name == "LBT")
					LBT = int.Parse(val);
				else if (name == "Jamming")
					Jamming = int.Parse(val);
				Debug.Log(n.InnerXml);
			}
			nodes = doc.SelectNodes("/systemStatus/repeater/available/p");
			List<string> repeaters = new List<string>();
			foreach (XmlNode n in nodes)
			{
				repeaters.Add(n.InnerText);
			}
			RepeaterProtocolsAvailable = new StringList(repeaters.ToArray());
			nodes = doc.SelectNodes("/systemStatus/repeater/enabled/p");
			bool[] repeatersEnabled = new bool[repeaters.Count];
			foreach (XmlNode n in nodes)
			{
				repeatersEnabled[repeaters.IndexOf(n.InnerText)] = true;
			}
			RepeaterProtocolsEnabled = new BoolList(repeatersEnabled);
			nodes = doc.SelectNodes("/systemStatus/receiver/available/p");
			List<string> receivers = new List<string>();
			foreach (XmlNode n in nodes)
			{
				receivers.Add(n.InnerText);
			}
			ReceiverProtocolsAvailable = new StringList(repeaters.ToArray());
			nodes = doc.SelectNodes("/systemStatus/receiver/enabled/p");
			bool[] receiversEnabled = new bool[receivers.Count];
			foreach (XmlNode n in nodes)
			{
				receiversEnabled[receivers.IndexOf(n.InnerText)] = true;
			}
			ReceiverProtocolsEnabled = new BoolList(receiversEnabled);
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
			Debug.LogError("Unable to update because deviceis not connected");
			return;
		}
		StartCoroutine("_UpdateRadioStatus");
	}
	IEnumerator _UpdateRadioStatus()
	{
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
			foreach (XmlNode band in bands)
			{
				var nodes = band.SelectNodes("i");
				bool freq868 = false;
				foreach (XmlNode n in nodes)
				{
					var name = n.SelectSingleNode("n").InnerText;
					var val = n.SelectSingleNode("v").InnerText;
					if (name == "Frequency")
					{
						freq868 = val.StartsWith("868");
						//if (freq868)
						//	FrequencyH = val;
					}
					else if (name == "Mac")
						MacAddress = val;
					else if (name == "LBT")
						LBT = int.Parse(val);
					else if (name == "Jamming")
						Jamming = int.Parse(val);
					Debug.Log(n.InnerXml);
				}
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
			Debug.LogError("Unable to update because deviceis not connected");
			return;
		}
		StartCoroutine("_UpdateTranscoderStatus");
	}
	IEnumerator _UpdateTranscoderStatus()
	{
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
			Debug.LogError("Unable to update because deviceis not connected");
			return;
		}
		StartCoroutine("_UpdateParrotStatus");
	}
	IEnumerator _UpdateParrotStatus()
	{
		while (_isMessagePending)
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
		s_serial.WriteTimeout = 100;
		s_serial.ReadTimeout = 100;
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
	/*IEnumerator SendCommandAndGetAnswer(string command, out byte[] response)
	{
		SendCommand(command);
		yield return ReadMessageCoroutine(out response);

	}

	private IEnumerator ReadMessageCoroutine(out byte[] response)
	{
		response = alreadyRead;
		yield return null;
	}*/

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
					Debug.Log(DeviceConnected);
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
						checkPortCoroutine = null;
						break;
					}
				}
			}
			/*ManagementObjectSearcher manObjSearch = new ManagementObjectSearcher("Select * from Win32_SerialPort");
			ManagementObjectCollection manObjReturn = manObjSearch.Get();

			foreach (ManagementObject manObj in manObjReturn)
			{
				//int s = manObj.Properties.Count;
				//foreach (PropertyData d in manObj.Properties)
				//{
				//    Console.WriteLine(d.Name);
				//}
				Console.WriteLine(manObj["DeviceID"].ToString());
				Console.WriteLine(manObj["Name"].ToString());
				Console.WriteLine(manObj["Caption"].ToString());
			}*/
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
