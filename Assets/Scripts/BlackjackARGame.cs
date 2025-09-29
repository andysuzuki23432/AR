using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackjackARGame : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text gameStateText;
    public Button hitButton;
    public Button standButton;
    public Button lookButton;

    [Header("Look Mechanic")]
    public float lookSuccessChance = 0.5f;

    private List<GameObject> dealerCards = new List<GameObject>();
    private List<GameObject> playerCards = new List<GameObject>();
    private CardManager cardManager;

    private GameState currentState = GameState.PlayerTurn;
    private bool isLookingAtDealerCard = false;
    private bool isRevealingCard = false;

    public enum GameState
    {
        PlayerTurn,
        DealerTurn,
        GameOver
    }

    void Start()
    {
        cardManager = GetComponent<CardManager>();
        StartGame();
    }

    void StartGame()
    {
        ClearTable();
        StartCoroutine(InitialDeal());
    }

    IEnumerator InitialDeal()
    {
        Debug.Log("=== INICIANDO DISTRIBUIÇÃO ===");
        
        yield return StartCoroutine(cardManager.DealSpecificCard("King", 
            cardManager.dealerFirstCardSpot.position, dealerCards, false));
        
        yield return StartCoroutine(cardManager.DealSpecificCard("Jack", 
            cardManager.dealerSecondCardSpot.position, dealerCards, true));

        yield return StartCoroutine(cardManager.DealSpecificCard("Queen", 
            cardManager.playerFirstCardSpot.position, playerCards, false));
        
        yield return StartCoroutine(cardManager.DealSpecificCard("Nine", 
            cardManager.playerSecondCardSpot.position, playerCards, false));

        Debug.Log($"Distribuição completa - Dealer: {dealerCards.Count}, Player: {playerCards.Count}");
        
        UpdateUI();
    }

    public void OnHit()
    {
        if (currentState != GameState.PlayerTurn) return;

        StartCoroutine(cardManager.DealSpecificCard("Two", 
            cardManager.playerThirdCardSpot.position, playerCards, false));

        int playerValue = cardManager.CalculateHandValue(playerCards);
        if (playerValue > 21)
        {
            gameStateText.text = "Estourou! Sua pontuação: " + playerValue;
            currentState = GameState.GameOver;
            StartCoroutine(RevealDealerCard());
            UpdateUI();
        }
        else
        {
            gameStateText.text = "Sua pontuação: " + playerValue;
        }
    }

    public void OnStand()
    {
        if (currentState != GameState.PlayerTurn) return;

        currentState = GameState.DealerTurn;
        StartCoroutine(DealerPlay());
    }

    public void OnLook()
    {
        if (currentState != GameState.PlayerTurn || isLookingAtDealerCard) return;

        bool success = Random.Range(0f, 1f) <= lookSuccessChance;
        
        if (success)
        {
            StartCoroutine(SuccessfulLook());
        }
        else
        {
            StartCoroutine(FailedLook());
        }
    }

    IEnumerator SuccessfulLook()
    {
        isLookingAtDealerCard = true;
        lookButton.interactable = false;

        gameStateText.text = "Look bem-sucedido! Revelando carta do dealer por 4 segundos...";
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(RevealDealerCardTemporarily());
        
        isLookingAtDealerCard = false;
        lookButton.interactable = true;
        UpdateUI();
    }

    IEnumerator FailedLook()
    {
        isLookingAtDealerCard = true;
        lookButton.interactable = false;
        
        gameStateText.text = "Look falhou! O dealer notou você.";
        yield return new WaitForSeconds(2f);

        isLookingAtDealerCard = false;
        lookButton.interactable = true;
        UpdateUI();
    }

    IEnumerator DealerPlay()
    {
        Debug.Log("=== INICIANDO TURNO DO DEALER ===");
        gameStateText.text = "Revelando cartas do dealer...";
        UpdateUI();
        
        yield return new WaitForSeconds(1f);
        
        yield return StartCoroutine(RevealDealerCard());
        
        yield return new WaitForSeconds(1f);
        
        DebugDealerCards("APÓS REVELAR CARTA - ANTES DO CÁLCULO");
        
        int dealerValue = cardManager.CalculateHandValue(dealerCards);
        int playerValue = cardManager.CalculateHandValue(playerCards);

        Debug.Log($"=== RESULTADO FINAL ===");
        Debug.Log($"Player: {playerValue}, Dealer: {dealerValue}");

        if (playerValue > 21)
        {
            gameStateText.text = "Você estourou! Dealer ganha.";
        }
        else if (dealerValue > 21)
        {
            gameStateText.text = "Dealer estourou! Você ganha!";
        }
        else if (playerValue > dealerValue)
        {
            gameStateText.text = "Você ganha! " + playerValue + " vs " + dealerValue;
        }
        else if (dealerValue > playerValue)
        {
            gameStateText.text = "Dealer ganha! " + dealerValue + " vs " + playerValue;
        }
        else
        {
            gameStateText.text = "Empate! " + playerValue + " vs " + dealerValue;
        }

        currentState = GameState.GameOver;
        UpdateUI();
    }

    IEnumerator RevealDealerCardTemporarily()
    {
        Debug.Log("Revelando carta do dealer TEMPORARIAMENTE");
        
        if (dealerCards.Count > 1 && dealerCards[1] != null)
        {
            ARCard hiddenCard = dealerCards[1].GetComponent<ARCard>();
            if (hiddenCard != null)
            {
                Debug.Log($"Carta encontrada: {hiddenCard.CardData.cardId}, Virada: {hiddenCard.IsFaceDown}");
                
                if (hiddenCard.IsFaceDown)
                {
                    hiddenCard.FlipCard();
                    Debug.Log("Carta virada para cima temporariamente");
                    
                    yield return new WaitForSeconds(4f);
                    
                    hiddenCard.FlipCard();
                    Debug.Log("Carta virada de volta para baixo");
                    
                    yield return new WaitForSeconds(0.5f);
                }
                else
                {
                    Debug.Log("Carta já estava virada para cima");
                    yield return new WaitForSeconds(4f); 
                }
            }
            else
            {
                Debug.LogError("ARCard não encontrado na segunda carta do dealer!");
            }
        }
        else
        {
            Debug.LogError("Segunda carta do dealer não existe!");
        }
    }

    IEnumerator RevealDealerCard()
    {
        Debug.Log("=== REVELANDO CARTA DO DEALER PERMANENTEMENTE ===");
        isRevealingCard = true;
        
        if (dealerCards.Count > 1 && dealerCards[1] != null)
        {
            ARCard hiddenCard = dealerCards[1].GetComponent<ARCard>();
            if (hiddenCard != null)
            {
                Debug.Log($"Carta encontrada: {hiddenCard.CardData.cardId}, Virada: {hiddenCard.IsFaceDown}");
                
                if (hiddenCard.IsFaceDown)
                {
                    Debug.Log("Virando carta do dealer permanentemente");
                    
                    hiddenCard.FlipCard();
                    
                    yield return new WaitForSeconds(1f);
                    
                    Debug.Log($"Carta agora está virada? {!hiddenCard.IsFaceDown}");
                    
                    if (!hiddenCard.IsFaceDown)
                    {
                        Debug.Log("CARTA DO DEALER REVELADA COM SUCESSO!");
                    }
                    else
                    {
                        Debug.LogError("CARTA DO DEALER NÃO FOI VIRADA!");
                    }
                }
                else
                {
                    Debug.Log("Carta do dealer já estava virada para cima");
                }
            }
            else
            {
                Debug.LogError(" ARCard não encontrado na segunda carta do dealer!");
            }
        }
        else
        {
            Debug.LogError($"dealerCards tem apenas {dealerCards.Count} cartas, esperava pelo menos 2");
        }
        
        isRevealingCard = false;
        yield return null;
    }

    void DebugDealerCards(string context)
    {
        Debug.Log($"=== DEBUG DEALER CARDS ({context}) ===");
        Debug.Log($"Total de cartas do dealer: {dealerCards.Count}");
        
        if (dealerCards.Count == 0)
        {
            Debug.LogError("Nenhuma carta do dealer encontrada!");
            return;
        }

        for (int i = 0; i < dealerCards.Count; i++)
        {
            if (dealerCards[i] == null)
            {
                Debug.LogError($"Dealer Card {i}: GameObject é NULL!");
                continue;
            }

            ARCard card = dealerCards[i].GetComponent<ARCard>();
            if (card != null)
            {
                Debug.Log($"Dealer Card {i}: {card.CardData.cardId}, Virada: {card.IsFaceDown}, Valor: {card.GetCardValue()}");
                
            }
            else
            {
                Debug.LogError($"Dealer Card {i}: ARCard component missing!");
            }
        }

        int manualValue = 0;
        foreach (GameObject cardObj in dealerCards)
        {
            if (cardObj != null)
            {
                ARCard card = cardObj.GetComponent<ARCard>();
                if (card != null && !card.IsFaceDown)
                {
                    int cardValue = card.GetCardValue();
                    manualValue += (cardValue >= 10) ? 10 : cardValue;
                }
            }
        }
        Debug.Log($"=== CÁLCULO MANUAL DO DEALER: {manualValue} ===");
    }

    void ClearTable()
    {
        foreach (GameObject card in dealerCards) 
        {
            if (card != null) Destroy(card);
        }
        foreach (GameObject card in playerCards) 
        {
            if (card != null) Destroy(card);
        }
        dealerCards.Clear();
        playerCards.Clear();
    }

    void UpdateUI()
    {
        if (currentState == GameState.PlayerTurn)
        {
            int playerValue = cardManager.CalculateHandValue(playerCards);
            gameStateText.text = $"Sua pontuação: {playerValue}\nHit, Stand ou Look?";
        }

        hitButton.interactable = (currentState == GameState.PlayerTurn);
        standButton.interactable = (currentState == GameState.PlayerTurn);
        lookButton.interactable = (currentState == GameState.PlayerTurn && !isLookingAtDealerCard && !isRevealingCard);
    }

    public void NewGame()
    {
        currentState = GameState.PlayerTurn;
        isLookingAtDealerCard = false;
        isRevealingCard = false;
        ClearTable();
        StartGame();
    }
}