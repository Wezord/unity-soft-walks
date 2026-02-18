using System.Collections;
using UnityEngine;

public class FPSPlayer : GenericPlayer
{
    public float rotationSpeed = 1f;
    public Weapon weapon;

    private Vector2 rotations = Vector2.zero;

    protected override void Initialize()
    {
        base.Initialize();
        Cursor.lockState = CursorLockMode.Locked;
    }


    protected void Update()
    {
        Turn();
    }

    public override float GetHeight()
    {
        return playerHeight;
    }

    void Turn()
    {
        rotations += input.GetViewAxis() * rotationSpeed / 60f;

        float e = 0.001f;
        rotations.y = Mathf.Clamp(rotations.y, -90+e, 90-e);

        capsule.transform.localRotation = Quaternion.Euler(0, rotations.x, 0);
        camera.transform.localRotation = Quaternion.Euler(-rotations.y, 0, 0);

        /*
        if(weapon != null)
        {
            var weaponRotation = weapon.getRotation();
            var angle = Quaternion.Angle(camera.transform.rotation, weaponRotation);
            
            float t = 0.8f;
            camera.transform.rotation = Quaternion.Slerp(camera.transform.rotation, weaponRotation, t);
        }
        */
    }

}