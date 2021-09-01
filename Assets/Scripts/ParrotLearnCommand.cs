using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParrotLearnCommand : MonoBehaviour
{
    public RFPlayerConnection rfplayer;
    public TMPro.TMP_Dropdown command;
    public TMPro.TMP_InputField id;
    public TMPro.TMP_InputField reminder;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            var cmd = "PARROTLEARN  ID " + id.text + " " + command.options[command.value].text;
            cmd += string.IsNullOrWhiteSpace(reminder.text) ? "" : " [" + reminder.text + "]";
            rfplayer.SendCommand(cmd);
        });
    }
}
