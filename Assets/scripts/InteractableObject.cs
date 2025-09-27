using UnityEngine;
using UnityEngine.InputSystem; // Nuevo Input System

public class InteractableObject : MonoBehaviour
{
    [Header("C�maras")]
    public Camera playerCamera;   // C�mara del jugador
    public Camera fixedCamera;    // C�mara fija en la escena

    [Header("UI")]
    public GameObject interactionUI; // Texto "Pulsa E para interactuar"
    public GameObject exitUI;        // Texto "Pulsa E para salir"

    [Header("Interacci�n")]
    public float interactionDistance = 3f;
    public GameObject playerCapsule;
    public bool interactuable = true;

    private Transform player;
    private bool isUsingFixedCamera = false;
    private bool isNear = false;

    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        // Encuentra al jugador con tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Activar solo la c�mara del jugador al inicio
        playerCamera.enabled = true;
        if (fixedCamera != null)
            fixedCamera.enabled = false;

        if (interactionUI != null) interactionUI.SetActive(false);
        if (exitUI != null) exitUI.SetActive(false);

        // Suscribirse al evento de Interact
        inputActions.Player.Interact.performed += ctx => TryInteract();
    }

    void Update()
    {
        

        // Verificar si el jugador est� cerca del objeto
        float distance = Vector3.Distance(player.position, transform.position);
        isNear = distance <= interactionDistance;

        // Mostrar/ocultar el letrero de interacci�n (solo si no estamos en c�mara fija)
        if(interactuable == true)
        {
            if (interactionUI != null)
                interactionUI.SetActive(isNear && !isUsingFixedCamera);
        }

    }

    public void TryInteract()
    {
        if (interactuable == true)
        {
            if (isNear || isUsingFixedCamera) // Permite salir aunque no est�s cerca
            {
                ToggleCamera();
            }
        }

    }

    void ToggleCamera()
    {
        
        isUsingFixedCamera = !isUsingFixedCamera;

        if (fixedCamera != null)
        {
            playerCamera.enabled = !isUsingFixedCamera;
            fixedCamera.enabled = isUsingFixedCamera;
        }
        

        // Mostrar/ocultar UI
        if (interactionUI != null) interactionUI.SetActive(!isUsingFixedCamera && isNear);
        if (exitUI != null) exitUI.SetActive(isUsingFixedCamera);

        // Mostrar/ocultar c�psula del jugador y cursor
        if (playerCapsule != null) playerCapsule.SetActive(!isUsingFixedCamera);
        if (isUsingFixedCamera)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (interactuable == false)
        {
            interactionUI.SetActive(false);
        }

    }
}
