using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Behaviour for every separate button in the scene.  It does heavily rely on the Tic Tac Toe manager however as it ties into it
/// </summary>
public class ButtonData : MonoBehaviour
{
    // Start is called before the first frame update

    // set for every different button
    // we'll be sending these over to the networked client
    [SerializeField] private int XPos;
    [SerializeField] private int YPos;

    // the main interaction piece
    private Button buttonComp;

    // refer to the tic tac toe manager instead, which has the connection to GameSystemManager.
    // but also try to find ways to decouple... worry about that later

    TicTacToeManager ticTacToeManager;

    void Start()
    {
        buttonComp = GetComponent<Button>();
        buttonComp.onClick.AddListener(OnButtonClicked);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonClicked()
    {
        Debug.Log(XPos + "," + YPos);
        //buttonComp.interactable = false;
    }

    public void ButtonReset()
    {
        buttonComp.interactable = true;
    }
}
