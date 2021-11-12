using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Behaviour for every separate button in the scene.  It does heavily rely on the Tic Tac Toe manager however as it ties into it
/// serves to visually represent the Tic Tac Toe array
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
        ticTacToeManagerRef.Search += SetButtonAtLocation;
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

        if (!CheckIfOccupied())
        {
            SetButtonAtLocation(XPos, YPos, ticTacToeManagerRef.PlayerID);

            // NOTIFIES THE TICTACTOE MANAGER THAT WE'RE PLACING A POSITION
            ticTacToeManagerRef.PlacePosition(XPos, YPos, ticTacToeManagerRef.PlayerID);
        }
        // check active buttons?
    }

    // just to set up a visual
    private void SetButtonAtLocation(int row, int column, int playerID)
    {
        if (row == XPos && column == YPos)
        {
            if (playerID == 1)
            {
                buttonComp.transform.GetChild(0).GetComponent<Text>().text = "O"; // test
            }
            else
            {
                buttonComp.transform.GetChild(0).GetComponent<Text>().text = "X"; // test
            }
            buttonComp.interactable = false;
        }
    }

    private bool CheckIfOccupied()
    {
        if (ticTacToeManagerRef.GetTicTacToeBoard[XPos, YPos] >= 1) // if it's 1 or higher, then the spot is already occupied
        {
            buttonComp.interactable = false;
            return true;
        }
        return false;
    }

    public void ReactivateButtonAtLocation(int row, int column)
    {
        if (row == XPos && column == YPos)
        {
            buttonComp.transform.GetChild(0).GetComponent<Text>().text = ""; // test
            buttonComp.interactable = true;
        }
    }

    // run through for all tic tac toe buttons.
    // call via tic tac toe manager
    public void ButtonReset()
    {
        buttonComp.interactable = true;
    }
}
