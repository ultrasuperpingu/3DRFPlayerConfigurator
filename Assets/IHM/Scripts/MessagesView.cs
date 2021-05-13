using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessagesView : MonoBehaviour
{
	public GameObject messageViewPrefab;
	
	public void AddMessage(string message)
	{
		var mess = Instantiate(messageViewPrefab);
		mess.transform.SetParent(GetComponent<ScrollRect>().content, false);
		mess.GetComponentInChildren<TextMeshProUGUI>().text = message;
		mess.GetComponentInChildren<Button>().onClick.AddListener(() => GUIUtility.systemCopyBuffer = message);
	}
	public void AddMessage(RFPMessage message)
	{
		var mess = Instantiate(messageViewPrefab);
		mess.transform.SetParent(GetComponent<ScrollRect>().content, false);
		mess.GetComponentInChildren<TextMeshProUGUI>().text = message.ToString();
		mess.GetComponentInChildren<Button>().onClick.AddListener(() => GUIUtility.systemCopyBuffer = mess.GetComponentInChildren<TextMeshProUGUI>().text);
	}
}
