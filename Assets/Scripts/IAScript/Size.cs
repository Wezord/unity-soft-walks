using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteAlways]
public class Size : MonoBehaviour
{

	[Min(0f)]
	public double realSize = 0f;

	[Min(0f)]
	public double targetSize = 0f;

	public bool scale = false;
	public bool reset = false;
	public bool global = false;

	public Transform start, end;

	private List<Size> children;

	private void Start()
	{
		if(IsActive())
		{
			RecalculateChildren();
		}
	}

	private void Update()
	{
		if(reset)
		{
			//Rescale(1.0 / ((transform.localScale.x+transform.localScale.y+transform.localScale.z)/3.0));
			transform.localScale = Vector3.one;
			if(!global)
			{
				foreach (Size l in children)
				{
					l.transform.localScale = Vector3.one;
				}
			}
			SetRealSize(Get());
			SetTargetSize(realSize);
			reset = false;
		}

		if(IsActive())
		{
			RecalculateChildren();

			Calculate();

			if(realSize == 0)
			{
				transform.localScale = Vector3.one;
				Calculate();
			}

			if(targetSize == 0)
			{
				SetTargetSize(realSize);
			}

			/*
			if (lastRealSize != realSize)
			{
				SetTargetSize(realSize);
				SetRealSize(realSize);
			}
			*/

			if (Math.Abs(realSize - targetSize) > 0.001 && scale)
			{
				Rescale(targetSize / realSize);
				Calculate();
			}

			/*
			if (transform.localScale != oldScale)
			{
				var scale = transform.localScale;
				Rescale(transform.localScale.magnitude / oldScale.magnitude);
				transform.localScale = scale;
				oldScale = transform.localScale;
			}
			*/
		}
	}

	private double Calculate()
	{
		realSize = Get();
		return realSize;
	}

	private void SetTargetSize(double Size)
	{
		targetSize = Size;
	}
	private void SetRealSize(double Size)
	{
		realSize = Size;
	}

	public void Rescale(double scale)
	{
		if (scale <= 0f) return;
		ApplyScale(scale);
		if(!global)
		{
			foreach (Size l in children)
			{
				l.ApplyScale(1.0 / scale);
			}
		}
	}

	public void ApplyScale(double scale)
	{
		transform.localScale *= (float) scale;
	}

	public double Get()
	{
		return IsActive() ? Vector3.Distance(end.position, start.position) : realSize;
	}
	public bool IsActive()
	{
		return end && start && !Application.isPlaying && enabled;
	}

	public void RecalculateChildren()
	{
		children = new List<Size>();
		if(!global) FindChildren(transform);
	}

	public void FindChildren(Transform tf)
	{
		for(int i = 0; i< tf.childCount; i++)
		{
			var child = tf.GetChild(i);
			Size l = child.GetComponent<Size>();
			if (l)
			{
				children.Add(l);
			} else
			{
				FindChildren(child);
			}
		}
	}

}
