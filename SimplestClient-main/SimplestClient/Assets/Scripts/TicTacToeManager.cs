using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeManager : MonoBehaviour
{
    // Start is called before the first frame update // tic tac toe UI
    //
    // [0,0] [1,0] [2,0]
    // [0,1] [1,1] [2,1]
    // [0,2] [1,2] [2,2]
    //
    GameObject button00, button10, button20, button01, button11, button21, button02, button12, button22;

    public Text textDisplay;

    GameObject networkedClient;

    // Game Variables
    [SerializeField] private int playerTurn;
    
    private void OnEnable()
    {
        
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReceiveMessage(string message)
    {
        textDisplay.text = message;
    }
}
