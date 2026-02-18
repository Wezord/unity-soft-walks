using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptedControl : MonoBehaviour
{

    public HumanBotParameters p;

    public Vector2[] walkDirections;
	public Vector2[] rootDirections;

	public int arcFrames = 400;

    private DirectionController rootDir;

    private int frameCounter = 0;
    private int walkIndex = 0;
	private int rootIndex = 0;
     

	void Awake()
    {
        rootDir = p.GetComponent<DirectionController>();
        p.training = false;
        Destroy(p.targetMover);
        var rootMover = rootDir.target.GetComponent<TargetMover>();
        if(rootMover)
		    Destroy(rootMover);
    }

    void FixedUpdate()
    {
        frameCounter++;
        if(frameCounter>arcFrames)
        {
            frameCounter = 1;
            walkIndex = (walkIndex + 1) % walkDirections.Length;
			rootIndex = (rootIndex + 1) % rootDirections.Length;
        }

        p.target.position = p.root.transform.position + new Vector3(walkDirections[walkIndex].x, 0, walkDirections[walkIndex].y);
		rootDir.target.position = rootDir.targetCenter.position + new Vector3(rootDirections[rootIndex].x, 0, rootDirections[rootIndex].y);
	}
}
