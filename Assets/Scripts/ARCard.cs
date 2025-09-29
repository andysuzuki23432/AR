using UnityEngine;
using System.Collections;

public class ARCard : MonoBehaviour
{
    private CardManager.CardData cardData;
    private bool isFaceDown;
    private Vector3 originalPosition;

    private SpriteRenderer frontRenderer;
    private SpriteRenderer backRenderer;

    public bool IsFaceDown => isFaceDown;
    public CardManager.CardData CardData => cardData;

    public void Initialize(CardManager.CardData data, bool faceDown)
    {
        cardData = data;
        isFaceDown = faceDown;
        originalPosition = transform.position;
        
        FindCardRenderers();
        UpdateCardAppearance();
        
        Debug.Log($"ARCard inicializado: {cardData.cardId}, Virada: {isFaceDown}");
    }

    void FindCardRenderers()
    {
        Transform frontTransform = FindDeepChild(transform, "Front ");
        Transform backTransform = FindDeepChild(transform, "Back_D2");

        if (frontTransform != null)
            frontRenderer = frontTransform.GetComponent<SpriteRenderer>();
        
        if (backTransform != null)
            backRenderer = backTransform.GetComponent<SpriteRenderer>();

        if (frontRenderer == null || backRenderer == null)
            Debug.LogWarning($"Não foi possível encontrar frente/verso na carta: {cardData.cardId}");
    }

    Transform FindDeepChild(Transform parent, string name)
    {
        if (parent.name == name)
            return parent;

        foreach (Transform child in parent)
        {
            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    void UpdateCardAppearance()
    {
        if (frontRenderer != null)
            frontRenderer.enabled = !isFaceDown;
        
        if (backRenderer != null)
            backRenderer.enabled = isFaceDown;
    }

    public void FlipCard()
    {
        StartCoroutine(FlipCardAnimation());
    }

    private IEnumerator FlipCardAnimation()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Vector3 startRotation = transform.eulerAngles;
        Vector3 targetRotation = startRotation + new Vector3(0, 180, 0);

        while (elapsed < duration)
        {
            transform.eulerAngles = Vector3.Lerp(startRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isFaceDown = !isFaceDown;
        UpdateCardAppearance();
        transform.eulerAngles = Vector3.zero;
        
        Debug.Log($"Carta virada: {cardData.cardId} → Agora virada: {isFaceDown}");
    }

    public int GetCardValue()
    {
        return cardData.value;
    }

    void OnMouseDown()
    {
        if (Application.isEditor)
        {
            FlipCard();
        }
    }
}