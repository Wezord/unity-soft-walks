using UnityEngine;

[ExecuteAlways]
public class SpawnSetter : MonoBehaviour
{
    public bool setSpawn = false;
    private Replayer replayer;
    private ActionAgent agent;

    void Update()
    {
        if(setSpawn)
        {
			setSpawn = false;
            if (!replayer) replayer = GetComponent<Replayer>();
			if (!agent) agent = GetComponent<ActionAgent>();
			replayer.Play();
            replayer.Stop();
            agent.SetSpawn(replayer.player, replayer.leftHand, replayer.rightHand);
		}
    }
}
