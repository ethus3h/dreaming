using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneNameString : MonoBehaviour {
    public string SceneName = "";
    public GameObject SceneLabel;

    void Start()
    {
        SceneLabel.GetComponent<TextMesh>().text = SceneName;
    }
}
