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
    public int currentTurn;

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
    Button sendButton;

    // multicast delegate to search for the proper button appllied
    public delegate void SearchButton(int row, int column);
    public event SearchButton Search;

    // just to check if the network client is functional
    [SerializeField] NetworkedClient networkedClient;

    // Game Variables
    [SerializeField] private int playerTurn;
    
    /// <summary>
    /// The moment this manager turns on, go through any and all possible items
    /// </summary>
    private void OnEnable()
    {
        // hope this works
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
        }

        sendButton.onClick.AddListener(SendChatMessage);
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
    /// Once received message from the network client, update the board with the index of the current player at that row and column
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="currentPlayer"></param>
    public void PlacePosition(int row, int column, int currentPlayer)
    {
       
        ticTacToeboard[row, column] = currentPlayer;
        Debug.Log(ticTacToeboard[row, column]);
        networkedClient.SendMessageToHost(ClientToServerSignifiers.PlayerAction + "," + row + "," + column + "," + playerID);
        // TODO:
        // send to the server
        // check possible win conditions
        if (CheckWinCondition())
        {
            networkedClient.SendMessageToHost(ClientToServerSignifiers.PlayerWins + "," + playerID);
        } 
    }
    
    /// <summary>
    /// Specific function for when we're receiving actions from the opponent
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="opponentPlayer"></param>
    public void OpponentPlacePosition(int row, int column, int opponentPlayer)
    {
        ticTacToeboard[row, column] = opponentPlayer;
        // call the event
        Search(row, column);
    }

    private bool CheckWinCondition()
    {
        if ((ticTacToeboard[0,0] == playerID && ticTacToeboard[1, 0] == playerID && ticTacToeboard[2, 0] == playerID)
        || (ticTacToeboard[0,1] == playerID && ticTacToeboard[1, 1] == playerID && ticTacToeboard[2, 1] == playerID)
        || (ticTacToeboard[0,2] == playerID && ticTacToeboard[1, 2] == playerID && ticTacToeboard[2, 2] == playerID)
        || (ticTacToeboard[0,0] == playerID && ticTacToeboard[0, 1] == playerID && ticTacToeboard[0, 2] == playerID)
        || (ticTacToeboard[1,0] == playerID && ticTacToeboard[1, 1] == playerID && ticTacToeboard[1, 2] == playerID)
        || (ticTacToeboard[2,0] == playerID && ticTacToeboard[2, 1] == playerID && ticTacToeboard[2, 2] == playerID)
        || (ticTacToeboard[0,0] == playerID && ticTacToeboard[1, 1] == playerID && ticTacToeboard[2, 2] == playerID)
        || (ticTacToeboard[2,0] == playerID && ticTacToeboard[1, 1] == playerID && ticTacToeboard[0, 2] == playerID))
        {
            Debug.Log("Player " + playerID + " wins");
            textDisplay.text = "Player " + playerID + " wins";
            return true;
        }
        else
        {
            // switch player turn
            return false;
        }
    }

    private void ResetGame()
    {

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
