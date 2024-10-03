using UnityEngine;
using System;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public event Action<Vector2, InputType> OnInputBegan;
    public event Action<Vector2, InputType> OnInputMoved;
    public event Action<Vector2, InputType> OnInputEnded;

    public enum InputType { Mouse, Touch }

    private void Awake()
    {
        // Singleton pattern to ensure a single instance
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // Handle touch inputs
        if (Input.touchSupported)
        {
            foreach (Touch touch in Input.touches)
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnInputBegan?.Invoke(worldPos, InputType.Touch);
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        OnInputMoved?.Invoke(worldPos, InputType.Touch);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        OnInputEnded?.Invoke(worldPos, InputType.Touch);
                        break;
                }
            }
        }
        else
        {
            // Handle mouse inputs
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                OnInputBegan?.Invoke(worldPos, InputType.Mouse);
            }
            if (Input.GetMouseButton(0))
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                OnInputMoved?.Invoke(worldPos, InputType.Mouse);
            }
            if (Input.GetMouseButtonUp(0))
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                OnInputEnded?.Invoke(worldPos, InputType.Mouse);
            }
        }
    }
}
