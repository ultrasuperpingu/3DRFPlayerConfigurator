﻿using System;
using System.Runtime.InteropServices;
using System.Text;

public class RFPMessage
{
	public enum MessageType
	{
		ASCII,
		BINARY
	}
	public enum FrameType : byte
	{
		KNOWN_PROTOCOL = 0,
		RFLINK = 1
	}
	string asciiContent;
	byte[] binaryContent;
	FrameType frameType;
	REGULAR_INCOMING_RF_TO_BINARY_USB_FRAME frameKnown;
	INCOMING_RFLINK_FRAME frameRFLink;
	public RFPMessage(MessageType t, byte[] content, int startIndex, int count)
	{
		Type = t;
		if (t == MessageType.ASCII)
		{
			asciiContent = Encoding.ASCII.GetString(content, startIndex, count);
		}
		else
		{
			binaryContent = new byte[count];
			Array.Copy(content, startIndex, binaryContent, 0, count);
			frameType = (FrameType)binaryContent[5];
			if (frameType == FrameType.KNOWN_PROTOCOL)
			{
				frameKnown = ByteArrayToStructure<REGULAR_INCOMING_RF_TO_BINARY_USB_FRAME>(binaryContent, 5);
				UnityEngine.Debug.Log(frameKnown.ToString());
			}
			else if (frameType == FrameType.RFLINK)
			{
				//TODO: FIXME (or check me) @see ByteArrayToStructure
				frameRFLink = ByteArrayToStructure<INCOMING_RFLINK_FRAME>(binaryContent, 5);
				UnityEngine.Debug.Log(frameRFLink.ToString());
			}
		}
	}
	const int MAX_NB_RFLINK_PULSE = 512;
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct INCOMING_RFLINK_FRAME
	{
		[FieldOffset(0)] public byte frameType; //=1: RFLINK Frame
		[FieldOffset(1)] public uint frequency; // Frequency expressed in Khz
												// Available : 433420, 433920, 868350, 868950. 
		[FieldOffset(5)] public sbyte RFLevel; // Unit : dB(high signal :-40dB to low : -110dB)
		[FieldOffset(6)] public sbyte floorNoise; // Unit : dB(high signal :-40dB to low : -110dB)
		[FieldOffset(7)] public byte pulseElementSize; // Value : 1 
		[FieldOffset(8)] public ushort number; // Number of Pulses upon RFLINK definition
		[FieldOffset(10)] public byte repeats; // Number of re-transmits upon RFLINK definition
		[FieldOffset(11)] public byte delay; // Delay in ms.after trans.upon RFLINK definition
		[FieldOffset(12)] public byte multiply; // Real pulse unit in microseconds upon RFLINK definition
												// Value = 40
		[FieldOffset(16)] public uint time; // Timestamp indicating when the signal was received upon RFLINK definition
		[FieldOffset(20)] public fixed byte pulses[MAX_NB_RFLINK_PULSE]; //size: number+2 Pulses[0] and Pulses[number+1] are set to 0  upon historical RFLINK definition
		public override string ToString()
		{
			string s = "[RFLink] frameType: " + frameType + " frequency: " + frequency + " RFLevel: " + RFLevel + " floorNoise: " + floorNoise + " pulseElementSize: " + pulseElementSize;
			s += "\r\nnbPulse: " + number + " repeats: " + repeats + " delay: " + delay + " multiply: " + multiply + " time:" + time;
			s += "pulses:\r\n";
			for (int i = 1; i < number && i < MAX_NB_RFLINK_PULSE; i++)
				s += pulses[i].ToString("X2") + " ";
			return s;
		}
	}
	public override string ToString()
	{
		if (Type == MessageType.ASCII)
			return ASCII;
		if(frameType == FrameType.KNOWN_PROTOCOL)
			return frameKnown.ToString();
		return frameRFLink.ToString();
	}

	public MessageType Type
	{
		get;
		protected set;
	}
	public string ASCII
	{
		get { return asciiContent; }
	}
	public byte[] BINARY
	{
		get { return binaryContent; }
	}
	public string BINARYHEX
	{
		get { return BitConverter.ToString(binaryContent).Replace("-", ""); }
	}
	public int SourceDestQualifier
	{
		get
		{
			if(Type == MessageType.ASCII)
			{
				return asciiContent[2];
			}
			return binaryContent[2];
		}
	}

	public enum SendProtocol : byte
	{
		UNKNOWN     = 0,
		VISONIC_433 = 1, //PowerCode only
		VISONIC_868 = 2, //PowerCode only
		CHACON_433  = 3, 
		DOMIA_433   = 4,
		X10_433     = 5,
		X2D_433     = 6, //Alarm
		X2D_868     = 7, //Alarm
		X2D_SHUTTER_868 = 8, //Qualifier7:6 =1 to emit X2D variant, else Qualifier = 0
		X2D_HA_ELEC_868 = 9, //Qualifier7:6 =1 to emit X2D variant, else 0
		X2D_HA_GAS_868  = 10, //Qualifier7:6 =1 to emit X2D variant, else 0 
		SOMFY_RTS_433   = 11, //Qualifier D0 = 1 to emit portal frame, else Qualifier D0 = 0(or nothing)
							  //DIM values between 0—15 emulate specific RTS functions. 
							  //Specify DIM %4 to emulate RTS “My” function. 
							  //Qualifier D1 = 1 can emulate the same specific RTS functions
							  //but specified on D7:4 of the qualifier. 
							  //Specify D7:4 to 4 to emulate RTS “My” function.
		BLYSS_433         = 12,
		PARROT_433_OR_868 = 13, //ID0 to ID254 (A1…P15). ID255(P16) is reserved and must not be used.
		OREGON_433X       = 14, // not reachable by API
		OWL_433X          = 15, // not reachable by API
		KD101_433         = 16, //Smoke detector. 32 bits ID
		DIGIMAX_TS10_433  = 17, // deprecated
		OREGON_V1_433     = 18, // not reachable by API
		OREGON_V2_433     = 19, // not reachable by API
		OREGON_V3_433     = 20, // not reachable by API
		TIC_433           = 21, // not reachable by API
		FS20_868          = 22
	}
	public enum Action : byte
	{
		OFF        = 0, //Used by most protocols
		ON         = 1, //Used by most protocols
		DIM        = 2, //Used by some protocols
		BRIGHT     = 3, //not used
		ALL_OFF    = 4, //Used by BLYSS
		ALL_ON     = 5, //Used by BLYSS
		ASSOC      = 6, //Used by most protocols
		DISSOC     = 7, //provision
		ASSOC_OFF  = 8, //Used by PARROT for its OFF entries
		DISSOC_OFF = 9// provision
	}
	public struct MESSAGE_CONTAINER_HEADER
	{
		public byte Sync1;
		public byte Sync2;
		public byte SourceDestQualifier;
		public byte QualifierOrLen_lsb;
		public byte QualifierOrLen_msb;
	}

	public struct REGULAR_INCOMING_BINARY_USB_FRAME
	{ // public binary API USB to RF frame sending
		public byte frameType;
		public byte cluster; // set 0 by default. Cluster destination.
		public byte protocol;
		public byte action;
		public uint ID; // LSB first. Little endian, mostly 1 byte is used
		public byte dimValue;
		public byte burst;
		public byte qualifier;
		public byte reserved2; // set 0
	}

/*
 *
Binary Data[]	Size	Type	Remark
FrameType	1	unsigned char	Value = 0
DataFlag	1	unsigned char	0: 433MHz, 1: 868MHz
RFLevel	1	signed char	Unit : dB  (high signal :-40dB to low : -110dB)
FloorNoise	1	signed char	Unit : dB  (high signal :-40dB to low : -110dB)
Protocol	1	unsigned char	See below
InfosType	1	unsigned char	See below
 */

/* *************************************************************************** */

/*
Binary Data[]	Size	Type	Remark
FrameType	1	unsigned char	Value = 0
DataFlag	1	unsigned char	0: 433MHz, 1: 868MHz
RFLevel	1	signed char	Unit : dB  (high signal :-40dB to low : -110dB)
FloorNoise	1	signed char	Unit : dB  (high signal :-40dB to low : -110dB)
RFQuality	1	unsigned char
Protocol	1	unsigned char	See below
InfosType	1	unsigned char	See below
Infos[0?9]	20	Signed or unsigned short
upon context	LSB first. Define provided data by the device
*/


/* ***************************************** */
	public enum IncomingProtocol
	{
		UNDEFINED = 0,
		X10       = 1,
		VISONIC   = 2,
		BLYSS     = 3,
		CHACON    = 4,
		OREGON    = 5,
		DOMIA     = 6,
		OWL       = 7,
		X2D       = 8,
		RFY       = 9,
		KD101     = 10,
		PARROT    = 11,
		DIGIMAX   = 12, /* deprecated */
		TIC       = 13,
		FS20      = 14
	}

	public struct INCOMING_RF_INFOS_TYPE0
	{ // used by X10 / Domia Lite protocols
		public ushort subtype;
		public ushort id;
		public override string ToString()
		{
			string s = "subtype: " + subtype + " ID: " + id;
			return s;
		}
	};
	public struct INCOMING_RF_INFOS_TYPE1
	{ // Used by X10 (32 bits ID) and CHACON
		public ushort subtype;
		public ushort idLsb;
		public ushort idMsb;
		public override string ToString()
		{
			string s = "[X10 (32 bits ID) and CHACON] subtype: " + subtype + " ID: " + (idLsb + idMsb << 16);
			return s;
		}
	};
	public struct INCOMING_RF_INFOS_TYPE2
	{ // Used by  VISONIC /Focus/Atlantic/Meian Tech
		public ushort subtype;
		public ushort idLsb;
		public ushort idMsb;
		public ushort qualifier;
		public override string ToString()
		{
			string s = "[VISONIC/Focus/Atlantic/Meian Tech] subtype: " + subtype + " ID: " + (idLsb + idMsb << 16) + " qualifier: " + qualifier;
			return s;
		}
	};
	public struct INCOMING_RF_INFOS_TYPE3
	{ //  Used by RFY PROTOCOL
		public ushort subtype;
		public ushort idLsb;
		public ushort idMsb;
		public ushort qualifier;
		public override string ToString()
		{
			string s = "[RFY] subtype: " + subtype + " ID: " + (idLsb + idMsb << 16) + " qualifier: " + qualifier;
			return s;
		}
	};

	public struct INCOMING_RF_INFOS_TYPE4
	{ // Used by  Scientific Oregon  protocol ( thermo/hygro sensors)
		public ushort subtype;
		public ushort idPHY;
		public ushort idChannel;
		public ushort qualifier;
		public short temp;    // UNIT:  1/10 of degree Celsius
		public ushort hygro; // 0...100  UNIT: %
		public override string ToString()
		{
			string s = "[Scientific Oregon protocol (thermo/hygro sensors)] subtype: " + subtype + " idPhy: " + idPHY + " idChannel: " + idChannel + " qualifier: " + qualifier;
			s += "\r\ntemperature: " + temp + " hygro: " + hygro;
			return s;
		}
	};
	public struct INCOMING_RF_INFOS_TYPE5
	{ // Used by  Scientific Oregon  protocol  ( Atmospheric  pressure  sensors)
		public ushort subtype;
		public ushort idPHY;
		public ushort idChannel;
		public ushort qualifier;
		public short temp;   // UNIT:  1/10 of degree Celsius
		public ushort hygro;    // 0...100  UNIT: %
		public ushort pressure; //  UNIT: hPa
		public override string ToString()
		{
			string s = "[Scientific Oregon protocol (Atmospheric pressure sensors)] subtype: " + subtype + " idPhy: " + idPHY + " idChannel: " + idChannel + " qualifier: " + qualifier;
			s += "\r\ntemperature: " + temp + " hygro: " + hygro + " pressure: " + pressure;
			return s;
		}
	};

	public struct INCOMING_RF_INFOS_TYPE6
	{ // Used by  Scientific Oregon  protocol  (Wind sensors)
		public ushort subtype;
		public ushort idPHY;
		public ushort idChannel;
		public ushort qualifier;
		public ushort speed;     // Averaged Wind speed   (Unit : 1/10 m/s, e.g. 213 means 21.3m/s)
		public ushort direction; //  Wind direction  0-359° (Unit : angular degrees)
		public override string ToString()
		{
			string s = "[Scientific Oregon protocol (Wind sensors)] subtype: " + subtype + " idPhy: " + idPHY + " idChannel: " + idChannel + " qualifier: " + qualifier;
			s += "\r\nspeed: " + speed + " direction: " + direction;
			return s;
		}

	};
	public struct INCOMING_RF_INFOS_TYPE7
	{ // Used by  Scientific Oregon  protocol  ( UV  sensors)
		public ushort subtype;
		public ushort idPHY;
		public ushort idChannel;
		public ushort qualifier;
		public ushort light; // UV index  1..10  (Unit : -)
		public override string ToString()
		{
			string s = "[Scientific Oregon protocol (UV sensors)] subtype: " + subtype + " idPhy: " + idPHY + " idChannel: " + idChannel + " qualifier: " + qualifier;
			s += "\r\nlight: " + light;
			return s;
		}
	};
	public struct INCOMING_RF_INFOS_TYPE8
	{ // Used by  OWL  ( Energy/power sensors)
		public ushort subtype;
		public ushort idPHY;
		public ushort idChannel;
		public ushort qualifier;
		public ushort energyLsb; // LSB: energy measured since the RESET of the device  (32 bits value). Unit : Wh
		public ushort energyMsb; // MSB: energy measured since the RESET of the device  (32 bits value). Unit : Wh
		public ushort power;     //  Instantaneous measured (total)power. Unit : W (with U=230V, P=P1+P2+P3))
		public ushort powerI1;   // Instantaneous measured at input 1 power. Unit : W (with U=230V, P1=UxI1)
		public ushort powerI2;   // Instantaneous measured at input 2 power. Unit : W (with U=230V, P2=UxI2)
		public ushort powerI3;   // Instantaneous measured at input 3 power. Unit : W (with U=230V, P2=UxI3)
		public override string ToString()
		{
			string s = "[OWL (Energy/power sensors)] subtype: " + subtype + " idPhy: " + idPHY + " idChannel: " + idChannel + " qualifier: " + qualifier;
			s += "\r\nenergy: " + (energyLsb + energyMsb << 16) + " power: "+power + " powerI1: " + powerI1 + " powerI2: " + powerI2 + " powerI3: " + powerI3;
			return s;
		}
	};

	public struct INCOMING_RF_INFOS_TYPE9
	{ // Used by  OREGON  ( Rain sensors)
		public ushort subtype;
		public ushort idPHY;
		public ushort idChannel;
		public ushort qualifier;
		public ushort totalRainLsb; // LSB: rain measured since the RESET of the device  (32 bits value). Unit : 0.1 mm
		public ushort totalRainMsb; // MSB: rain measured since the RESET of the device
		public ushort rain;         // Instantaneous measured rain. Unit : 0.01 mm/h
		public override string ToString()
		{
			string s = "[OREGON (Rain sensors)] subtype: " + subtype + " idPhy: " + idPHY + " idChannel: " + idChannel + " qualifier: " + qualifier;
			s += "\r\ntotalRain: " + (totalRainLsb + totalRainMsb << 16);
			return s;
		}
	};

	public struct INCOMING_RF_INFOS_TYPE10
	{ // Used by Thermostats  X2D protocol
		public ushort subtype;
		public ushort idLsb;
		public ushort idMsb;
		public ushort qualifier; // D0 : Tamper Flag, D1: Alarm Flag, D2: Low Batt Flag, D3: Supervisor Frame, D4: Test  D6:7 : X2D variant
		public ushort function;
		public ushort mode;    //
		public ushort data1; // provision
		public ushort data2; // provision
		public ushort data3; // provision
		public ushort data4; // provision
		public override string ToString()
		{
			string s = "[Thermostats X2D protocol] subtype: " + subtype + " ID: " + (idLsb + idMsb << 16) + " qualifier: " + qualifier;
			s += "\r\nfunction: " + function + " mode: " + mode;
			return s;
		}
	};
	public struct INCOMING_RF_INFOS_TYPE11
	{ // Used by Alarm/remote control devices  X2D protocol
		public ushort subtype;
		public ushort idLsb;
		public ushort idMsb;
		public ushort qualifier; // D0 : Tamper Flag, D1: Alarm Flag, D2: Low Batt Flag, D3: Supervisor Frame, D4: Test  D6:7 : X2D variant
		public ushort reserved1;
		public ushort reserved2;
		public ushort data1; //  provision
		public ushort data2; //  provision
		public ushort data3; //  provision
		public ushort data4; //  provision
		public override string ToString()
		{
			string s = "[Alarm/remote control devices X2D] subtype: " + subtype + " ID: " + (idLsb + idMsb << 16) + " qualifier: " + qualifier;
			return s;
		}
	};

	public struct INCOMING_RF_INFOS_TYPE12
	{ // Used by  DIGIMAX TS10 protocol // deprecated
		public ushort subtype;
		public ushort idLsb;
		public ushort idMsb;
		public ushort qualifier;
		public short temp;     // UNIT:  1/10 of degree
		public short setPoint; // UNIT:  1/10 of degree
		public override string ToString()
		{
			string s = "[DIGIMAX TS10 protocol] subtype: " + subtype + " ID: " + (idLsb + idMsb << 16) + " qualifier: " + qualifier;
			s += "\r\ntemp: " + temp + " setPoint: " + setPoint;
			return s;
		}
	};

	public struct INCOMING_RF_INFOS_TYPE13
	{               // Used by  Cartelectronic TIC/Pulses devices (Teleinfo/TeleCounters)
		public ushort subtype; // subtype/version (0: Teleinfo mode, 1: Encoder Mode, 2: Linky Mode)
		public ushort idLsb;
		public ushort idMsb;
		public ushort qualifier; // Teleinfo mode:
								 // DO: battery flag (1: low)
								 // D1: Apparent power valid
								 // D2: teleinfo error (1: error)
								 // D3-D4: (0: no change price time warning, 1: white, 2: blue, 3 red/PEJP)
								 // D5: reserved for future usage
								 // D6: Teleinfo desactivated
								 // D7: Production
								 // Encoder mode
								 // DO: battery flag (1: low)
								 // Linky mode
								 // DO: battery flag (1: low)
								 // D1: Apparent power valid
								 // D2: teleinfo present
								 // D3-D4: PEJP or color price forcast for tomorrow(0: no change price time warning, 1: white, 2: blue, 3 red/PEJP)
								 // D5-D6: color price for today (0: no change price time warning, 1: white, 2: blue, 3 red/PEJP)
		public ushort infos;     // Teleinfo mode:
								  // D0-D7: contract type/current price time (1: HC, 0: HP)
								  // Linky mode:
								  // D0-D3: spare
								  // D4-D7: current index
								  // D8-D15: average voltage
		public ushort counter1Lsb; // unit Wh (HC)
		public ushort counter1Msb;
		public ushort counter2Lsb; // unit Wh (HP)
		public ushort counter2Msb;
		public ushort apparentPower; // unit: Watt (in fact, it is VA)
		public override string ToString()
		{
			string s = "[Cartelectronic TIC/Pulses devices (Teleinfo/TeleCounters)] subtype: " + subtype + " ID: " + (idLsb + idMsb << 16) + " qualifier: " + qualifier;
			s += "\r\ninfos: "+infos + " counter1: " + (counter1Lsb + counter1Msb << 16) + " counter2: " + (counter2Lsb + counter2Msb << 16) + "apparentPower: " + apparentPower;
			return s;
		}
	};

	public struct INCOMING_RF_INFOS_TYPE14
	{ // Used by FS20. Same file as INCOMING_RF_INFOS_TYPE2
		public ushort subtype;
		public ushort idLsb;
		public ushort idMsb;
		public ushort qualifier;
		public override string ToString()
		{
			string s = "[FS20] subtype: " + subtype + " ID: " + (idLsb + idMsb<<16) + " qualifier: " + qualifier;
			return s;
		}
	};
	/* *************************************************************************** */

	public struct REGULAR_INCOMING_RF_TO_BINARY_USB_FRAME_HEADER
	{                // public binary API   RF to USB
		public byte frameType; // Value = 0
		public byte cluster;   // cluster origin. Reserved field
		public byte dataFlag;  // 0: 433Mhz, 1: 868MHz
		public sbyte rfLevel;     // Unit : dBm  (high signal :-40dBm to low : -110dB)
		public sbyte floorNoise;  // Unit : dBm  (high signal :-40dBm to low : -110dB)
		public byte rfQuality; // factor or receiving quality : 1...10 : 1 : worst quality, 10 : best quality
		public byte protocol;  // protocol under scope
		public byte infoType;  // type of payload
		public override string ToString()
		{
			string s = "FrameType: " + frameType + " Protocol: " + protocol + " RF level:" + rfLevel + " RF quality:" + rfQuality;
			return s;
		}
	};
	const int REGULAR_INCOMING_RF_BINARY_USB_FRAME_INFOS_WORDS_NUMBER = 10;


	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct REGULAR_INCOMING_RF_TO_BINARY_USB_FRAME
	{ // public binary API
		[FieldOffset(0)] public REGULAR_INCOMING_RF_TO_BINARY_USB_FRAME_HEADER header;
		[FieldOffset(8)] public fixed short word[REGULAR_INCOMING_RF_BINARY_USB_FRAME_INFOS_WORDS_NUMBER];
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE0 type0;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE1 type1;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE2 type2;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE2 type3;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE4 type4;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE5 type5;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE6 type6;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE7 type7;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE8 type8;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE9 type9;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE10 type10;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE11 type11;
		[FieldOffset(8)] public INCOMING_RF_INFOS_TYPE12 type12;
		public override string ToString()
		{
			string s = header.ToString() + "\r\n";
			switch (header.infoType)
			{
				case 0:
					s += type0.ToString();
					break;
				case 1:
					s += type1.ToString();
					break;
				case 2:
					s += type2.ToString();
					break;
				case 3:
					s += type3.ToString();
					break;
				case 4:
					s += type4.ToString();
					break;
				case 5:
					s += type5.ToString();
					break;
				case 6:
					s += type6.ToString();
					break;
				case 7:
					s += type7.ToString();
					break;
				case 8:
					s += type8.ToString();
					break;
				case 9:
					s += type9.ToString();
					break;
				case 10:
					s += type10.ToString();
					break;
				case 11:
					s += type11.ToString();
					break;
				case 12:
					s += type12.ToString();
					break;
				default:
					s += "Error, infoType not known";
					break;
			}
			return s;
		}
	}
	//TODO: FIXME (or check me)
	// What happens if the size of byte array is less than the structure size (and it happens)
	unsafe T ByteArrayToStructure<T>(byte[] bytes, int startOffset = 0) where T : struct
	{
		fixed (byte* ptr = &bytes[startOffset])
		{
			return (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
		}
	}
}
