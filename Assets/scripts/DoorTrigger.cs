using UnityEngine;

public class DoubleSlidingDoor : MonoBehaviour
{
    public Transform puerta1;
    public Transform puerta2;
    public float triggerDistance = 5f;
    public float openOffset = 1f; // cuánto se mueve cada puerta
    public float openSpeed = 2f;

    private Transform player;
    private Vector3 puerta1Closed;
    private Vector3 puerta1Open;
    private Vector3 puerta2Closed;
    private Vector3 puerta2Open;

    void Start()
    {
        // Guarda posiciones iniciales
        puerta1Closed = puerta1.position;
        puerta2Closed = puerta2.position;

        puerta1Open = puerta1Closed + new Vector3(openOffset, 0, 0);
        puerta2Open = puerta2Closed + new Vector3(-openOffset, 0, 0);

        // Busca automáticamente al jugador con tag "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("No se encontró un objeto con tag 'Player'");
    }

    void Update()
    {
        if (player == null) return;

        // Calcula distancia desde el jugador a la puerta1
        float distance = Vector3.Distance(player.position, puerta1.position);

        if (distance <= triggerDistance)
        {
            puerta1.position = Vector3.Lerp(puerta1.position, puerta1Open, Time.deltaTime * openSpeed);
            puerta2.position = Vector3.Lerp(puerta2.position, puerta2Open, Time.deltaTime * openSpeed);
        }
        else
        {
            puerta1.position = Vector3.Lerp(puerta1.position, puerta1Closed, Time.deltaTime * openSpeed);
            puerta2.position = Vector3.Lerp(puerta2.position, puerta2Closed, Time.deltaTime * openSpeed);
        }
    }

}
