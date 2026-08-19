using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Controla un único ítem de carta dentro de la grilla de la "baraja".
/// Se instancia una vez por cada personaje devuelto por la Rick and Morty API.
/// </summary>
public class CardUI : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private GameObject loadingIndicator;

    /// <summary>
    /// Rellena la carta con los datos de la Rick and Morty API (nombre e
    /// imagen del personaje) e inicia la descarga asíncrona de la imagen.
    /// </summary>
    public void Setup(ApiManager apiManager, CardData card)
    {
        if (cardNameText != null)
        {
            cardNameText.text = card.name;
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
}
