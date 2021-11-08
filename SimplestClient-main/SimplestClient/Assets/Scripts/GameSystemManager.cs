using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystemManager : MonoBehaviour
{
    // login UI
    GameObject submitButton, joinGameButton, userNameInput, passwordInput, loginToggle, createToggle, ticTacToeSquareButton;
    GameObject textNameInfo, textPasswordInfo;

    // chatroom input
    GameObject chatInputField;
    GameObject submitMsgButton, presetMsgButton1, presetMsgButton2, presetMsgButton3, presetMsgButton4, chatMessagePanel;


    // Containing a reference to the network client script
    GameObject networkedClient;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjects)
        {
            // setup for login screen
            if (go.name == "UsernameInputField")
            {
                userNameInput = go;
            }
            
            else if (go.name == "PasswordInputField")
            {
                passwordInput = go;
            }
            else if (go.name == "SubmitButton")
            {
                submitButton = go;
            }
            else if (go.name == "JoinGameRoomButton")
            {
                joinGameButton = go;
            }
            else if (go.name == "TicTacToeSquareButton")
            {
                ticTacToeSquareButton = go;
            }

            else if (go.name == "UsernameText")
            {
                textNameInfo = go;
            }
            else if (go.name == "PasswordText")
            {
                textPasswordInfo = go;
            }
            else if (go.name == "LoginToggle")
            { loginToggle = go; }
            else if (go.name == "CreateToggle")
            { createToggle = go; }
            else if (go.name == "NetworkedClient")
            { networkedClient = go; }

            // chat window
            else if (go.tag == "SubmitMessageButton")
            { submitMsgButton = go; } 
            else if (go.tag == "ChatTextPanel")
            { chatMessagePanel = go; } 
            else if (go.tag == "Preset1")
            { presetMsgButton1 = go; }
            else if (go.tag == "Preset2")
            { presetMsgButton2 = go; }
            else if (go.tag == "Preset3")
            { presetMsgButton3 = go; }
            else if (go.tag == "Preset4")
            { presetMsgButton4 = go; }
            else if (go.tag == "ChatTextBox")
            { chatInputField = go; }

        }
        submitButton.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
        loginToggle.GetComponent<Toggle>().onValueChanged.AddListener(LoginToggleChanged);
        createToggle.GetComponent<Toggle>().onValueChanged.AddListener(CreateToggleChanged);
        joinGameButton.GetComponent<Button>().onClick.AddListener(JoinGameRoomButtonPressed); // on clicking the button, join game roon button pressed function is called
        ticTacToeSquareButton.GetComponent<Button>().onClick.AddListener(TicTacToeSquareButtonPressed); // on clicking the button, join game roon button pressed function is called

        submitMsgButton.GetComponent<Button>().onClick.AddListener(SendChatMessage);

        ChangeStates(GameStates.LoginMenu);
    }

    /// <summary>
    /// When the submit button is pressed, we get the password and username input from the input field text box
    /// send this altogether in one string, with the signifier first, and the name and password.
    /// </summary>
    public void SubmitButtonPressed()
    {
        // we want to send login to server
        string p = passwordInput.GetComponent<InputField>().text;
        string n = userNameInput.GetComponent<InputField>().text;
        
        string msg;

        // choosing whether to create an an account or a login,
        // separation of signifier, name, and password, spitting out the message
       
        if (createToggle.GetComponent<Toggle>().isOn)
            msg = ClientToServerSignifiers.CreateAccount + "," + n + "," + p;
        else
            msg = ClientToServerSignifiers.Login + "," + n + "," + p;

        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(msg);
        Debug.Log(msg);
    }
     public void LoginToggleChanged(bool newValue)
    {
        // we want to send login to server
            
            createToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);        
    }
    
    public void CreateToggleChanged(bool newValue)
    {
        // we want to send login to server
            loginToggle.GetComponent<Toggle>().SetIsOnWithoutNotify(!newValue);
    }

    /// <summary>
    /// depending on what the state of the menu is, change the UI accordingly
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeStates(int newState)
    {
        submitButton.SetActive(false);
        loginToggle.SetActive(false);
        createToggle.SetActive(false);
        userNameInput.SetActive(false);
        passwordInput.SetActive(false);
        joinGameButton.SetActive(false);
        ticTacToeSquareButton.SetActive(false);
        textNameInfo.SetActive(false);
        textPasswordInfo.SetActive(false);


        if (newState == GameStates.LoginMenu)
        {
            submitButton.SetActive(true);
            loginToggle.SetActive(true);
            createToggle.SetActive(true);
            userNameInput.SetActive(true);
            passwordInput.SetActive(true); 
            textNameInfo.SetActive(true);
            textPasswordInfo.SetActive(true);

        }
        else if (newState == GameStates.MainMenu)
        {
            joinGameButton.SetActive(true);
        }
        else if (newState == GameStates.WaitingForPlayers)
        {
            //joinGameButton.SetActive(false);
            // could have a back button
        }
        else if (newState == GameStates.TicTacToe)
        {
            ticTacToeSquareButton.SetActive(true);
        }
    }

    public void JoinGameRoomButtonPressed()
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.WaitingToJoinGameRoom + "");
        ChangeStates(GameStates.WaitingForPlayers);
    }
    public void TicTacToeSquareButtonPressed()
    {
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.TicTacToePlay + "");
        ChangeStates(GameStates.TicTacToe);
    }

    public void SendChatMessage()
    {
        string text = chatInputField.GetComponent<InputField>().text;
        // take the message from 
        string msg;
        msg = ClientToServerSignifiers.SendChatMessage + "," + text; 
        networkedClient.GetComponent<NetworkedClient>().SendMessageToHost(ClientToServerSignifiers.SendChatMessage + "");
    }
}

static public class GameStates
{
    public const int LoginMenu = 1;
    public const int MainMenu = 2;
    public const int WaitingForPlayers = 3;
    public const int TicTacToe = 4;
    public const int Chatroom = 5;
}
