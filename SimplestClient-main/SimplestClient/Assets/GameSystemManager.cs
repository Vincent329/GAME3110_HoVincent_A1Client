using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystemManager : MonoBehaviour
{
    GameObject submitButton, userNameInput, passwordInput, loginToggle, createToggle;
    GameObject networkedClient;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach(GameObject go in allObjects)
        {
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
            else if (go.name == "LoginToggle")
            { loginToggle = go; }
            else if (go.name == "CreateToggle")
            { createToggle = go; }
            else if (go.name == "NetworkedClient")
            { networkedClient = go; }
        }
        submitButton.GetComponent<Button>().onClick.AddListener(SubmitButtonPressed);
        loginToggle.GetComponent<Toggle>().onValueChanged.AddListener(LoginToggleChanged);
        createToggle.GetComponent<Toggle>().onValueChanged.AddListener(CreateToggleChanged);
    }
    public void SubmitButtonPressed()
    {
        // we want to send login to server
        string p = passwordInput.GetComponent<InputField>().text;
        string n = userNameInput.GetComponent<InputField>().text;
        
        string msg;

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

}
