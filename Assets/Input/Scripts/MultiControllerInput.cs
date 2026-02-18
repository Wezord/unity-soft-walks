using UnityEngine;
using UnityEngine.InputSystem;

public class MultiControllerInput : GenericPlayerInput
{
    public Gamepad gamepad;
    private bool isControllerActive = false;

    public override Vector2 GetMovementAxis()
    {
        if (!IsControllerValid()) return Vector2.zero;
        return gamepad.leftStick.ReadValue();
    }

    public override Vector2 GetViewAxis()
    {
        if (!IsControllerValid()) return Vector2.zero;
        return gamepad.rightStick.ReadValue();
    }

    public override bool GetJumpButton()
    {
        if (!IsControllerValid()) return false;
        return gamepad.aButton.isPressed;
    }

    public override bool GetPrimaryActionButton()
    {
        if (!IsControllerValid()) return false;
        return gamepad.rightTrigger.isPressed;
    }

    public override bool GetSecondaryActionButton()
    {
        if (!IsControllerValid()) return false;
        return gamepad.leftTrigger.isPressed;
    }

    public override bool GetInteractButton()
    {
        if (!IsControllerValid()) return false;
        return gamepad.xButton.isPressed;
    }

    public override bool GetWalkingButton()
    {
        if (!IsControllerValid()) return false;
        return gamepad.leftShoulder.isPressed;
    }

    public override float GetControlAxis()
    {
        if (!IsControllerValid()) return 0f;
        return gamepad.rightStick.ReadValue().y;
    }

    public override bool IsActive()
    {
        return IsControllerValid();
    }

    private bool IsControllerValid()
    {
        return isControllerActive && gamepad != null;
    }

    public void SetGamepad(Gamepad newGamepad)
    {
        if (newGamepad != null && newGamepad != gamepad)
        {
            print("newGamepad: " + newGamepad);
            gamepad = newGamepad;
            isControllerActive = true;
        }
    }
    
} 