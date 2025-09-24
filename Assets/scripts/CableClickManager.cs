using UnityEngine;
using UnityEngine.InputSystem;

public class CableClickManager : MonoBehaviour
{
    public Camera maainCamera;

    private PlayerInputActions inputActions;
    private CableGameManager gameManager;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        gameManager = FindFirstObjectByType<CableGameManager>();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Click.performed += ctx => OnClick();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void OnClick()
    {
        Ray ray = maainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            CableConnector connector = hit.collider.GetComponent<CableConnector>();
            if (connector != null)
            {
                Debug.Log(" Clickeado en " + connector.cableColor);
                gameManager.CableClicked(connector);
            }
        }
    }
}
