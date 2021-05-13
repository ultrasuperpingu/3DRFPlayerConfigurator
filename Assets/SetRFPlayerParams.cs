using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RFPlayerConnection))]
public class SetRFPlayerParams : MonoBehaviour
{
	private RFPlayerConnection rfpCon;
	private void Start()
	{
		rfpCon = GetComponent<RFPlayerConnection>();
	}
	public void SetSelectivityL(float val)
	{
		rfpCon.SelectivityL = (int)val;
	}
	public void SetSelectivityH(float val)
	{
		rfpCon.SelectivityL = (int)val;
	}
	public void SetSensitivityL(float val)
	{
		rfpCon.SensitivityL = (int)val;
	}
	public void SetSensitivityH(float val)
	{
		rfpCon.SensitivityH = (int)val;
	}
	public void SetDSPTriggerL(float val)
	{
		rfpCon.DSPTriggerL = (int)val;
	}
	public void SetDSPTriggerH(float val)
	{
		rfpCon.DSPTriggerH = (int)val;
	}
	public void SetRFLinkTriggerL(float val)
	{
		rfpCon.RFLinkTriggerL = (int)val;
	}
	public void SetRFLinkTriggerH(float val)
	{
		rfpCon.RFLinkTriggerH = (int)val;
	}
	public void SetLBT(float val)
	{
		rfpCon.LBT = (int)val;
	}
	public void SetJamming(float val)
	{
		rfpCon.Jamming = (int)val;
	}
	public void SetLedActivity(bool val)
	{
		rfpCon.LEDActivity = val;
	}
}
