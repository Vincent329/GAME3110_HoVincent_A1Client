using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PresetMessageButton : MonoBehaviour
{
    // Start is called before the first frame update
    public string PresetMessage;

    private Button buttonComp;
    private NetworkedClient networkedClient;
    void Start()
    {
        buttonComp = GetComponent<Button>();
        networkedClient = FindObjectOfType<NetworkedClient>();

        buttonComp.onClick.AddListener(SendPresetMessageToOpponent);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Send a message over
    /// </summary>
    public void SendPresetMessageToOpponent()
    {
        networkedClient.SendMessageToHost(ClientToServerSignifiers.SendPresetMessage + "," + PresetMessage);
    }
}
