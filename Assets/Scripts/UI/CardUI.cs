using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Globalization;

/// <summary>
/// Controla un único ítem de carta dentro de la grilla de la "baraja".
/// Se instancia una vez por cada carta devuelta por la Deck of Cards API.
/// </summary>
public class CardUI : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private GameObject loadingIndicator;

    /// <summary>
    /// Rellena la carta con los datos de la Deck of Cards API (valor, palo
    /// e imagen) e inicia la descarga asíncrona de la imagen.
    /// </summary>
    public void Setup(ApiManager apiManager, CardData card)
    {
        if (cardNameText != null)
        {
            cardNameText.text = FormatName(card.value, card.suit);
        }

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }

        StartCoroutine(LoadSprite(apiManager, card.image));
    }

    private IEnumerator LoadSprite(ApiManager apiManager, string spriteUrl)
    {
        yield return apiManager.GetSprite(
            spriteUrl,
            sprite =>
            {
                if (cardImage != null) cardImage.sprite = sprite;
                if (loadingIndicator != null) loadingIndicator.SetActive(false);
            },
            error =>
            {
                Debug.LogWarning(error);
                if (loadingIndicator != null) loadingIndicator.SetActive(false);
            }
        );
    }

    /// <summary>Convierte value="KING", suit="DIAMONDS" en "King of Diamonds".</summary>
    private string FormatName(string value, string suit)
    {
        if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(suit)) return "???";
        string prettyValue = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
        string prettySuit = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(suit.ToLowerInvariant());
        return $"{prettyValue} of {prettySuit}";
    }
}
