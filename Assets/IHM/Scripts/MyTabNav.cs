using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MyTabNav : MonoBehaviour
{
	public FedoraEssentials.Tabber tabGroup;
	public List<GameObject> tabButtons;
	public List<Selectable> pageStartSelected;
	[System.Serializable]
	public class DialogPanel
	{
		public GameObject panel;
		public Selectable panelStartSelected;
		public List<Selectable> panelSelectables;
	}
	public List<DialogPanel> dialogs;
	public void Update()
	{
		Selectable next = null;
		if (SimpleFileBrowser.FileBrowser.IsOpen)
			return;
		//Debug.Log(EventSystem.current.currentSelectedGameObject);
		if (Input.GetKeyDown(KeyCode.Tab) && !Input.GetKey(KeyCode.LeftShift))
		{
			next = CheckDialog();
			if(!next)
				next = CheckGlobal();
		}
		else if (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift))
		{
			next = CheckDialog(true);
			if (!next)
				next = CheckGlobal(true);
		}
		if (next != null && next.isActiveAndEnabled)
		{
			EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
		}
	}

	private Selectable CheckDialog(bool up = false)
	{
		foreach(var d in dialogs)
		{
			if(d.panel.activeInHierarchy)
			{
				if (EventSystem.current.currentSelectedGameObject == null || 
					!d.panelSelectables.Contains(EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>()))
				{
					return d.panelStartSelected;
				}
				else if(EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>())
				{
					Selectable next = null;
					if(up)
						next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnLeft();
					else
						next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnRight();
					if(next == null)
					{
						if (up)
							next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
						else
							next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
					}
					if (next == null)
					{
						next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
					}
					return next;
				}
			}
		}
		return null;
	}

	private Selectable CheckGlobal(bool up = false)
	{
		Selectable next = null;
		if (EventSystem.current.currentSelectedGameObject == null)
		{
			if(up && tabGroup.activePanel > -1)
				next = pageStartSelected[tabGroup.activePanel].GetComponent<Selectable>();
			else
				next = tabButtons[0].GetComponent<Selectable>();
		}
		else if (!up && tabButtons.Contains(EventSystem.current.currentSelectedGameObject))
		{
			if (tabGroup.activePanel > -1)
				next = pageStartSelected[tabGroup.activePanel].GetComponent<Selectable>();
		}
		else
		{
			var sel = EventSystem.current.currentSelectedGameObject;
			if (sel && sel.GetComponent<Selectable>())
			{
				if(up)
					next = sel.GetComponent<Selectable>().FindSelectableOnLeft();
				else
					next = sel.GetComponent<Selectable>().FindSelectableOnRight();
				if (!next)
				{
					if(up)
						next = sel.GetComponent<Selectable>().FindSelectableOnUp();
					else
						next = sel.GetComponent<Selectable>().FindSelectableOnDown();
				}
			}
		}

		return next;
	}
}
