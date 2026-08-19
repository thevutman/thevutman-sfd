using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Centraliza todas las llamadas HTTP del proyecto:
///  1) La API falsa (my-json-server sobre el db.json del repo de GitHub)
///     para obtener los jugadores y su baraja de IDs de cartas.
///  2) La API de terceros (Rick and Morty API) para resolver esos IDs a
///     "cartas" (cada una es un personaje, con nombre e imagen) y
///     mostrarlas.
///
/// IMPORTANTE: reemplaza fakeApiBaseUrl con la URL de TU repositorio si
/// cambias de usuario/nombre de repo en GitHub.
/// Formato: https://my-json-server.typicode.com/{usuario-github}/{repo}
/// </summary>
public class ApiManager : MonoBehaviour
{
    [Header("Fake API (my-json-server + db.json en GitHub)")]
    [SerializeField]
    private string fakeApiBaseUrl = "https://my-json-server.typicode.com/thevutman/thevutman-sfd";

    [Header("API de terceros (Rick and Morty API)")]
    [SerializeField]
    private string thirdPartyApiBaseUrl = "https://rickandmortyapi.com/api/character/";

    /// <summary>
    /// Trae el listado completo de jugadores (cada uno con su nombre y su
    /// arreglo de IDs de cartas) desde la API falsa.
    /// GET https://my-json-server.typicode.com/{usuario}/{repo}/players
    /// </summary>
    public IEnumerator GetPlayers(Action<PlayerData[]> onSuccess, Action<string> onError)
    {
        string url = $"{fakeApiBaseUrl}/players";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Error consultando {url}: {request.error}");
                yield break;
            }

            try
            {
                PlayerData[] players = JsonHelper.FromJson<PlayerData>(request.downloadHandler.text);
                onSuccess?.Invoke(players);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Error parseando la respuesta de jugadores: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Resuelve TODOS los IDs de la baraja de un jugador en una sola
    /// llamada a la Rick and Morty API, pidiendo varios IDs separados por
    /// coma (soportado nativamente por esa API):
    /// GET https://rickandmortyapi.com/api/character/1,5,12,23,37
    ///
    /// Detalle importante: si se pide UN solo ID, esa API responde con un
    /// objeto plano (no un arreglo). Por eso se detecta el caso y se
    /// envuelve en un arreglo de un elemento antes de seguir.
    /// </summary>
    public IEnumerator GetCardsByIds(int[] cardIds, Action<CardData[]> onSuccess, Action<string> onError)
    {
        if (cardIds == null || cardIds.Length == 0)
        {
            onSuccess?.Invoke(Array.Empty<CardData>());
            yield break;
        }

        string ids = string.Join(",", cardIds);
        string url = $"{thirdPartyApiBaseUrl}{ids}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Error consultando {url}: {request.error}");
                yield break;
            }

            string json = request.downloadHandler.text.TrimStart();

            try
            {
                CardData[] cards = json.StartsWith("[")
                    ? JsonHelper.FromJson<CardData>(json)
                    : new[] { JsonUtility.FromJson<CardData>(json) };

                onSuccess?.Invoke(cards);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Error parseando la baraja: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Descarga una imagen (URL de sprite) y la devuelve como Sprite,
    /// lista para asignar a un componente Image de la UI.
    /// </summary>
    public IEnumerator GetSprite(string imageUrl, Action<Sprite> onSuccess, Action<string> onError)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            onError?.Invoke("URL de imagen vacía.");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Error descargando imagen {imageUrl}: {request.error}");
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
            onSuccess?.Invoke(sprite);
        }
    }
}
