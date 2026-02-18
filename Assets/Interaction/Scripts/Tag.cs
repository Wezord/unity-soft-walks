using UnityEngine;

public class Tag : MonoBehaviour
{
    public GameObject fps;
    public Color fpsColor;
    public LayerMask fpsMask;

    public GameObject vr;
    public Color vrColor;
    public LayerMask vrMask;

    private bool started = false;
    private PlayerType playerType = PlayerType.FPS;
    private new Light light;
    
    private void SetItVR()
	{
        if(started && playerType == PlayerType.FPS)
		{
            SetIt(PlayerType.VR);
        }
	}

    private void SetItFPS()
    {
        if (started && playerType == PlayerType.VR)
        {
            SetIt(PlayerType.FPS);
        }
    }

    
    public void StartGame()
	{
        Events.grabEvent += (grabber, obj, type) =>
        {
            if (obj.TryGetComponent<TagGrabbed>(out var grabbed))
            {
                Destroy(grabbed);
            }
            var tg = obj.AddComponent<TagGrabbing>();
            tg.type = type;
            switch (type)
            {
                case PlayerType.VR:
                    tg.collided = SetItFPS;
                    break;
                case PlayerType.FPS:
                    tg.collided = SetItVR;
                    break;
            }
        };

		Events.ungrabEvent += (grabber, obj, type) =>
        {
            if (obj.TryGetComponent<TagGrabbing>(out var grabbing))
			{
                Destroy(grabbing);
			}
            var tg = obj.AddComponent<TagGrabbed>();
            tg.type = type;
            switch (type)
            {
                case PlayerType.VR:
                    tg.collided = SetItFPS;
                    tg.mask = vrMask;
                    break;
                case PlayerType.FPS:
                    tg.collided = SetItVR;
                    tg.mask = fpsMask;
                    break;
            }
        };

        var l = new GameObject();
        light = l.AddComponent<Light>();
        light.type = LightType.Point;
        light.intensity = 3;

        var fpsTc = fps.AddComponent<TagCollider>();
        fpsTc.collided = SetItFPS;
        fpsTc.mask = fpsMask;
        
        var vrTc = vr.AddComponent<TagCollider>();
        vrTc.collided = SetItVR;
        vrTc.mask = vrMask;
    }

    public void SetIt(PlayerType type)
	{
        this.playerType = type;
        light.transform.SetParent(playerType == PlayerType.FPS ? fps.transform : vr.transform);
        light.transform.localPosition = Vector3.up * 3;
        light.color = playerType == PlayerType.FPS ? fpsColor : vrColor;
    }


	public void Trigger()
	{
        if (started)
            return;

        if (!vr.activeInHierarchy || !fps.activeInHierarchy)
        {
            Debug.Log("Need two players to play Tag");
            return;
        }

		var startPlayer = UnityEngine.Random.Range(0, 2);

		if (startPlayer == 0)
		{
			started = true;
			StartGame();
			SetIt(PlayerType.VR);
		}
		else
		{
			started = true;
			StartGame();
			SetIt(PlayerType.FPS);
		}
	}
}
