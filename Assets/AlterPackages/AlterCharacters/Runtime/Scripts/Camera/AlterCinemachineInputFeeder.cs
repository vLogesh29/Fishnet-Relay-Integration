using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class AlterCinemachineInputFeeder : MonoBehaviour, AxisState.IInputAxisProvider
{
    [Tooltip("If set, Input Actions will be auto-enabled at start")]
    public bool AutoEnableInputs = true;

    [SerializeField] InputActionReference InputRef;
    //private void OnEnable()
    //{
    //    if (AutoEnableInputs)
    //        EnableInput();
    //}
    public void EnableInput()
    {
        if (InputRef == null)
            return;
        InputRef.action.Enable();
    }
    public void DisableInput()
    {
        if (InputRef == null)
            return;
        InputRef.action.Disable();
        lookInput = Vector2.zero;
    }
    private void OnDisable()
    {
        DisableInput();
    }

    Vector2 lookInput = Vector2.zero;
    void UpdateInput()
    {
        if (!InputRef.action.enabled)
            EnableInput();
        lookInput = InputRef.action.ReadValue<Vector2>();
    }

    [SerializeField] private Vector2 MouseSpeed = new Vector2(5, 10), TouchSpeed = new Vector2(25, 30);
    public void UpdateSpeed(Vector2 newInputSpeed)
    {
        MouseSpeed = newInputSpeed;
        TouchSpeed = newInputSpeed;
    }
    Vector2 Speed
    {
        get
        {
            if (Application.isMobilePlatform)
                return TouchSpeed;
            else
                return MouseSpeed;
        }
    }
    public float GetAxisValue(int axis)
    {
        UpdateInput();
        if (axis == 0) // x axis
        {
            return lookInput.x * Speed.x;
        }
        else if (axis == 1) // y axis
        {
            return lookInput.y * Speed.y;
        }
        else
        {
            return 0f;
        }
    }

    public void UpdateInputAxis(Vector3 delta)
    {
        // not used
    }

    public void Reset()
    {
        // not used
    }
}
