using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class DynamicStringToggleList : MonoBehaviour
{
	public Toggle togglePrefab;
	private ScrollRect scrollrect;
	public RFPlayerConnection rfplayer;
	public string command = "REPEATER";
	private void Start()
	{
		scrollrect = GetComponent<ScrollRect>();
	}
	public void SetList(string[] list)
	{
		while (scrollrect.content.transform.childCount > 0)
		{
			var c = scrollrect.content.transform.GetChild(0);
			c.SetParent(null);
			DestroyImmediate(c.gameObject);
		}
		foreach (var l in list)
		{
			var i = Instantiate(togglePrefab);
			i.transform.SetParent(scrollrect.content.transform);
			i.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = l;
			i.onValueChanged.AddListener(b => rfplayer.SendCommand(command + (b ? " + " : " - ") + l));
		}
	}
	public void SetEnabled(bool[] list)
	{
		for(int i=0;i< scrollrect.content.childCount;i++)
		{
			scrollrect.content.GetChild(i).GetComponent<Toggle>().SetIsOnWithoutNotify(list[i]);
		}
	}
}
