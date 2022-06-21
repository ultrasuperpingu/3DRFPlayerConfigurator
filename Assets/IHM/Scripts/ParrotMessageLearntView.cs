using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParrotMessageLearntView : MonoBehaviour
{
	public GameObject parrotMessagePrefab;
	public RFPlayerConnection rfplayer;

	public void SetList(RFPlayerConnection.ParrotMessageLearnt[] list)
	{
		var scrollrect = GetComponent<ScrollRect>();
		while (scrollrect.content.transform.childCount > 0)
		{
			var c = scrollrect.content.transform.GetChild(0);
			c.SetParent(null);
			DestroyImmediate(c.gameObject);
		}
		foreach (var l in list)
		{
			var i = Instantiate(parrotMessagePrefab);
			i.transform.SetParent(scrollrect.content.transform);
			i.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = l.ToString();
			i.GetComponentInChildren<Button>().onClick.AddListener(() => rfplayer.SendCommand(l.Command));
		}
	}
}
