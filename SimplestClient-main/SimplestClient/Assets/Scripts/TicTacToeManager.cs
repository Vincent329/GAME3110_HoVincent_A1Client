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
    Button sendButton;

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
        textDisplay.text = ticTacToeboard[row, column].ToString();
        // send to the server
    }
    
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
