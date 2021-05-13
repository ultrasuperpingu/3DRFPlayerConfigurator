using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TestInsideScreen : MonoBehaviour
{
	RectTransform recttr;
	bool needCheckInsideScreen;
	private void OnEnable()
	{
		needCheckInsideScreen = true;
		recttr = GetComponent<RectTransform>();
	}
	// Update is called once per frame
	void Update()
	{
		if (needCheckInsideScreen)
		{
			needCheckInsideScreen = false;
			var rect = RectTransformToScreenSpace(recttr);
			if (rect.xMin < 0)
			{
				recttr.position -= Vector3.right * rect.xMin;
			}
			if (rect.xMax > Screen.width)
			{
				recttr.position -= Vector3.right * (rect.xMax - Screen.width);
			}
			if (rect.yMin < 0)
			{
				recttr.position -= Vector3.up * rect.yMin;
			}
			if (rect.yMax > Screen.height)
			{
				recttr.position -= Vector3.up * (rect.yMax - Screen.height);
			}
		}
	}
	public static Rect RectTransformToScreenSpace(RectTransform transform)
	{
		Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
		return new Rect((Vector2)transform.position - (size * transform.pivot), size);
	}
}
