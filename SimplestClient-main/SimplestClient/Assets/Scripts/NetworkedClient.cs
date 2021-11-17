using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class NetworkedClient : MonoBehaviour
{

    int connectionID;
    int maxConnections = 1000; // maximum amount of users that can get into the server
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 10563;
    byte error;
    bool isConnected = false;
    int ourClientID;

    // All the gamesystemmanager needs to worry about is keeping track of states
    GameObject gameSystemManager;

    // the tictactoe manager is where we're going to be referencing for any game specific calls
    TicTacToeManager ticTacToeManagerRef;


    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        // search for the game system manager in the client side
        foreach (GameObject go in allObjects)
        {
            if (go.GetComponent<GameSystemManager>() != null)
            {
                gameSystemManager = go;
            }          
        }
        ticTacToeManagerRef = gameSystemManager.GetComponent<GameSystemManager>().GetTicTacToeManager.GetComponent<TicTacToeManager>();
        Connect();
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.S))
        //    SendMessageToHost("Hello from client");

        UpdateNetworkConnection();
    }

    private void UpdateNetworkConnection()
    {
        if (isConnected)
        {
            int recHostID;
            int recConnectionID;
            int recChannelID;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("connected.  " + recConnectionID);
                    ourClientID = recConnectionID;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ProcessRecievedMsg(msg, recConnectionID);
                    //Debug.Log("got msg = " + msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    Debug.Log("disconnected.  " + recConnectionID);
                    break;
            }
        }
    }
    
    private void Connect()
    {

        if (!isConnected)
        {
            Debug.Log("Attempting to create connection");

            NetworkTransport.Init();

            ConnectionConfig config = new ConnectionConfig();
            reliableChannelID = config.AddChannel(QosType.Reliable);
            unreliableChannelID = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, maxConnections);
            hostID = NetworkTransport.AddHost(topology, 0);
            Debug.Log("Socket open.  Host ID = " + hostID);

            connectionID = NetworkTransport.Connect(hostID, "10.0.0.3", socketPort, 0, out error); // server is local on network

            if (error == 0)
            {
                isConnected = true;

                Debug.Log("Connected, id = " + connectionID);

            }
        }
    }
    
    public void Disconnect()
    {
        NetworkTransport.Disconnect(hostID, connectionID, out error);
    }
    
    public void SendMessageToHost(string msg)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, connectionID, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);

        string[] csv = msg.Split(',');
        int signifier = int.Parse(csv[0]);

        if (signifier == ServerToClientSignifiers.AccountCreationComplete)
        {
            gameSystemManager.GetComponent<GameSystemManager>().ChangeStates(GameStates.MainMenu);
        } 
        else if (signifier == ServerToClientSignifiers.LoginComplete)
        {
            gameSystemManager.GetComponent<GameSystemManager>().ChangeStates(GameStates.MainMenu);

        }
        else if (signifier == ServerToClientSignifiers.AccountCreationFailed)
        {
            Debug.Log("Account Not Created, please try again");
        }
        else if (signifier == ServerToClientSignifiers.LoginFailed)
        {
            Debug.Log("Login Failed, please try again");
        }
        else if (signifier == ServerToClientSignifiers.OpponentPlay)
        {
            // receive actions from the opponent
            ticTacToeManagerRef.ServerPlacePosition(int.Parse(csv[1]), int.Parse(csv[2]), int.Parse(csv[3]));
        }

        // should the game room be established, start the tic tac toe game
        else if (signifier == ServerToClientSignifiers.GameStart)
        {
            gameSystemManager.GetComponent<GameSystemManager>().ChangeStates(GameStates.TicTacToe);
            ticTacToeManagerRef.PlayerTurn = 1; // set up the turn count
            ticTacToeManagerRef.PlayerID = int.Parse(csv[1]); // set up the player ID

            // TO-DO: on TicTacToe Start, retrieve the list of replays from the server
        }
        else if (signifier == ServerToClientSignifiers.SendMessage)
        {
            Debug.Log("Change Message Here: "+ csv[1]);

            // connect to the Tic Tac Toe Manager through the game manager...
            // might be a cleaner way to do this but this works
            ticTacToeManagerRef.ReceiveMessage(csv[1]);
        }
        else if (signifier == ServerToClientSignifiers.NotifyOpponentWin)
        {
            Debug.Log("Opponent Has Won: " + csv[1]);
            // turn off the buttons so that no one can input in anymore

            ticTacToeManagerRef.ActivateResetButton();
            ticTacToeManagerRef.ActivateSaveReplayButton();
            ticTacToeManagerRef.GameOverOnWin();
            // reset button set active, send the notification to the opponent
        }
        else if (signifier == ServerToClientSignifiers.GameReset)
        {
            Debug.Log("Opponent resets the game");
            ticTacToeManagerRef.ResetGame();

            // add to the list of dropdowns
        }
        else if (signifier == ServerToClientSignifiers.ChangeTurn)
        {
            // ideally, functionality should only work between players 1 and 2
            // 3 and upwards will not have any prompts to interact with the buttons
            ticTacToeManagerRef.CheckTurn(int.Parse(csv[1]));
        }
        else if (signifier == ServerToClientSignifiers.UpdateReplayList)
        {
            Debug.Log("Action: " + csv[1] + "," + csv[2]);
            ticTacToeManagerRef.AddToDropdownMenu(int.Parse(csv[1]), csv[2]);
        }

        else if (signifier == ServerToClientSignifiers.StartReplay)
        {
            Debug.Log("Commence Replay");

            // clean code note: make an enum of states 
            // like GAMEPLAY or REPLAY or SPECTATOR
            ticTacToeManagerRef.IsReplaying = true;
            ticTacToeManagerRef.ReplayMode();
        }
        else if (signifier == ServerToClientSignifiers.ProcessReplay)
        {
            // Replay animation data
            // make a class that holds that
            // rather than immediately displaying the data
            // ticTacToeManagerRef.ServerPlacePosition(int.Parse(csv[1]), int.Parse(csv[2]), int.Parse(csv[3]));
            ticTacToeManagerRef.replayAnimationQueue.Enqueue(new TicTacToePlayAnimation(int.Parse(csv[1]), int.Parse(csv[2]), int.Parse(csv[3])));
        } 
        else if (signifier == ServerToClientSignifiers.EndReplay)
        {
            Debug.Log("End Replay");
            ticTacToeManagerRef.IsReplaying = false;
            ticTacToeManagerRef.ReplayMode();
        }

        #region Spectator Specific functionality
        // ---------------- RECEIVING FOR SPECTATORS ----------------------

        else if (signifier == ServerToClientSignifiers.MidwayJoin)
        {
            Debug.Log("Joining in Midway");
            // TO DO: DEACTIVATE THE BOARD BUT UPDATE CURRENTLY OCCUPIED SPACES
            gameSystemManager.GetComponent<GameSystemManager>().ChangeStates(GameStates.TicTacToe);
            ticTacToeManagerRef.PlayerID = int.Parse(csv[1]); // set up the player ID
            ticTacToeManagerRef.SpectatorShutdown();
        } 
        else if (signifier == ServerToClientSignifiers.UpdateSpectator)
        {
            Debug.Log("Updating from player turn action");
            ticTacToeManagerRef.SpectatorUpdate(int.Parse(csv[1]), int.Parse(csv[2]), int.Parse(csv[3]));
        }
        else if (signifier == ServerToClientSignifiers.ResetSpectator)
        {
            Debug.Log("Reset board");
            ticTacToeManagerRef.ResetButtons();

        }
        #endregion
    }

    public bool IsConnected()
    {
        return isConnected;
    }
}


public static class ClientToServerSignifiers
{
    public const int CreateAccount = 1;
    public const int Login = 2;
    public const int WaitingToJoinGameRoom = 3;
    public const int TicTacToe = 4;

    // Game process actions
    public const int PlayerAction = 5;
    public const int SendPresetMessage = 6;
    public const int PlayerWins = 7;
    public const int ResetGame = 8;

    // Replay System functionality
    public const int SaveReplay = 9;
    public const int RequestReplay = 10;

}
public static class ServerToClientSignifiers
{
    public const int LoginComplete = 1;
    public const int LoginFailed = 2;
    public const int AccountCreationComplete = 3;
    public const int AccountCreationFailed = 4;
    public const int OpponentPlay = 5; // once player makes an action, send an action back to the receiver client
    public const int GameStart = 6;
    public const int SendMessage = 7;
    public const int NotifyOpponentWin = 8; // notify to the opponent that there's a win
    public const int ChangeTurn = 9;
    public const int GameReset = 10;

    // spectator functions
    public const int MidwayJoin = 11;
    public const int UpdateSpectator = 12;
    public const int ResetSpectator = 13;

    // replay functionality
    public const int ProcessReplay = 14;
    public const int UpdateReplayList = 15;
    public const int StartReplay = 16;
    public const int EndReplay = 17; // specific case to end a replay when we're done running through the list so that the clients can reset the board
    public const int SaveReplay = 18;
}