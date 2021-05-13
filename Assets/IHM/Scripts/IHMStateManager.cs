using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IHMStateManager : MonoBehaviour
{
	public GameObject connectionPanel;
	public GameObject connectedPanel;
	public RFPlayerConnection rfplayer;
	public Animator animator;
	private void Awake()
	{
		Disconnected();
		rfplayer.onConnected.AddListener(() => Connected());
		rfplayer.onDisconnected.AddListener(() => Disconnected());
	}

	private void Disconnected()
	{
		connectionPanel.SetActive(true);
		connectedPanel.SetActive(false);
		animator.SetTrigger("Disconnected");
	}
	private void Connected()
	{
		connectionPanel.SetActive(false);
		connectedPanel.SetActive(true);
		animator.SetTrigger("Connected");
	}
}
