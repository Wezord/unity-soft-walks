using UnityEngine;

public class BotArrowController : MonoBehaviour
{
    public GameObject prefab;
    public HumanBotParameters p;

	private Transform dummyTarget;

    void Start()
    {
		var arrowTarget = CreateArrow(p.root.transform);
		//var arrowSpeed = CreateArrow(p.root.transform);

		dummyTarget = new GameObject().transform;
		dummyTarget.transform.position = p.target.position;

		arrowTarget.target = dummyTarget;
		//arrowSpeed.speedBody = p.root.body;
		arrowTarget.color = Color.white;
		//arrowSpeed.color = Color.red;
		arrowTarget.horizontal = true;
		//arrowSpeed.horizontal = true;
		arrowTarget.distance = true;
		//arrowSpeed.distance = true;
		var horizontalCamera = GetComponent<HorizontalCamera>();
		if(horizontalCamera && horizontalCamera.isActiveAndEnabled)
		{
			arrowTarget.setHeight = true;
			arrowTarget.height = 0;
		}


		var controllers = p.GetComponents<DirectionController>();
		
		foreach(var controller in controllers)
		{
			var arrowLookTarget = CreateArrow(controller.pointer);
			//var arrowLookDirection = CreateArrow(controller.pointer);

			arrowLookTarget.target = controller.target;
			arrowLookTarget.color = Color.green;
			//arrowLookDirection.color = Color.red;
			if(controller.horizontal)
			{
				arrowLookTarget.horizontal = true;
				//arrowLookDirection.horizontal = true;
			}
		}

		var positionControllers = p.GetComponents<PositionController>();

		foreach(var controller in positionControllers)
		{
			var arrow = CreateArrow(controller.pointer);
			arrow.target = controller.target;
			arrow.color = Color.white;
			arrow.distance = true;
		}
		
	}

	private void LateUpdate()
	{
		dummyTarget.transform.position = p.root.transform.position + (p.target.position - p.root.transform.position).normalized * p.targetSpeed/2f;
	}

	private ArrowController CreateArrow(Transform origin)
    {
		var arrowSpeed = gameObject.AddComponent<ArrowController>();
		arrowSpeed.prefab = prefab;
		arrowSpeed.origin = origin;
		return arrowSpeed;
	}
}
