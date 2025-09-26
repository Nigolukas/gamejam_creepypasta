using UnityEngine;
using System.Collections.Generic;

public class CableGameManager : MonoBehaviour
{
    public LineRenderer linePrefab; // Prefab de un LineRenderer configurado
    private CableConnector firstSelected = null;
    private int conectados = 0;
    public GameObject sensor;
    public GameObject player;
    public GameObject particulas;
    public InteractableObject interactableObject;

    private List<LineRenderer> activeLines = new List<LineRenderer>();

    public void CableClicked(CableConnector cable)
    {
        if (firstSelected == null)
        {
            // Primer click → guardamos
            firstSelected = cable;
        }
        else
        {
            // Segundo click → comprobamos si coinciden
            if (firstSelected.cableColor == cable.cableColor && firstSelected != cable)
            {
                // Crear la línea
                CreateConnection(firstSelected, cable, cable.cableMaterial);
            }
            else
            {
                Debug.Log(" Los colores no coinciden!");
            }

            // Reiniciamos la selección
            firstSelected = null;
        }
    }

    void CreateConnection(CableConnector a, CableConnector b, Material mat)
    {
        LineRenderer newLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        newLine.positionCount = 2;
        newLine.SetPosition(0, a.transform.position);
        newLine.SetPosition(1, b.transform.position);

        // Crear una copia del material para que no afecte otras líneas
        newLine.material = new Material(mat);
        newLine.textureMode = LineTextureMode.Tile;
        // Ajustar la orientación de la textura según la dirección de la línea
        if (Vector3.Dot(a.transform.right, b.transform.position - a.transform.position) < 0)
        {
            newLine.material.mainTextureScale = new Vector2(-1, 1);
        }
        else
        {
            newLine.material.mainTextureScale = new Vector2(1, 1);
        }

        activeLines.Add(newLine);
        Debug.Log(" Conectados " + a.cableColor);
        conectados++;
        if(conectados == 4)
        {
            player.SetActive(true);
            sensor.SetActive(true);
            particulas.SetActive(false);
            interactableObject.TryInteract();
        }
    }

}
