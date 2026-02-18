using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Jointer : MonoBehaviour
{
	private GenericPlayerInput input;

	private Grabber grabber;
	private Grabber grabbedGrabber;
	private List<Grabber> grabbers = new List<Grabber>();

	private bool pressed = false;
	private bool jointing = false;

    void Start()
    {
		input = GenericPlayerInput.GetInput(gameObject);
		grabber = GetComponent<Grabber>();
		grabbedGrabber = null;
	}
	void FixedUpdate()
	{
		if (grabber.grabJoint != null && grabber.grabJoint.connectedBody != null)
		{
			var grabbedObject = grabber.grabJoint.connectedBody.gameObject;
			grabbedGrabber = grabbedObject.GetComponent<Grabber>();

			if (grabbedGrabber == null)
			{
				grabbedGrabber = grabbedObject.AddComponent<Grabber>();
				grabbedGrabber.playerType = grabber.playerType;
				//grabbedGrabber.grabSoundEvent = grabber.grabSoundEvent;
				//grabbedGrabber.unGrabSoundEvent = grabber.unGrabSoundEvent;
			}

			jointing = grabbedGrabber.grabbing;

			if (input.GetInteractButton() && !pressed)
			{
				jointing = !jointing;
				grabbedGrabber.SetGrabbing(jointing);
			}
			pressed = input.GetInteractButton();

		} else
		{
			if(grabbedGrabber != null && !grabbedGrabber.grabbing)
			{
				Destroy(grabbedGrabber);
				grabbedGrabber = null;
			}
			pressed = false;
			jointing = false;
		}
	}
}
