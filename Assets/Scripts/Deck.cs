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
        /*TODO:
         * Calcular las probabilidades de:
         * - Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
         * - Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
         * - Probabilidad de que el jugador obtenga más de 21 si pide una carta          
         */
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
