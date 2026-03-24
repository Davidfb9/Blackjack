using UnityEngine;
using UnityEngine.UI;

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;
    public Text playerPointsMessage;
    public Text dealerPointsMessage;
    public Text creditMessage;
    public Dropdown betDropdown;

    private int credits = 1000;
    private int currentBet = 0;
    public int[] values = new int[52];
    int cardIndex = 0;    
       
    private void Awake()
    {    
        InitCardValues();        

    }

    private void Start()
    {
        ShuffleCards();
        StartGame();        
    }

    private void InitCardValues()
    {
        // Patrón de valores por palo: As=11, 2-10 su valor, J/Q/K=10
        int[] pattern = { 11, 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10 };

        for (int suit = 0; suit < 4; suit++)
        {
            for (int rank = 0; rank < 13; rank++)
            {
                values[suit * 13 + rank] = pattern[rank];
            }
        }
    }

    private void ShuffleCards()
    {
        for (int i = 51; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            // Intercambiamos faces[i] con faces[j]
            Sprite tempSprite = faces[i];
            faces[i] = faces[j];
            faces[j] = tempSprite;

            // Intercambiamos values[i] con values[j] de forma coordinada
            int tempValue = values[i];
            values[i] = values[j];
            values[j] = tempValue;
        }
    }

    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }

        // Comprobamos Blackjack después de repartir las 2 cartas iniciales
        bool playerBlackjack = player.GetComponent<CardHand>().points == 21;
        bool dealerBlackjack = dealer.GetComponent<CardHand>().points == 21;

        if (playerBlackjack || dealerBlackjack)
        {
            dealer.GetComponent<CardHand>().InitialToggle(); // Revelamos la carta oculta del dealer

            hitButton.interactable = false;
            stickButton.interactable = false;

            if (playerBlackjack && dealerBlackjack)
                finalMessage.text = "¡Empate! Ambos tienen Blackjack.";
            else if (playerBlackjack)
                finalMessage.text = "¡Blackjack! ¡Ganaste!";
            else
                finalMessage.text = "¡Blackjack del Dealer! Perdiste.";
        }
    }

    private void CalculateProbabilities()
    {
        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;
        int remaining = 52 - cardIndex; // Cartas que quedan en el mazo

        if (remaining <= 0)
        {
            probMessage.text = "Sin cartas restantes.";
            return;
        }

        int dealerBeats = 0;   // Dealer gana con su carta oculta
        int between1721 = 0;   // Jugador obtiene 17-21 pidiendo carta
        int bust = 0;          // Jugador se pasa de 21 pidiendo carta

        for (int i = cardIndex; i < 52; i++)
        {
            int cardValue = values[i];

            // Probabilidad 1: ¿El dealer ya supera al jugador con su mano actual?
            // (la carta oculta ya está contada en dealerPoints)
            if (dealerPoints > playerPoints && dealerPoints <= 21)
                dealerBeats++;

            // Probabilidad 2 y 3: ¿Qué pasa si el jugador pide esta carta?
            int newPlayerPoints = playerPoints + cardValue;
            // Ajuste del As: si se pasa y tiene un As contado como 11, lo cuenta como 1
            if (newPlayerPoints > 21 && cardValue == 11)
                newPlayerPoints -= 10;

            if (newPlayerPoints >= 17 && newPlayerPoints <= 21)
                between1721++;
            else if (newPlayerPoints > 21)
                bust++;
        }

        float pDealerBeats = dealerPoints > playerPoints && dealerPoints <= 21 ? 100f : 0f;
        float pBetween1721 = (float)between1721 / remaining * 100f;
        float pBust = (float)bust / remaining * 100f;

        probMessage.text =
            $"Dealer gana: {pDealerBeats:F1}%\n" +
            $"Jugador 17-21 si pide: {pBetween1721:F1}%\n" +
            $"Jugador se pasa si pide: {pBust:F1}%";
    }

    void PushDealer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        dealer.GetComponent<CardHand>().Push(faces[cardIndex],values[cardIndex]);
        cardIndex++;        
    }

    void PushPlayer()
    {
        /*TODO:
         * Dependiendo de cómo se implemente ShuffleCards, es posible que haya que cambiar el índice.
         */
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]/*,cardCopy*/);
        cardIndex++;
        CalculateProbabilities();
    }

    public void Hit()
    {
        // Si es la mano inicial (solo 2 cartas), volteamos la carta oculta del dealer
        if (player.GetComponent<CardHand>().cards.Count == 2)
            dealer.GetComponent<CardHand>().InitialToggle();

        // Repartimos carta al jugador
        PushPlayer();

        // Comprobamos si el jugador se ha pasado de 21
        if (player.GetComponent<CardHand>().points > 21)
        {
            finalMessage.text = "¡Te has pasado! El Dealer gana.";
            hitButton.interactable = false;
            stickButton.interactable = false;
        }
    }

    public void Stand()
    {
        // Si es la mano inicial, volteamos la carta oculta del dealer
        if (player.GetComponent<CardHand>().cards.Count == 2)
            dealer.GetComponent<CardHand>().InitialToggle();

        // El dealer pide cartas mientras tenga 16 o menos
        while (dealer.GetComponent<CardHand>().points <= 16)
            PushDealer();

        // Desactivamos los botones
        hitButton.interactable = false;
        stickButton.interactable = false;

        // Comparamos puntuaciones y mostramos resultado
        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;

        if (dealerPoints > 21)
            finalMessage.text = "¡El Dealer se ha pasado! ¡Ganaste!";
        else if (playerPoints > dealerPoints)
            finalMessage.text = "¡Ganaste!";
        else if (playerPoints < dealerPoints)
            finalMessage.text = "¡El Dealer gana!";
        else
            finalMessage.text = "¡Empate!";
    }

    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();          
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }
    
}
