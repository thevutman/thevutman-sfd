using System;

/// <summary>
/// Representa un jugador tal como llega desde la API falsa
/// (https://my-json-server.typicode.com/&lt;usuario&gt;/&lt;repo&gt;/players).
/// "cards" es el listado de IDs que forman la "baraja" de ese jugador.
/// </summary>
[Serializable]
public class PlayerData
{
    public int id;
    public string name;
    public int[] cards;
}

/// <summary>
/// Subconjunto de campos que nos interesan de la respuesta de la Rick and
/// Morty API (https://rickandmortyapi.com/api/character/{id}). Cada
/// "carta" de la baraja es en realidad un personaje: se muestra su nombre
/// y su imagen. JsonUtility ignora silenciosamente el resto de los campos
/// que no declaremos aquí (status, species, episodios, etc.).
/// </summary>
[Serializable]
public class CardData
{
    public int id;
    public string name;
    public string image;
}
