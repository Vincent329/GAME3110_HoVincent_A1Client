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

    public int PlayerID
    {
        get => playerID;
        set
        {
            playerID = value;
        }
    }

    public Text textDisplay, IDDisplay;
    InputField chatInputField;
    Button sendButton, resetButtonTrigger, saveReplayButton;

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

    // ------------------------ REPLAY LIST --------------------------------
    [SerializeField] Dropdown replayDropdownList;

    // flips on if replay is happening
    [SerializeField] private bool isReplaying;
    public bool IsReplaying
    {
        get => isReplaying;
        set
        {
            isReplaying = value;
        }
    }

    public Queue<TicTacToePlayAnimation> replayAnimationQueue;
    float delayReplayAnimation;
        
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
                resetButtonTrigger = tempButton;
            }
            else if (tempButton.gameObject.name == "Save Replay Button")
            {
                saveReplayButton = tempButton;
            }
        }

        Dropdown[] allDropdownBoxes = FindObjectsOfType<Dropdown>();
        foreach (Dropdown tempDrop in allDropdownBoxes)
        {
            if (tempDrop.gameObject.name == "Replay List")
            {
                replayDropdownList = tempDrop;
            }
        }

        sendButton.onClick.AddListener(SendChatMessage);
        resetButtonTrigger.onClick.RemoveAllListeners();
        resetButtonTrigger.onClick.AddListener(ResetButtonPrompt); 
        saveReplayButton.onClick.RemoveAllListeners();
        saveReplayButton.onClick.AddListener(SaveReplayButtonPrompt);

        resetButtonTrigger.gameObject.SetActive(false);
        saveReplayButton.gameObject.SetActive(false);

        // clean the list on first entry
        replayDropdownList.options.Clear();
        replayDropdownList.onValueChanged.RemoveAllListeners();
        replayDropdownList.onValueChanged.AddListener(delegate { LoadReplayDropDownChanged(replayDropdownList); });

        replayAnimationQueue = new Queue<TicTacToePlayAnimation>();
        
        //});
    }

    void Start()
    {
        // set up the board size
        ticTacToeboard = new int[3,3];
    }

    // Update is called once per frame
    void Update()
    {
        if (replayAnimationQueue.Count > 0)
        {
            if (delayReplayAnimation <= 0.0f)
            {
                // QUEUE USE, the first item will be the first one played then removed from the list
                TicTacToePlayAnimation currentPlay = replayAnimationQueue.Dequeue();
                ServerPlacePosition(currentPlay.row, currentPlay.column, currentPlay.playerID);

                delayReplayAnimation = 1.0f;
            } else
            {
                delayReplayAnimation -= Time.deltaTime;
            }
            
        }

        
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
        resetButtonTrigger.gameObject.SetActive(true);
    }

    public void ActivateSaveReplayButton()
    {
        saveReplayButton.gameObject.SetActive(true);
    }

    public void SaveReplayButtonPrompt()
    {
        networkedClient.SendMessageToHost(ClientToServerSignifiers.SaveReplay + "");
        saveReplayButton.gameObject.SetActive(false);
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
            ActivateSaveReplayButton();
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
    public void ServerPlacePosition(int row, int column, int opponentPlayer)
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
            textDisplay.text = "Player " + playerID + " wins";
            return true;
        }
        return false;
    }

    /// <summary>
    /// Event when the reset button gets chosen
    /// </summary>
    private void ResetButtonPrompt()
    {
        ResetGame();
        networkedClient.SendMessageToHost(ClientToServerSignifiers.ResetGame + "");
        Debug.Log("Reset finish");
    }

    /// <summary>
    /// Checks for the draw
    /// </summary>
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
        ResetButtons();

        playerTurn = 1; // reset player turn
        resetButtonTrigger.gameObject.SetActive(false);
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

    /// <summary>
    /// Deactivates Buttons attached with ButtonData component
    /// </summary>
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
    #region Functionality for Replay Dropdown list
    public void AddToDropdownMenu(int index, string replayName)
    {
        replayDropdownList.options.Add(new Dropdown.OptionData() { text = replayName });
    }

    private void LoadReplayDropDownChanged(Dropdown dropdown)
    {
        // send to the network that we request a replay
        networkedClient.SendMessageToHost(ClientToServerSignifiers.RequestReplay + "," + dropdown.options[dropdown.value].text);
    }


    public void ReplayMode()
    {
        if (isReplaying)
        {
            ResetButtons();
            SpectatorShutdown();
        }
        else
        {
            ActivateResetButton();
        }
    }

    #endregion 
    #region // ------------------ SPECTATOR FUNCTIONALITY -------------------

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

    #endregion
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

/// <summary>
/// structure for holding row, column, and player data on replay
/// </summary>
public class TicTacToePlayAnimation
{
    public int row;
    public int column;
    public int playerID;

    public TicTacToePlayAnimation(int in_Row, int in_Column, int in_PlayerID)
    {
        row = in_Row;
        column = in_Column;
        playerID = in_PlayerID;
    }
}
