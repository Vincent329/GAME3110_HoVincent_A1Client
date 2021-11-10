using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonData : MonoBehaviour
{
    // Start is called before the first frame update

    // set for every different button
    // we'll be sending these over to the networked client
    [SerializeField] private int XPos;
    [SerializeField] private int YPos;

    private Button buttonComp;

    GameObject networkedClient;

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
    }
}
