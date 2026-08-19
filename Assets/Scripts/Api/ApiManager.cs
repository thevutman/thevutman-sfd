using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Centraliza todas las llamadas HTTP del proyecto:
///  1) La API falsa (my-json-server sobre el db.json del repo de GitHub)
///     para obtener los jugadores y su baraja de IDs de cartas.
///  2) La API de terceros (Deck of Cards API) para resolver esos IDs a
///     cartas de póker reales (valor, palo e imagen) y mostrarlas.
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

    [Header("API de terceros (Deck of Cards API)")]
    [SerializeField]
    private string thirdPartyApiBaseUrl = "https://deckofcardsapi.com/api/deck/";

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
    /// Resuelve TODOS los IDs de la baraja de un jugador consultando la
    /// Deck of Cards API. La API real necesita DOS llamadas encadenadas:
    ///   1) crear un mazo nuevo que contenga exactamente esas cartas
    ///      (en ese orden), sin barajar:
    ///      GET https://deckofcardsapi.com/api/deck/new/?cards=AS,2H,KD
    ///   2) "extraer" (draw) esas mismas cartas del mazo recién creado
    ///      para obtener el detalle completo (value, suit, image):
    ///      GET https://deckofcardsapi.com/api/deck/{deck_id}/draw/?count=3
    /// (El primer endpoint solo confirma que el mazo se creó; no trae el
    /// detalle de cada carta — por eso hace falta el segundo paso.)
    /// </summary>
    public IEnumerator GetCardsByIds(int[] cardIds, Action<CardData[]> onSuccess, Action<string> onError)
    {
        if (cardIds == null || cardIds.Length == 0)
        {
            onSuccess?.Invoke(Array.Empty<CardData>());
            yield break;
        }

        string codes = string.Join(",", cardIds.Select(CardCodeMapper.IdToCode));

        // Paso 1: crear el mazo parcial con exactamente esas cartas.
        string newDeckUrl = $"{thirdPartyApiBaseUrl}new/?cards={codes}";
        string deckId;

        using (UnityWebRequest newDeckRequest = UnityWebRequest.Get(newDeckUrl))
        {
            yield return newDeckRequest.SendWebRequest();

            if (newDeckRequest.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Error consultando {newDeckUrl}: {newDeckRequest.error}");
                yield break;
            }

            NewDeckResponse newDeck;
            try
            {
                newDeck = JsonUtility.FromJson<NewDeckResponse>(newDeckRequest.downloadHandler.text);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Error parseando la creación del mazo: {e.Message}");
                yield break;
            }

            if (newDeck == null || !newDeck.success)
            {
                onError?.Invoke("La Deck of Cards API respondió success = false al crear el mazo.");
                yield break;
            }

            deckId = newDeck.deck_id;
        }

        // Paso 2: extraer (draw) esas cartas ya creadas para obtener su detalle.
        string drawUrl = $"{thirdPartyApiBaseUrl}{deckId}/draw/?count={cardIds.Length}";

        using (UnityWebRequest drawRequest = UnityWebRequest.Get(drawUrl))
        {
            yield return drawRequest.SendWebRequest();

            if (drawRequest.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Error consultando {drawUrl}: {drawRequest.error}");
                yield break;
            }

            DrawCardsResponse response;
            try
            {
                response = JsonUtility.FromJson<DrawCardsResponse>(drawRequest.downloadHandler.text);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Error parseando la baraja: {e.Message}");
                yield break;
            }

            if (response == null || !response.success)
            {
                onError?.Invoke("La Deck of Cards API respondió success = false al extraer las cartas.");
                yield break;
            }

            onSuccess?.Invoke(response.cards);
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
