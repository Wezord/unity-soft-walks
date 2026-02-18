using UnityEngine;

public class Replayer : MonoBehaviour
{

	[SerializeField]
	private string filename;
	[SerializeField]
	private bool playing = false;
	public Transform player;
	public Transform leftHand;
	public Transform rightHand;

	private Recording recording;

	private bool loaded = false;

	public void Start()
	{
		if (playing)
		{
			Play();
		}
	}

	public void Load(string filename)
	{
		this.filename = filename;
		recording = new Recording(player, leftHand, rightHand);
		recording.Load(filename);
		recording.Apply();
		loaded = true;
	}

	public void Play()
	{
		Load(filename);
		playing = true;
	}

	public void Stop()
	{
		playing = false;
	}

	public void FixedUpdate()
	{
		if(playing)
		{
			if (!loaded) Load(filename);
			bool applying = recording.Apply();
			if(!applying) { Stop(); }
		}
	}

	public bool IsPlaying()
	{
		return playing;
	}

	public bool IsLoaded()
	{
		return loaded;
	}

}
