using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Orquesta la pantalla principal:
///  1) Al iniciar, consulta la API falsa y llena el selector de jugadores.
///  2) Al elegir un jugador (dropdown), muestra su nombre y reconstruye
///     la grilla de cartas consultando la API de terceros (una sola
///     llamada con todos los IDs de la baraja de ese jugador).
///
/// Requiere que le asignes en el Inspector: apiManager, playerDropdown,
/// userNameText, statusText, cardsContainer y cardPrefab.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ApiManager apiManager;
    [SerializeField] private TMP_Dropdown playerDropdown;
    [SerializeField] private TMP_Text userNameText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Transform cardsContainer;
    [SerializeField] private CardUI cardPrefab;

    [Header("Datos del proyecto")]
    [Tooltip("Se muestra en el encabezado de la pantalla, tal como pide la actividad.")]
    [SerializeField] private TMP_Text projectAuthorText;
    [SerializeField] private string studentFullName = "Santiago Velasco";

    private PlayerData[] players;

    private void Start()
    {
        if (projectAuthorText != null)
        {
            projectAuthorText.text = $"Actividad 2 - {studentFullName}";
        }

        if (playerDropdown != null)
        {
            playerDropdown.onValueChanged.AddListener(OnPlayerSelected);
        }

        SetStatus("Cargando jugadores...");
        StartCoroutine(apiManager.GetPlayers(OnPlayersLoaded, OnError));
    }

    private void OnPlayersLoaded(PlayerData[] loadedPlayers)
    {
        players = loadedPlayers;

        if (players == null || players.Length == 0)
        {
            SetStatus("La API no devolvió jugadores.");
            return;
        }

        if (playerDropdown != null)
        {
            playerDropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (PlayerData player in players)
            {
                options.Add(player.name);
            }
            playerDropdown.AddOptions(options);
        }

        SetStatus(string.Empty);
        OnPlayerSelected(0);
    }

    /// <summary>
    /// Mecanismo para "intercambiar de usuario": se dispara cada vez que
    /// cambia la selección del dropdown y vuelve a construir la baraja.
    /// </summary>
    public void OnPlayerSelected(int index)
    {
        if (players == null || index < 0 || index >= players.Length) return;

        PlayerData selectedPlayer = players[index];

        if (userNameText != null)
        {
            userNameText.text = $"Jugador: {selectedPlayer.name}";
        }

        LoadDeckForPlayer(selectedPlayer);
    }

    private void LoadDeckForPlayer(PlayerData player)
    {
        ClearCards();

        if (player.cards == null || player.cards.Length == 0)
        {
            SetStatus($"{player.name} no tiene cartas en su baraja.");
            return;
        }

        SetStatus($"Cargando {player.cards.Length} cartas de {player.name}...");

        StartCoroutine(apiManager.GetCardsByIds(player.cards, OnCardsLoaded, OnError));
    }

    private void OnCardsLoaded(CardData[] cards)
    {
        if (cardPrefab == null || cardsContainer == null) return;

        foreach (CardData card in cards)
        {
            CardUI cardInstance = Instantiate(cardPrefab, cardsContainer);
            cardInstance.Setup(apiManager, card);
        }

        SetStatus(string.Empty);
    }

    private void ClearCards()
    {
        if (cardsContainer == null) return;

        for (int i = cardsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(cardsContainer.GetChild(i).gameObject);
        }
    }

    private void OnError(string message)
    {
        Debug.LogError(message);
        SetStatus(message);
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
