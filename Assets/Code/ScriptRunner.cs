using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptRunner : MonoBehaviour {

    ScriptEngine _scriptEngine;

	// Use this for initialization
	void Start () {
        _scriptEngine = new ScriptEngine(printMessage);
    }
    
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Return))
        {
            Debug.Log("Starting running script. Should Print 'TestPrint' every second, five times.");

            string pythonSource =
                "import time\n" +
                "for x in xrange(0, 5):\n" +
                "    print 'TestPrint'\n" +
                "    time.sleep(1)\n";

            // The issue here is that the whole ExecuteScript will run
            // Until it is finished. I would like to be able to exit it
            // so that Unity can do its rendering and then go back to
            // continuing execution once one frame has been rendered.
            _scriptEngine.ExecuteScript(pythonSource);

            Debug.Log("Finished running script.");
        }
    }

    void printMessage(string s)
    {
        Debug.Log(s);
        // Here is where I would like to rerender the scene or be able to go
        // back to the main loop so that the message gets printed each time.
        // In my application I am not using Debug.Log, but instead a custom
        // print function that prints to the console which is a textfield.
        // The main issue is the same though. All outputs will be printed
        // once the whole "ExecuteScript" function is finished. I want to
        // be able to see each individual print once they happen.
    }
}
