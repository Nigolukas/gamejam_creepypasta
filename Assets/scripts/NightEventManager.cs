using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NightEvent
{
    [Header("Camera Settings")]
    public RawImage cameraFeed;          // UI (RawImage en el Canvas)
    public Texture normalTexture;        // Textura normal (RenderTexture real)
    public Texture eventTexture;         // Textura evento (ruido, video, etc.)

    [Header("3D Plane in World")]
    public Renderer worldScreenRenderer; // Plano en el entorno 3D (el que usa la misma RenderTexture)

    [Header("Objects to Toggle")]
    public GameObject[] activateObjects;
    public GameObject[] deactivateObjects;

    [Header("Interactable")]
    public InteractableObject interactable;
}


public class NightEventManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float realMinutesPerGameHour = 2f; // 1 hora = 2 minutos reales
    private float currentTime; // 20 = 8PM
    private bool nightActive = true;

    [Header("UI")]
    public TextMeshProUGUI clockText;

    [Header("Events")]
    public List<NightEvent> nightEvents = new List<NightEvent>();

    [Header("Event Settings")]
    public float eventCheckInterval = 3f; // cada cuántos segundos se tiran los dados
    private float eventTimer = 0f;        // acumulador de tiempo

    private NightEvent activeEvent = null; // referencia al evento activo

    private void Start()
    {
        currentTime = 20f; // 8PM
        StartCoroutine(TimeRoutine());
    }

    IEnumerator TimeRoutine()
    {
        while (nightActive)
        {
            // avanzar tiempo
            currentTime += (1f / realMinutesPerGameHour) * (Time.deltaTime / 60f);

            if (currentTime >= 30f) // 6AM
            {
                currentTime = 30f;
                nightActive = false;
                Debug.Log("La noche terminó.");
                yield break;
            }

            UpdateClockUI();

            // si hay un evento activo, revisar si ya se solucionó
            if (activeEvent != null)
            {
                if (activeEvent.interactable != null && activeEvent.interactable.interactuable == false)
                {
                    // restaurar cámara cuando el evento termina
                    ResetEvent(activeEvent);
                    Debug.Log("Evento solucionado en " + activeEvent.cameraFeed.name);
                    activeEvent = null;
                }
            }
            else
            {
                // acumular tiempo para tirar los dados
                eventTimer += Time.deltaTime;
                if (eventTimer >= eventCheckInterval)
                {
                    eventTimer = 0f;

                    if (Random.Range(0f, 1f) < GetEventProbability())
                    {
                        TriggerRandomEvent();
                    }
                }
            }

            yield return null;
        }
    }

    void UpdateClockUI()
    {
        int hour = Mathf.FloorToInt(currentTime) % 24;
        int minute = Mathf.FloorToInt((currentTime - Mathf.Floor(currentTime)) * 60);

        string ampm = hour >= 12 ? "PM" : "AM";
        int displayHour = (hour > 12) ? hour - 12 : hour;
        if (displayHour == 0) displayHour = 12;

        clockText.text = $"{displayHour:00}:{minute:00} {ampm}";
    }

    float GetEventProbability()
    {
        if (currentTime < 22) return 0.01f; // 8PM–10PM
        if (currentTime < 26) return 0.05f; // 10PM–2AM
        if (currentTime < 28) return 0.1f;  // 2AM–4AM
        return 0.2f; // 4AM–6AM
    }

    void TriggerRandomEvent()
    {
        if (nightEvents.Count == 0) return;

        int index = Random.Range(0, nightEvents.Count);
        NightEvent ev = nightEvents[index];

        Debug.Log("Evento aleatorio activado en " + ev.cameraFeed.name);

        activeEvent = ev; // marcar evento como activo

        // 1. Cambiar la cámara en UI
        if (ev.cameraFeed != null && ev.eventTexture != null)
            ev.cameraFeed.texture = ev.eventTexture;

        // 1b. Cambiar la textura en el plano del mundo 3D
        if (ev.worldScreenRenderer != null && ev.eventTexture != null)
            ev.worldScreenRenderer.material.mainTexture = ev.eventTexture;

        // 2. Activar objetos
        foreach (var obj in ev.activateObjects)
            if (obj != null) obj.SetActive(true);

        // 3. Desactivar objetos
        foreach (var obj in ev.deactivateObjects)
            if (obj != null) obj.SetActive(false);

        // 4. Habilitar el interactuable
        if (ev.interactable != null)
            ev.interactable.interactuable = true;
    }

    public void ResetEvent(NightEvent ev)
    {
        if (ev.cameraFeed != null && ev.normalTexture != null)
            ev.cameraFeed.texture = ev.normalTexture;

        if (ev.worldScreenRenderer != null && ev.normalTexture != null)
            ev.worldScreenRenderer.material.mainTexture = ev.normalTexture;

        // restaurar objetos (opcional, si quieres)
        foreach (var obj in ev.activateObjects)
            if (obj != null) obj.SetActive(false);

        foreach (var obj in ev.deactivateObjects)
            if (obj != null) obj.SetActive(true);
    }
}
