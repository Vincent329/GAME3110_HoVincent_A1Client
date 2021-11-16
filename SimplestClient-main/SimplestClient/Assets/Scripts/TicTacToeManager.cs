using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeManager : MonoBehaviour
{
    // Start is called before the first frame update // tic tac toe UI
    //
    // [0,0] [1,0] [2,0]
    // [0,1] [1,1] [2,1]
    // [0,2] [1,2] [2,2]
    //
    private int[,] ticTacToeboard;
    public int[,] GetTicTacToeBoard => ticTacToeboard;
    
    // player variables
    // if current turn is not equal to the player ID, then it's not the player's turn to move
    [SerializeField] private int playerID;

    // TODO:
    // on game start, assign the correct player ID as either 1 or 2
    public int PlayerID
    {
        get => playerID;
        set
        {
            playerID = value;
        }
    }

    public Text textDisplay;
    InputField chatInputField;
    Button sendButton, resetButton;

    // --------------------EVENTS-----------------------------------------
    // multi-cast delegate to search for the proper button appllied
    public delegate void SearchButton(int row, int column, int opponentID);
    public event SearchButton Search;

    // multi-cast delegate for all the buttons to be reset upon game restart
    public delegate void ResetButton(int row, int column);
    public event ResetButton Reset;
    
    // multi-cast delegate for all the buttons to be reset upon game restart
    public delegate void DeactivateBoard(int row, int column);
    public event DeactivateBoard Deactivate;

    public delegate void DelegateTurn(int row, int column, int checkTurn);
    public event DelegateTurn NextTurn;

    // just to check if the network client is functional
    [SerializeField] NetworkedClient networkedClient;

    // Game Variables... need to use this to determine a draw
    // Purpose: counts from 1 to 9, upon 9, it's a reset
    [SerializeField] private int playerTurn;
    public int PlayerTurn
    {
        get => playerTurn;
        set
        {
            playerTurn = value;
        }
    }

    [SerializeField] bool finishedPlaying;
    public bool FinishedPlaying
    {
        get => finishedPlaying;
        set
        {
            finishedPlaying = value;
        }
    }
    /// <summary>
    /// The moment this manager turns on, go through any and all possible items
    /// </summary>
    private void OnEnable()
    {
        // finds the client object for communication to the server
        networkedClient = GameObject.FindObjectOfType<NetworkedClient>();

        InputField[] allInputFields = FindObjectsOfType<InputField>();
        foreach (InputField tempField in allInputFields)
        {
            if (tempField.gameObject.name == "Chat Input Field")
                chatInputField = tempField;
        }

        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button tempButton in allButtons)
        {
            if (tempButton.gameObject.name == "Send Message Button")
            {
                sendButton = tempButton;
            } 
            else if (tempButton.gameObject.name == "Reset Game Button")
            {
                resetButton = tempButton;
            }
        }

        sendButton.onClick.AddListener(SendChatMessage);
        resetButton.onClick.AddListener(ResetButtonPrompt);
        resetButton.gameObject.SetActive(false);
    }

    void Start()
    {
        // set up the board size
        ticTacToeboard = new int[3,3];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Goes through the board to reactivate the board for the player with the next turn
    /// </summary>
    /// <param name="currentTurn"></param>
    public void CheckTurn(int currentTurn)
    {
        for (int i = 0; i < ticTacToeboard.GetLength(0); i++)
        {
            for (int j = 0; j < ticTacToeboard.GetLength(1); j++)
            {
                if (ticTacToeboard[i, j] == 0)
                {
                    NextTurn(i, j, currentTurn);
                }
            }
        }
    }

    /// <summary>
    /// Any point whether we either win, lose, or draw, activate the reset game button
    /// </summary>
    public void ActivateResetButton()
    {
        resetButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// Called from Button data at the specific row and column assigned, 
    /// the purpose of this function is to send information of the player's action to the server
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="currentPlayer"></param>
    public void PlacePosition(int row, int column, int currentPlayer)
    {
        
        ticTacToeboard[row, column] = currentPlayer;

        networkedClient.SendMessageToHost(ClientToServerSignifiers.PlayerAction + "," + row + "," + column + "," + playerID);
        // TODO:
        // send to the server
        // check possible win conditions
        if (CheckWinCondition())
        {
            networkedClient.SendMessageToHost(ClientToServerSignifiers.PlayerWins + "," + playerID);
            ActivateResetButton();
        }
        // check if we've hit a draw
        else
        {
            CheckDraw();
        }
    }

    /// <summary>
    /// Once received message from the network client, update the board with the index of the OPPONENT player at that row and column
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="opponentPlayer"></param>
    public void OpponentPlacePosition(int row, int column, int opponentPlayer)
    {
        // assign the player
        ticTacToeboard[row, column] = opponentPlayer;
        // check if the board is full
        // call the event
        Search(row, column, opponentPlayer);
        
        CheckDraw(); // increment the turn counter
    }

    /// <summary>
    /// Checks possible win conditions
    /// </summary>
    /// <returns></returns>
    private bool CheckWinCondition()
    {
        // search all possible win conditions
        if ((ticTacToeboard[0,0] == playerID && ticTacToeboard[1, 0] == playerID && ticTacToeboard[2, 0] == playerID)
        || (ticTacToeboard[0,1] == playerID && ticTacToeboard[1, 1] == playerID && ticTacToeboard[2, 1] == playerID)
        || (ticTacToeboard[0,2] == playerID && ticTacToeboard[1, 2] == playerID && ticTacToeboard[2, 2] == playerID)
        || (ticTacToeboard[0,0] == playerID && ticTacToeboard[0, 1] == playerID && ticTacToeboard[0, 2] == playerID)
        || (ticTacToeboard[1,0] == playerID && ticTacToeboard[1, 1] == playerID && ticTacToeboard[1, 2] == playerID)
        || (ticTacToeboard[2,0] == playerID && ticTacToeboard[2, 1] == playerID && ticTacToeboard[2, 2] == playerID)
        || (ticTacToeboard[0,0] == playerID && ticTacToeboard[1, 1] == playerID && ticTacToeboard[2, 2] == playerID)
        || (ticTacToeboard[2,0] == playerID && ticTacToeboard[1, 1] == playerID && ticTacToeboard[0, 2] == playerID))
        {
            //Debug.Log("Player " + playerID + " wins");
            //textDisplay.text = "Player " + playerID + " wins";
            return true;
        }
        return false;
    }

    private void ResetButtonPrompt()
    {
        ResetGame();
        networkedClient.SendMessageToHost(ClientToServerSignifiers.ResetGame + "");
    }

    public void CheckDraw()
    {
        // check draw here
        playerTurn++;
        if (playerTurn > 9) // tic tac toe has a max of 9 turns
        {
            ActivateResetButton(); // turn on the reset button
        }
    }


    /// <summary>
    /// Resets the game board, goes through the array and resets values as well as the button attached to that array location
    /// </summary>
    public void ResetGame()
    {
        Debug.Log("Commence Game Reset");
        ResetButtons();

        playerTurn = 1; // reset player turn
        resetButton.gameObject.SetActive(false);
    }

    public void ResetButtons()
    {
        for (int i = 0; i < ticTacToeboard.GetLength(0); i++)
        {
            for (int j = 0; j < ticTacToeboard.GetLength(1); j++)
            {
                // set the tictactoe board to 0 to reset the value
                // call the delegate to reset the button at that specific location
                ticTacToeboard[i, j] = 0;
                Reset(i, j);
            }
        }
    }
    public void GameOverOnWin()
    {
        for (int i = 0; i < ticTacToeboard.GetLength(0); i++)
        {
            for (int j = 0; j < ticTacToeboard.GetLength(1); j++)
            {
                if (ticTacToeboard[i, j] == 0)
                {
                    Deactivate(i, j);
                }
            }
        }
    }

    // ------------------ SPECTATOR FUNCTIONALITY -------------------

    /// <summary>
    /// if the player's a spectator, shut down all the functionality of the board
    /// </summary>
    public void SpectatorShutdown()
    {
        for (int i = 0; i < ticTacToeboard.GetLength(0); i++)
        {
            for (int j = 0; j < ticTacToeboard.GetLength(1); j++)
            {             
                Deactivate(i, j);
            }
        }
    }

    /// <summary>
    /// Simply update the board for the spectator
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="player"></param>
    public void SpectatorUpdate(int row, int column, int player)
    {
        // assign the player
        ticTacToeboard[row, column] = player;
        // check if the board is full
        // call the event
        Search(row, column, player);
    }

    // ---------------- CHAT FUNCTIONALITY -------------------------------------

    /// <summary>
    /// Sends a message over to the server for the other client.
    /// </summary>
    private void SendChatMessage()
    {
        networkedClient.SendMessageToHost(ClientToServerSignifiers.SendPresetMessage + "," + chatInputField.text);
    }

    public void ReceiveMessage(string message)
    {
        textDisplay.text = message;
    }
}
