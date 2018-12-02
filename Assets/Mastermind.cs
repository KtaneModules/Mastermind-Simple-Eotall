using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;
using Random = UnityEngine.Random;

public class Mastermind : MonoBehaviour
{

    public KMBombInfo Info;
    public KMBombModule Module;
    public KMAudio Audio;
    public KMSelectable[] Slot; //array for the clickable buttons(or slots)
    public KMSelectable Query; //query-button
    public KMSelectable Submit; //submit-button


    public TextMesh DisplaytextLeft; //left display text
    public TextMesh DisplaytextRight; //right display text
    public TextMesh[] CB; // Colorblind Button 1
    private Color[] SlotColor = { Color.white, Color.magenta, Color.yellow, Color.green, Color.red, Color.blue };

    private int[] SlotNR = { 0, 0, 0, 0, 0 }; //the "value" of every slot (=colors)
    private int[] corrSlot = { 0, 0, 0, 0, 0 }; //array containing the correct value for every slot (0 equals slot 0, 1 equals slot 1...)
    private string[] SlotColors = { "W", "M", "Y", "G", "R", "B" };
    private int PegBlack = 0;
    private int PegWhite = 0;
    private int PegGray = 0;

    private bool isActive = false;
    private bool isSolved = false;
    private bool cbActive = false;

    private static int moduleIdCounter = 1;
    private int moduleId = 0;






    // Loading screen
    void Start()
    {
        CB[0].gameObject.SetActive(false);
        CB[1].gameObject.SetActive(false);
        CB[2].gameObject.SetActive(false);
        CB[3].gameObject.SetActive(false);
        CB[4].gameObject.SetActive(false);

        if (GetComponent<KMColorblindMode>().ColorblindModeActive)
        {
            cbActive = true;
            CB[0].gameObject.SetActive(true);
            CB[1].gameObject.SetActive(true);
            CB[2].gameObject.SetActive(true);
            CB[3].gameObject.SetActive(true);
            CB[4].gameObject.SetActive(true);

        }

        moduleId = moduleIdCounter++;
        Module.OnActivate += Activate;

        for (int i = 0; i < 5; i++)
        {
            corrSlot[i] = Random.Range(0, 6);
        }
        //		Debug.Log ("[Mastermind #{0}] The correct code is {1} {2} {3} {4} {5} ", moduleId, corrSlot[0], corrSlot[1], corrSlot[2], corrSlot[3], corrSlot[4]);
        Debug.Log("[Mastermind Simple #" + moduleId + "] The correct code is " + SlotColors[corrSlot[0]] + " " + SlotColors[corrSlot[1]] + " " + SlotColors[corrSlot[2]] + " " + SlotColors[corrSlot[3]] + " " + SlotColors[corrSlot[4]]);

        DisplaytextLeft.text = " ";
        DisplaytextRight.text = " ";

    }

    // Lights off
    void Awake()
    {

        Query.OnInteract += delegate ()
        {
            handleQuery();
            return false;
        };

        Submit.OnInteract += delegate ()
        {
            handleSubmit();
            return false;
        };



        for (int i = 0; i < 5; i++)
        {
            int j = i;
            Slot[i].OnInteract += delegate ()
            {
                handleSlot(j);
                return false;
            };
        }
    }

    // Lights on
    void Activate()
    {

        isActive = true;

    }

    void handleQuery()
    {

        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Query.transform);
        Query.AddInteractionPunch();

        if (!isActive || isSolved)
            return;
        else
        {


            // substituting variables
            int[] PHSlotNR = { SlotNR[0], SlotNR[1], SlotNR[2], SlotNR[3], SlotNR[4] };
            Debug.Log("[Mastermind Simple #" + moduleId + "] Query: " + SlotColors[PHSlotNR[0]] + " " + SlotColors[PHSlotNR[1]] + " " + SlotColors[PHSlotNR[2]] + " " + SlotColors[PHSlotNR[3]] + " " + SlotColors[PHSlotNR[4]]);
            int[] PHcorrSlot = { corrSlot[0], corrSlot[1], corrSlot[2], corrSlot[3], corrSlot[4] };


            // assigning black pegs
            for (int i = 0; i < 5; i++)
            {
                int j = i;
                if (PHSlotNR[j] < 6)
                {
                    if (PHSlotNR[j] == PHcorrSlot[j])
                    {
                        PegBlack++;
                        PHSlotNR[j] = 6;
                        PHcorrSlot[j] = 7;
                    }
                }
            }


            // assigning white pegs
            for (int i = 0; i < 5; i++)
            {
                int j = i;
                if (PHSlotNR[j] < 6)
                {
                    for (int k = 0; k < 5; k++)
                    {
                        int l = k;
                        if (PHSlotNR[j] == PHcorrSlot[l])
                        {
                            PegWhite++;
                            PHSlotNR[j] = 6;
                            PHcorrSlot[l] = 7;
                        }
                    }
                }
            }


            // assigning gray pegs
            PegGray = (5 - PegBlack - PegWhite);

            Debug.Log("[Mastermind Simple #" + moduleId + "] Query result: " + PegBlack + " correct, " + PegWhite + " correct, but in wrong positions");

            // update display
            DisplaytextLeft.text = PegBlack.ToString();
            DisplaytextRight.text = PegWhite.ToString();

            // reset
            PegBlack = 0;
            PegWhite = 0;
            PegGray = 0;
        }
    }

    void handleSubmit()
    {

        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Submit.transform);
        Submit.AddInteractionPunch();

        if (!isActive || isSolved)
            return;
        else
            for (int i = 0; i < 5; i++)
            {
                int j = i;
                if (SlotNR[j] == corrSlot[j])
                    PegBlack++;
            }
        if (PegBlack == 5)
        {
            Module.HandlePass();
            isSolved = true;
            Debug.Log("[Mastermind Simple #" + moduleId + "] Submit: " + SlotColors[SlotNR[0]] + " " + SlotColors[SlotNR[1]] + " " + SlotColors[SlotNR[2]] + " " + SlotColors[SlotNR[3]] + " " + SlotColors[SlotNR[4]] + " - Code correct. Module solved");

        }
        else
        {
            PegBlack = 0;
            Module.HandleStrike();
            Debug.Log("[Mastermind Simple #" + moduleId + "] Submit: " + SlotColors[SlotNR[0]] + " " + SlotColors[SlotNR[1]] + " " + SlotColors[SlotNR[2]] + " " + SlotColors[SlotNR[3]] + " " + SlotColors[SlotNR[4]] + " - Code incorrect. Strike");
        }

    }

    void handleSlot(int num)
    {

        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Slot[num].transform);

        if (!isActive || isSolved) return;

        else
            SlotNR[num] = SlotNR[num] + 1;
        if (SlotNR[num] > 5)
            SlotNR[num] = 0;
        if (cbActive == true)
        {
            if (SlotColors[SlotNR[num]] == "B")
                CB[num].color = Color.white;
            else
                CB[num].color = Color.black;
            CB[num].text = SlotColors[SlotNR[num]];

        }
       // if  (cbActive == false)
        Slot[num].GetComponent<MeshRenderer>().material.color = SlotColor[SlotNR[num]];
        //		Debug.Log ("[Mastermind Simple #" + moduleId + "] Slot " + (num) + " color is now " + SlotColors[corrSlot[num]]);

    }

    private string TwitchHelpMessage = "Query the currently set colors with !{0} query. Query specific colors with !{0} query r b g y m. Submit the currently set colors with !{0} submit.  Submit specific colors with !{0} submit r b g y m. [Valid colors are R, G, B, M, Y, W]";
    IEnumerator ProcessTwitchCommand(string inputCommand)
    {
        List<string> SlotColorsFull = new List<string>(SlotColors);
        SlotColorsFull.AddRange(new[] { "WHITE", "MAGENTA", "YELLOW", "GREEN", "RED", "BLUE" });

        string[] split = inputCommand.ToUpperInvariant().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if ((split.Length != 1 && split.Length != 6) || (split[0] != "QUERY" && split[0] != "SUBMIT")) yield break;
        yield return null;

        if (split.Length == 6)
        {
            for (int i = 0; i < 5; i++)
            {
                int index = SlotColorsFull.IndexOf(split[i + 1]) % 6;
                if (index < 0)
                {
                    yield return string.Format("sendtochaterror What the hell is color {0}? The only colors I recognize are: Red, Green, Blue, Yellow, Magenta, White", split[i + 1]);
                    yield break;
                }
                while (SlotNR[i] != index)
                {
                    handleSlot(i);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        if (split[0] == "QUERY")
        {
            handleQuery();
        }
        else
        {
            handleSubmit();
        }
        yield return new WaitForSeconds(0.1f);
    }
}
