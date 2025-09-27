using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CableColorData
{
    public string colorName;
    public Material colorMaterial;
}

[System.Serializable]
public class ColorCount
{
    public string colorName;
    public Material colorMaterial;
    public int count = 1;
}

public class CableGameManager : MonoBehaviour
{
    public LineRenderer linePrefab;
    private CableConnector firstSelected = null;
    private int conectados = 0;
    public GameObject sensor;
    public GameObject player;
    public GameObject particulas;
    public InteractableObject interactableObject;
    public GameObject UIOcultar;

    private List<LineRenderer> activeLines = new List<LineRenderer>();

    [Header("Connectors & Colors")]
    public List<CableConnector> conectores = new List<CableConnector>();
    public List<CableColorData> coloresDisponibles = new List<CableColorData>();
    [Tooltip("Opcional: usar para fijar cuántas veces aparece cada color (si está vacío se usa coloresDisponibles)")]
    public List<ColorCount> colorCounts = new List<ColorCount>();

    // Llama a esto cuando inicie/reinicie el minijuego
    public void IniciarMinijuego()
    {
        // limpiar estado previo
        ClearLines();
        conectados = 0;
        firstSelected = null;

        // asignar colores
        AssignColorsToConnectors();
    }

    void AssignColorsToConnectors()
    {
        int n = conectores.Count;
        if (n == 0)
        {
            Debug.LogWarning("No hay conectores en la lista!");
            return;
        }

        List<CableColorData> pool = new List<CableColorData>();

        // 1) Si el usuario definió colorCounts, construir pool con las cantidades exactas
        if (colorCounts != null && colorCounts.Count > 0)
        {
            int total = 0;
            foreach (var cc in colorCounts) total += Mathf.Max(0, cc.count);

            // Construir pool
            foreach (var cc in colorCounts)
            {
                int times = Mathf.Max(0, cc.count);
                for (int i = 0; i < times; i++)
                {
                    pool.Add(new CableColorData { colorName = cc.colorName, colorMaterial = cc.colorMaterial });
                }
            }

            // Si la suma no coincide con el número de conectores, adaptar:
            if (pool.Count < n)
            {
                // rellenar ciclando coloresDisponibles si hay
                if (coloresDisponibles != null && coloresDisponibles.Count > 0)
                {
                    int idx = 0;
                    while (pool.Count < n)
                    {
                        var c = coloresDisponibles[idx % coloresDisponibles.Count];
                        pool.Add(new CableColorData { colorName = c.colorName, colorMaterial = c.colorMaterial });
                        idx++;
                    }
                }
                else
                {
                    Debug.LogWarning("colorCounts suma menos que conectores y no hay coloresDisponibles para rellenar.");
                    // rellenamos con el último colorCounts repetido
                    var last = colorCounts[colorCounts.Count - 1];
                    while (pool.Count < n)
                        pool.Add(new CableColorData { colorName = last.colorName, colorMaterial = last.colorMaterial });
                }
            }
            else if (pool.Count > n)
            {
                // si sobra, recortamos
                pool.RemoveRange(n, pool.Count - n);
            }
        }
        else
        {
            // 2) No hay colorCounts → usar coloresDisponibles
            if (coloresDisponibles == null || coloresDisponibles.Count == 0)
            {
                Debug.LogError("No hay coloresDisponibles y no se especificaron colorCounts.");
                return;
            }

            if (coloresDisponibles.Count >= n)
            {
                // tomar un subconjunto único (sin repetición): mezclar y tomar primeros n
                pool.AddRange(coloresDisponibles);
                Shuffle(pool);
                if (pool.Count > n) pool.RemoveRange(n, pool.Count - n);
            }
            else
            {
                // menos colores que conectores: rellenamos ciclando los colores disponibles
                int i = 0;
                while (pool.Count < n)
                {
                    var c = coloresDisponibles[i % coloresDisponibles.Count];
                    pool.Add(new CableColorData { colorName = c.colorName, colorMaterial = c.colorMaterial });
                    i++;
                }
            }
        }

        // mezclar la pool antes de asignar
        Shuffle(pool);

        // asignar a conectores
        for (int i = 0; i < n; i++)
        {
            var conn = conectores[i];
            var data = pool[i];
            conn.cableColor = data.colorName;
            conn.cableMaterial = data.colorMaterial;

            // si tienen renderer, actualizar material para visual
            var rend = conn.GetComponent<Renderer>();
            if (rend != null) rend.material = data.colorMaterial;
        }

        // DEBUG: imprimir conteo por color para verificar distribucion
        var counts = new Dictionary<string, int>();
        foreach (var d in pool)
        {
            if (!counts.ContainsKey(d.colorName)) counts[d.colorName] = 0;
            counts[d.colorName]++;
        }
        string msg = "Asignación colores: ";
        foreach (var kv in counts) msg += $"{kv.Key}={kv.Value} ";
        Debug.Log(msg);
    }

    // Fisher-Yates
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            int r = Random.Range(i, list.Count); // correct: [i..count-1]
            T tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    void ClearLines()
    {
        foreach (var line in activeLines)
        {
            if (line != null) Destroy(line.gameObject);
        }
        activeLines.Clear();
    }
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
        if (conectados == 4)
        {
            player.SetActive(true);
            sensor.SetActive(true);
            particulas.SetActive(false);
            interactableObject.TryInteract();
            interactableObject.interactuable = false;
            UIOcultar.SetActive (false);
            conectados = 0;

            // 🔥 Destruir las líneas creadas
            foreach (var line in activeLines)
            {
                if (line != null)
                    Destroy(line.gameObject);
            }
            IniciarMinijuego();
            activeLines.Clear();
        }

    }

}
