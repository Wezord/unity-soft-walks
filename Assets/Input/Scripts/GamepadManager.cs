using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadManager : MonoBehaviour
{
    public GameObject prefabToSpawn;
    private HashSet<Gamepad> spawnedGamepads = new HashSet<Gamepad>();

    void Update()
    {   
        foreach (var gamepad in Gamepad.all)
        {
            if (!spawnedGamepads.Contains(gamepad) && AnyButtonPressed(gamepad))
            {
                SpawnForGamepad(gamepad);
            }
        }
    }

    private bool AnyButtonPressed(Gamepad gamepad)
    {
        // Check common buttons
        return gamepad.buttonSouth.wasPressedThisFrame ||
               gamepad.buttonNorth.wasPressedThisFrame ||
               gamepad.buttonEast.wasPressedThisFrame ||
               gamepad.buttonWest.wasPressedThisFrame ||
               gamepad.leftShoulder.wasPressedThisFrame ||
               gamepad.rightShoulder.wasPressedThisFrame ||
               gamepad.leftTrigger.wasPressedThisFrame ||
               gamepad.rightTrigger.wasPressedThisFrame ||
               gamepad.startButton.wasPressedThisFrame ||
               gamepad.selectButton.wasPressedThisFrame ||
               gamepad.leftStickButton.wasPressedThisFrame ||
               gamepad.rightStickButton.wasPressedThisFrame ||
               gamepad.dpad.up.wasPressedThisFrame ||
               gamepad.dpad.down.wasPressedThisFrame ||
               gamepad.dpad.left.wasPressedThisFrame ||
               gamepad.dpad.right.wasPressedThisFrame;
    }

    private void SpawnForGamepad(Gamepad gamepad)
    {
        if (prefabToSpawn != null)
        {
            var go = Instantiate(prefabToSpawn, transform.position, transform.rotation, transform);
            go.GetComponent<MultiControllerInput>().SetGamepad(gamepad);
        }
        spawnedGamepads.Add(gamepad);
    }

    void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        print(device.GetType());
        Debug.Log($"{change}: {device.displayName}");
    }
} 