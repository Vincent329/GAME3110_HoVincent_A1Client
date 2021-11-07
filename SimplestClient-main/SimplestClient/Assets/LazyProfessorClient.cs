
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class LazyProfessorClient : MonoBehaviour
{

    int connectionID;
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5494;
    byte error;
    bool isConnected = false;
    int ourClientID;

    // Start is called before the first frame update
    void Start()
    {
        Connect();
    }

    // Update is called once per frame
    void Update()
    {
        // Password: Password
        if (Input.GetKeyDown(KeyCode.C))
            SendMessageToHost(ClientToServerTransferSignifiers.CreateAccount + ",101334300,Password");
        else if (Input.GetKeyDown(KeyCode.L))
            SendMessageToHost(ClientToServerTransferSignifiers.Login + ",101334300,Password");

        else if (Input.GetKeyDown(KeyCode.M))
            SendMessageToHost(ClientToServerTransferSignifiers.RequestMarkInformation + "");
        else if (Input.GetKeyDown(KeyCode.I))
            SendMessageToHost(ClientToServerTransferSignifiers.RequestAccountInformation + "");

        else if (Input.GetKeyDown(KeyCode.D))
            SendMessageToHost(ClientToServerTransferSignifiers.SubmitDiscordUserName + ",Vince3Ho#9395");
        
        else if (Input.GetKeyDown(KeyCode.R))
            SendMessageToHost(ClientToServerTransferSignifiers.SubmitAssignmentTwoLink + ",https://docs.google.com/document/d/19LMHfVapNLOnsnruTNaau7k75VUXDKtmEx2fvXvNZho/edit");
        
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

            connectionID = NetworkTransport.Connect(hostID, "142.112.20.199", socketPort, 0, out error); 
            
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
        Debug.Log("Server says:  " + msg);
    }

    public bool IsConnected()
    {
        return isConnected;
    }


}


public static class ClientToServerTransferSignifiers
{
    public const int CreateAccount = 1;
    public const int Login = 2;
    public const int RequestAccountInformation = 3;
    public const int RequestMarkInformation = 4;


    public const int SubmitEmail = 101;
    public const int SubmitDiscordUserName = 102;
    public const int SubmitFirstName = 103;
    public const int SubmitLastName = 104;
    public const int SubmitStreamDataLabGitRepoLink = 105;
    public const int SubmitNetworkedServerGitRepoLink = 106;
    public const int SubmitNetworkedClientGitRepoLink = 107;
    public const int SubmitAssignmentOneLink = 108;
    public const int SubmitAssignmentTwoLink = 109;
    public const int SubmitAssignmentThreeLink = 110;

}
