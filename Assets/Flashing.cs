using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Flashing : MonoBehaviour
{
    private TextMeshProUGUI mesh;
    private void Start()
    {
        mesh = GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        Color c = mesh.color;
        c.a = Mathf.PingPong(Time.time, 1);
        mesh.color = c;
    }
}
