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
        ticTacToeManagerRef.Reset += ReactivateButtonOnReset;
        ticTacToeManagerRef.NextTurn += ButtonOnTurnChange;
        ticTacToeManagerRef.Deactivate += DeactivateButton;
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

        // if the button ISN'T occupied on the board
        if (!CheckIfOccupied())
        {
            SetButtonAtLocation(XPos, YPos, ticTacToeManagerRef.PlayerID);

            // NOTIFIES THE TICTACTOE MANAGER THAT WE'RE PLACING A POSITION
            ticTacToeManagerRef.PlacePosition(XPos, YPos, ticTacToeManagerRef.PlayerID);
        }
        // check active buttons?
    }

    /// <summary>
    /// Sets up a visual representation of the selected button, whether or not the player has pressed it.
    /// upon opponent's selection, a delegate will be called with the opponent's selected row and column
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="playerID"></param>
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

  

    /// <summary>
    /// Function to return the condition if the button on the grid is occupied
    /// </summary>
    /// <returns></returns>
    private bool CheckIfOccupied()
    {
        if (ticTacToeManagerRef.GetTicTacToeBoard[XPos, YPos] >= 1) // if it's 1 or higher, then the spot is already occupied
        {
            buttonComp.interactable = false;
            return true;
        }
        return false;
    }


    // run through for all tic tac toe buttons.
    // call via tic tac toe manager
    public void ReactivateButtonOnReset(int row, int column)
    {
        if (row == XPos && column == YPos)
        {
            buttonComp.transform.GetChild(0).GetComponent<Text>().text = ""; // test
            buttonComp.interactable = true;
        }
    }

    /// <summary>
    /// Reactivate any buttons that haven't been selected yet on player's turn
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="turnArgument"></param>
    public void ButtonOnTurnChange(int row, int column, int turnArgument)
    {
        if (row == XPos && column == YPos)
        {
            if (ticTacToeManagerRef.PlayerID == turnArgument)
            {
                
                buttonComp.interactable = true;
            } else
            {
                buttonComp.interactable = false;
            }
        }
    }

    private void DeactivateButton(int row, int column)
    {
        buttonComp.interactable = false;
    }
}
