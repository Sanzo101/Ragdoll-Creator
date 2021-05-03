using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCharacter : MonoBehaviour
{
    [SerializeField]
    GameObject[] Characters;
    int index =1;
    int oldindex;
    int olderindex;
    [SerializeField]
    ResetScene[] Reset;
    // Start is called before the first frame update
    void Start()
    {
 
    }

    // Update is called once per frame
    void Update()
    {
        oldindex = index - 1;
        olderindex = oldindex - 1;
        if(oldindex < 0)
        {
            oldindex = Characters.Length- 1;
        }
        if(olderindex <0 )
        {
            olderindex = Characters.Length - 1;
        }
        Characters[index].SetActive(true);
        Characters[oldindex].SetActive(false);
        Characters[olderindex].SetActive(false);

        if(Input.GetKeyDown(KeyCode.RightArrow))
        {          
            if (index < Characters.Length - 1)
            {
                index++;
                Reset[index].ResetChar();
            }
            else if(index == Characters.Length - 1)
            {
                index = 0;
            }
        }

    }
}
