using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MyTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Multiline]
	public string text;
	public RectTransform tooltip;
	public float delay = 0.6f;
	private float time = -1;
	private Vector3 lastPos;
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (tooltip)
		{
			time = 0;
			lastPos = Input.mousePosition;
		}
	}
	public void OnPointerExit(PointerEventData eventData)
	{
		if (tooltip)
		{
			tooltip.gameObject.SetActive(false);
			time = -1;
		}
	}
	private void Update()
	{
		if(time >= 0)
		{
			if((lastPos-Input.mousePosition).sqrMagnitude > 1)
			{
				lastPos = Input.mousePosition;
				time = 0;
				return;
			}
			time += Time.deltaTime;
			if (time > delay && !tooltip.gameObject.activeSelf)
			{
				DisplayTooltip();
			}
		}
	}

	private void DisplayTooltip()
	{
		tooltip.gameObject.SetActive(true);
		tooltip.GetComponentInChildren<TextMeshProUGUI>().text = text;
		tooltip.position = Input.mousePosition;
	}

}
