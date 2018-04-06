using System;
using IronPython.Hosting;
using System.Collections.Generic;

/// <summary>
/// Python script engine wrapper which contain the Python scope used in an
/// application.
/// </summary>
public class ScriptEngine
{
    private Microsoft.Scripting.Hosting.ScriptEngine _engine;

    // Main scope
    private Microsoft.Scripting.Hosting.ScriptScope _mainScope;

    public delegate void LogToConsole(string message);
    private event LogToConsole _logToConsole;
    private List<Action> _thingsToDo = new List<Action>();

    /// <summary>
    /// Constructor. Creates a Python engine and a main scope where scripts can
    /// be executed. Also creates modules that can be added in the main scope.
    /// The standard output channel is changed in the constructor so that
    /// logging can be done to the provided console via the LogToConsole
    /// delegate.
    /// </summary>
    /// <param name="logToConsole">
    /// Function used to log to console. Injected to ScriptEngine.\
    /// </param>
    public ScriptEngine(LogToConsole logToConsole)
    {
        _logToConsole = logToConsole;
        _engine = Python.CreateEngine();

        // Create the main scope
        _mainScope = _engine.CreateScope();

        // This expression is used when initializing the scope. Changing the
        // standard output channel. Needs the function 'write' to be defined.
        string initExpression = @"
import sys
sys.stdout = standardOutput";
        _mainScope.SetVariable("standardOutput", this);
        _mainScope.SetVariable("SomeAction", new Action(SomeAction));

        // Run initialization, also executes the main config file.
        ExecuteScript(initExpression);
    }

    public void ExecuteScript(string sourceString)
    {
        Microsoft.Scripting.Hosting.ScriptSource sourceCode =
            _engine.CreateScriptSourceFromString(sourceString);
        try
        {
            sourceCode.Execute(_mainScope);
        }
        catch (Exception e)
        {
            _logToConsole(e.Message);
        }
    }

    public void DoWork()
    {
        if (_thingsToDo.Count > 0)
        {
            Action a = _thingsToDo[0];
            _thingsToDo.RemoveAt(0);
            a();
        }
    }

    public void SomeAction()
    {
        _thingsToDo.Add(() => {
            System.Threading.Thread.Sleep(1000);
        });
    }

    /// <summary>
    /// This function is used by the standard output in a scope. When setting
    /// sys.stdout to 'this', this function will be used for outputs.
    /// </summary>
    /// <param name="s">
    /// String to log to console. Should contain explicit newline characters
    /// for new lines to take affect.
    /// </param>
    public void write(string s)
    {
        _thingsToDo.Add(() => {
            if (_logToConsole != null)
            {
                _logToConsole(s);
            }
        });
    }
}
