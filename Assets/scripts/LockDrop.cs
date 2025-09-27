using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class LockDrop : MonoBehaviour, IDropHandler
{
    [SerializeField] private string correctKeyName; // Nombre de la llave correcta
    public InteractableObject interactableObject = null;
    public GameObject puertasAbiertas;
    public GameObject puertasCerradas;
    public GameObject minijuegoUI;
    public GameObject InicioUI;
    public GameObject UIOcultar;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;

        if (dropped != null && dropped.name == correctKeyName)
        {
            Debug.Log("¡Ganaste! La llave correcta abrió el cerrojo.");
            StartCoroutine(RotateCorrect());
        }
        else
        {
            Debug.Log("Llave incorrecta. Intenta de nuevo.");
            StartCoroutine(Wobble());
        }
    }

    private IEnumerator RotateCorrect()
    {
        Quaternion startRot = rectTransform.rotation;
        Quaternion endRot = Quaternion.Euler(0, 0, -90); // Giro 90° en Z hacia la derecha
        float t = 0f;
        float duration = 0.5f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            rectTransform.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }
        interactableObject.TryInteract();
        interactableObject.interactuable = false;
        puertasAbiertas.SetActive(false);
        puertasCerradas.SetActive(true);
        InicioUI.SetActive(true);
        minijuegoUI.SetActive(false);
        UIOcultar.SetActive(false);
        rectTransform.rotation = startRot;
    }

    private IEnumerator Wobble()
    {
        float intensity = 10f; // grados de tambaleo
        float speed = 20f;     // qué tan rápido vibra
        float duration = 0.5f; // cuánto dura

        Quaternion originalRot = rectTransform.rotation;
        float time = 0f;

        while (time < duration)
        {
            float angle = Mathf.Sin(time * speed) * intensity;
            rectTransform.rotation = Quaternion.Euler(0, 0, angle);
            time += Time.deltaTime;
            yield return null;
        }

        rectTransform.rotation = originalRot; // vuelve al estado normal
    }
}
