using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Test : MonoBehaviour
    {
    public GameObject Main;

    private void Start()
    {
        Instantiate(Main);
    }
}

