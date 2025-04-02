using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;
using UnityEngine.TextCore.Text;
using System.Threading;

public class IGUIManager : MonoBehaviour
{

    public int lives;
    public TextMeshProUGUI lifeTracker;
    public GameObject character1;
    public GameObject character2;
    public TextMeshProUGUI timerText;

    private float secondsCount;
    private int minuteCount;

    public PlayerManager playerManager;




    // Start is called before the first frame update
    void Start()
    {
        character1.SetActive(true);
        character2.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        lifeTracker.text = "x" + lives;

        UpdateTimerUI();
    }

    void LoseLife()
    {
        if (lifeTracker != null)
        {
            if (lives != 0)
            {
                lives -= 1;
            }
        }
    }

    public void SwitchCharacterImage()
    {
        character1.SetActive(!character1.activeSelf);
        character2.SetActive(!character2.activeSelf);
    }

    public void UpdateTimerUI()
    {
        //set timer UI
        secondsCount += Time.deltaTime;
        timerText.text = minuteCount + ":" + (int)secondsCount;
        if (secondsCount >= 60)
        {
            minuteCount++;
            secondsCount = 0;
        }
    }
}
