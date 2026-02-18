using System;
using UnityEngine;

public class Recorder : MonoBehaviour
{

    public string folder;
	public string filename;

	private VRPlayer player;

    private bool pressed = false;
    public bool on = false;

    private Recording recording;

    void Start()
    {
        player = GetComponent<VRPlayer>();
        recording = new Recording(player.camera.transform, player.leftHand.transform, player.rightHand.transform);

		if(on)
		{
			System.IO.FileInfo file = new System.IO.FileInfo(folder);
			file.Directory.Create();
			recording.Initialize();
		}

	}

	void FixedUpdate()
    {
		/*
        VRPlayer.player.leftHand.device.query(CommonUsages.secondaryButton, out bool pressed);
        
        if(pressed && !this.pressed)
		{
			on = !on;
			if(!on) {
				Flush();
            }

        }

        this.pressed = pressed;
        */

		if(Input.GetKey(KeyCode.Delete))
		{
			Application.Quit();
			return;
		}

		if (on) {

            recording.Write();

		}

    }

	void Flush()
	{
		if (!on) return;

		recording.Save(folder + "/" + filename + "-" + DateTime.Now.ToFileTime() + ".rcd");
		recording.Clear();
	}

	private void OnDisable()
	{
		Flush();
	}

}
