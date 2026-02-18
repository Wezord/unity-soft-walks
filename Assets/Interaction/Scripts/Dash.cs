using UnityEngine;

public class Dash : MonoBehaviour
{
    private GenericPlayerInput input;
    public Transform axis;
    public float distance;
    public float thickness;
    public GenericPlayer player;

	//public AK.Wwise.Event soundEvent;

	private LayerMask mask;
    void Start()
    {
        input = GenericPlayerInput.GetInput(gameObject);
        mask = ~LayerMask.GetMask("VRPlayer", "FPSPlayer", "LeftHand", "RightHand", "FPSWeapon", "Ungrabbable");
    }

    private bool dashing = false;

    void FixedUpdate()
    {
        if(player.standing && !dashing && input.GetSecondaryActionButton())
        {
            Vector2 moveAxis = input.GetMovementAxis();

            Vector3 direction = player.movementAxis.forward;
            Vector3 normal = new Vector3(direction.x, 0, direction.z).normalized;
            Vector3 tangent = new Vector3(normal.z, 0, -normal.x);
            Vector3 axe = (moveAxis.x * tangent + moveAxis.y * normal).normalized;

            if (Physics.SphereCast(player.camera.transform.position, thickness, axe, out var raycastHit, distance, mask))
            {
                transform.position = raycastHit.point;
            }
            else
            {
                transform.position += axe * distance;
            }

            //soundEvent.Post(gameObject);

		}

        dashing = input.GetSecondaryActionButton();
        
    }
}
