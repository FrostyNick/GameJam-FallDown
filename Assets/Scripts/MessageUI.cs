using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageUI : MonoBehaviour
{
    [SerializeField] private MessageUI nextMessage;
    public bool triggeredFinish = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Return)) {
            if(nextMessage){
                nextMessage.gameObject.SetActive(true);
            }else{
                triggeredFinish = true;
            }
            this.gameObject.SetActive(false);
        }
        
    }

    public void isVisible(bool test){

    }
}
