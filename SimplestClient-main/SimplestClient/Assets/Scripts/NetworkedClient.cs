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
            Debug.Log("Opponent Play");
            // receive actions from the opponent
            ticTacToeManagerRef.OpponentPlacePosition(int.Parse(csv[1]), int.Parse(csv[2]), int.Parse(csv[3]));
        }

        // should the game room be established, start the tic tac toe game
        else if (signifier == ServerToClientSignifiers.GameStart)
        {
            gameSystemManager.GetComponent<GameSystemManager>().ChangeStates(GameStates.TicTacToe);
            ticTacToeManagerRef.PlayerID = int.Parse(csv[1]); // set up the player ID

            // over here, assign the player ID value as well
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

            // reset button set active, send the notification to the opponent
        }

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
    public const int PlayerAction = 5;
    public const int SendPresetMessage = 6;
    public const int PlayerWins = 7;
    public const int ResetGame = 8;
    public const int LogAction = 9;
    public const int RequestReplay = 10;

}
public static class ServerToClientSignifiers
{
    public const int LoginComplete = 1;
    public const int LoginFailed = 2;
    public const int AccountCreationComplete = 3;
    public const int AccountCreationFailed = 4;

    public const int OpponentPlay = 5; // receiving the opponent's action
    public const int GameStart = 6;
    public const int SendMessage = 7;
    public const int NotifyOpponentWin = 8; // notify to the opponent that there's a win
    public const int GameReset = 9;

}

