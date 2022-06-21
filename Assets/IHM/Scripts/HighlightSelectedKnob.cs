using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HighlightSelectedKnob : MonoBehaviour, ISelectHandler, IDeselectHandler
{
	public GameObject selectedKnob;

    public void OnSelect(BaseEventData eventData)
	{
		selectedKnob.SetActive(true);
	}
	public void OnDeselect(BaseEventData eventData)
	{
		selectedKnob.SetActive(false);
	}
}
