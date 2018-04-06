using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptRunner : MonoBehaviour {

    #region Fields

    public TextAsset script;

    private ScriptEngine _scriptEngine;

    #endregion

    #region CONSTRUCTOR

    // Use this for initialization
    void Start () {
        _scriptEngine = this.gameObject.AddComponent<ScriptEngine>();
        _scriptEngine.Init(Debug.Log);
    }

    #endregion

    #region Methods

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            _scriptEngine.ExecuteScript(this.script.text);
        }
    }
    
    #endregion

}
