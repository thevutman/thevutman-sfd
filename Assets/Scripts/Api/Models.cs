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
/// Respuesta del endpoint que crea un mazo nuevo (parcial o completo).
/// GET https://deckofcardsapi.com/api/deck/new/?cards=AS,2H,KD
/// No trae el detalle de cada carta, solo confirma que el mazo se creó.
/// </summary>
[Serializable]
public class NewDeckResponse
{
    public bool success;
    public string deck_id;
    public bool shuffled;
    public int remaining;
}

/// <summary>
/// Respuesta del endpoint que "extrae" (draw) cartas de un mazo ya creado.
/// GET https://deckofcardsapi.com/api/deck/{deck_id}/draw/?count=3
/// Este sí trae el detalle completo (value, suit, image) de cada carta.
/// </summary>
[Serializable]
public class DrawCardsResponse
{
    public bool success;
    public string deck_id;
    public CardData[] cards;
    public int remaining;
}

/// <summary>
/// Una carta individual de la baraja de póker devuelta por la Deck of Cards API.
/// value: "ACE", "2".."10", "JACK", "QUEEN", "KING"
/// suit:  "SPADES", "HEARTS", "DIAMONDS", "CLUBS"
/// </summary>
[Serializable]
public class CardData
{
    public string code;
    public string image;
    public CardImages images;
    public string value;
    public string suit;
}

[Serializable]
public class CardImages
{
    public string svg;
    public string png;
}
