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

    TicTacToeManager ticTacToeManagerRef;

    void Start()
    {
        buttonComp = GetComponent<Button>();
        buttonComp.onClick.AddListener(OnButtonClicked);
        ticTacToeManagerRef = FindObjectOfType<TicTacToeManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // keep updating???
    }

    private void OnButtonClicked()
    {
        Debug.Log(XPos + "," + YPos);
        //buttonComp.interactable = false;
        // check the manager ref if the icon is filled first

        if (ticTacToeManagerRef.GetTicTacToeBoard[XPos, YPos] >= 1) // if it's 1 or higher, then the spot is already occupied
        {
            buttonComp.interactable = false;
            return;
        }
        else
        {
            if (ticTacToeManagerRef.PlayerID == 1)
            {
                buttonComp.transform.GetChild(0).GetComponent<Text>().text = "O"; // test
            }
            else
            {
                buttonComp.transform.GetChild(0).GetComponent<Text>().text = "X"; // test
            }
            ticTacToeManagerRef.PlacePosition(XPos, YPos, ticTacToeManagerRef.PlayerID);
            buttonComp.interactable = false;
        }
    }

    public void ButtonReset()
    {
        buttonComp.interactable = true;
    }
}
