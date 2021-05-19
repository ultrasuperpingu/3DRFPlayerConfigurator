using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LaunchFirmwareUpdate : MonoBehaviour
{
	public TMP_InputField fileInput;
	public RFPlayerConnection rfplayer;
	public GameObject confirmUpdate;
	public GameObject updateProgress;
	public Slider updateProgressSlider;
	private void Start()
	{
		rfplayer.onFirmwareUpdateEnded.AddListener(() => updateProgress.SetActive(false));
		rfplayer.onFirmwareUpdateProgress.AddListener(p => updateProgressSlider.value = p);
	}
	public void UpdateFirmware()
	{
		if(!SimpleFileBrowser.FileBrowserHelpers.FileExists(fileInput.text))
		{
			return;
		}
		confirmUpdate.SetActive(false);
		updateProgress.SetActive(true);
		updateProgressSlider.value = 0;
		rfplayer.UpdateFirmware(fileInput.text);
	}
}
