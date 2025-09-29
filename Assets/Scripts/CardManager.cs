using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    [System.Serializable]
    public class CardData
    {   
        public string cardId;
        public GameObject cardPrefab;
        public int value;
    }

    [Header("Cards - Apenas 5 cartas específicas")]
    public CardData queenCard;
    public CardData nineCard;   
    public CardData kingCard;    
    public CardData jackCard; 
    public CardData twoCard;  

    [Header("Spawn Points")]
    public Transform deckLocation;
    public Transform playerFirstCardSpot;
    public Transform playerSecondCardSpot;
    public Transform playerThirdCardSpot;
    public Transform dealerFirstCardSpot;
    public Transform dealerSecondCardSpot;

    private Dictionary<string, CardData> cardDictionary = new Dictionary<string, CardData>();

    void Start()
    {
        cardDictionary.Add("Queen", queenCard);
        cardDictionary.Add("Nine", nineCard);
        cardDictionary.Add("King", kingCard);
        cardDictionary.Add("Jack", jackCard);
        cardDictionary.Add("Two", twoCard);
        
        Debug.Log("CardManager iniciado com " + cardDictionary.Count + " cartas");
    }

    public CardData GetCard(string cardName)
    {
        if (cardDictionary.ContainsKey(cardName))
        {
            Debug.Log($"CardManager: Carta {cardName} encontrada (Valor: {cardDictionary[cardName].value})");
            return cardDictionary[cardName];
        }
        
        Debug.LogError($"CardManager: Carta {cardName} não encontrada no dicionário!");
        return null;
    }

    public IEnumerator DealSpecificCard(string cardName, Vector3 position, List<GameObject> hand, bool faceDown)
    {
        Debug.Log($"CardManager: Distribuindo {cardName} para posição {position}, virada: {faceDown}");
        
        CardData cardData = GetCard(cardName);
        if (cardData == null) 
        {
            Debug.LogError($"CardManager: Não foi possível obter dados da carta {cardName}");
            yield break;
        }

        if (cardData.cardPrefab == null)
        {
            Debug.LogError($"CardManager: Prefab da carta {cardName} é null!");
            yield break;
        }

        GameObject newCard = Instantiate(cardData.cardPrefab, deckLocation.position, Quaternion.identity);
        Debug.Log($"CardManager: Prefab instanciado: {newCard.name}");

        ARCard arCard = newCard.GetComponent<ARCard>();
        if (arCard == null)
        {
            Debug.Log("CardManager: Adicionando componente ARCard");
            arCard = newCard.AddComponent<ARCard>();
        }

        arCard.Initialize(cardData, faceDown);
        Debug.Log($"CardManager: ARCard inicializado - {cardData.cardId}, Virada: {faceDown}");

        if (newCard.GetComponent<Collider>() == null)
        {
            BoxCollider collider = newCard.AddComponent<BoxCollider>();
            collider.size = new Vector3(2f, 0.1f, 2.5f);
        }

        yield return StartCoroutine(MoveCardToPosition(newCard.transform, position, 0.5f));

        hand.Add(newCard);
        Debug.Log($"CardManager: Carta {cardName} adicionada à mão. Total na mão: {hand.Count}");
    }

    private IEnumerator MoveCardToPosition(Transform card, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = card.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            card.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        card.position = targetPosition;
    }

    public int CalculateHandValue(List<GameObject> hand)
    {
        int value = 0;
        Debug.Log($"=== CardManager: CALCULANDO MÃO COM {hand.Count} CARTAS ===");

        if (hand.Count == 0)
        {
            Debug.LogWarning("CardManager: Mão vazia!");
            return 0;
        }

        foreach (GameObject cardObj in hand)
        {
            if (cardObj == null)
            {
                Debug.LogWarning("CardManager: GameObject da carta é null!");
                continue;
            }

            ARCard arCard = cardObj.GetComponent<ARCard>();
            if (arCard != null)
            {
                int cardValue = arCard.GetCardValue();
                bool isFaceDown = arCard.IsFaceDown;
                
                Debug.Log($"CardManager: Processando {arCard.CardData.cardId} - Valor: {cardValue}, Virada: {isFaceDown}");

                if (!isFaceDown)
                {
                    int pointsToAdd = 0;
                    if (cardValue >= 10) 
                    {
                        pointsToAdd = 10;
                        Debug.Log($"CardManager: {arCard.CardData.cardId} é figura → 10 pontos");
                    }
                    else 
                    {
                        pointsToAdd = cardValue; 
                        Debug.Log($"CardManager: {arCard.CardData.cardId} → {cardValue} pontos");
                    }
                    
                    value += pointsToAdd;
                    Debug.Log($"CardManager: Adicionando {pointsToAdd} pontos → Total: {value}");
                }
                else
                {
                    Debug.Log($"CardManager: {arCard.CardData.cardId} está virada → 0 pontos");
                }
            }
            else
            {
                Debug.LogError("CardManager: ARCard não encontrado no GameObject da carta!");
            }
        }

        Debug.Log($"=== CardManager: VALOR FINAL DA MÃO: {value} ===");
        return value;
    }

    [ContextMenu("Debug All Cards")]
    public void DebugAllCardsInScene()
    {
        Debug.Log("=== DEBUG GERAL DE CARTAS ===");
        ARCard[] allCards = FindObjectsOfType<ARCard>();
        Debug.Log($"Total de cartas na cena: {allCards.Length}");

        foreach (ARCard card in allCards)
        {
            Debug.Log($"Carta: {card.CardData.cardId}, Virada: {card.IsFaceDown}, Valor: {card.GetCardValue()}, Posição: {card.transform.position}");
        }
    }
}