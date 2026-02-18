using UnityEngine;
using UnityEngine.Assertions;

public class HumanBotController : MonoBehaviour
{

	public new Camera camera;
    public HumanBotParameters p;

	private GenericPlayerInput input;

    private Transform target;

    private float speed;

	void Awake()
    {
        if (!isActiveAndEnabled) return;

        if(!p) p = GetComponent<HumanBotParameters>();
        Assert.IsNotNull(p, "HumanBotController: HumanBotParameters not found");

		if (!camera) camera = GetComponent<Camera>();
        if (!camera) {
            camera = Camera.main;
        }
		Assert.IsNotNull(camera, "HumanBotController: Camera not found");
        
		input = GenericPlayerInput.GetInput(gameObject);
        var targ = new GameObject();
        target = targ.transform;
		p.target = target;

        speed = p.targetSpeed;

        p.training = false;
        //p.maxDyingFrames = 100;
        p.breakForce = Mathf.Infinity;
        p.breakTorque = Mathf.Infinity;

        var controllers = p.GetComponents<DirectionController>();
        foreach(var controller in controllers)
        {
			controller.target = camera.transform;
		}

	}

    void FixedUpdate()
    {
        var moveAxis = input.GetMovementAxis();
        if(moveAxis.magnitude == 0f)
        {
            p.targetSpeed = 0f;
            return;
        } else if(input.GetWalkingButton())
        {
			p.targetSpeed = 4f;
		} else
        {
            p.targetSpeed = 1.36f;
        }

        var axis = camera.transform.forward;
        axis.y = 0;
        var normal = axis.normalized;
        Vector3 tangent = new Vector3(normal.z, 0, -normal.x);
		Vector3 axe = (moveAxis.x * tangent + moveAxis.y * normal).normalized;
		target.position = p.root.transform.position + axe * 10f;
	}
}
