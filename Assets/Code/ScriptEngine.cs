using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using IronPython.Hosting;

/// <summary>
/// Python script engine wrapper which contain the Python scope used in an
/// application.
/// </summary>
public class ScriptEngine : MonoBehaviour
{
    
    #region Fields

    private Microsoft.Scripting.Hosting.ScriptEngine _engine;
    private Microsoft.Scripting.Hosting.ScriptScope _mainScope;

    private int _mainThreadId;

    private System.Action<string> _logCallback;
    private System.Action _joinMainThread;
    private object _joinLock = new object();

    #endregion

    #region CONSTRUCTOR

    private void Awake()
    {
        _mainThreadId = Thread.CurrentThread.ManagedThreadId;
    }
    
    public void Init(System.Action<string> logCallback)
    {
        _logCallback = logCallback;
        _engine = Python.CreateEngine();

        // Create the main scope
        _mainScope = _engine.CreateScope();
        
        // This expression is used when initializing the scope. Changing the
        // standard output channel and referencing UnityEngine assembly.
        string initExpression = @"
import sys
sys.stdout = unity
import clr
clr.AddReference(unityEngineAssembly)";
        _mainScope.SetVariable("unity", new ScriptScope(this));
        _mainScope.SetVariable("unityEngineAssembly", typeof(UnityEngine.Object).Assembly);

        // Run initialization, also executes the main config file.
        ExecuteScript(initExpression);
    }

    #endregion

    #region Properties
    
    public System.Action<string> LogCallback
    {
        get { return _logCallback; }
        set { _logCallback = value; }
    }

    #endregion

    #region Methods

    public void ExecuteScript(string script)
    {
        this.ScriptStart(script);
    }

    public void ExecuteScriptAsync(string script)
    {
        ThreadPool.QueueUserWorkItem(this.ScriptStart, script);
    }

    #endregion

    #region Private Methods For Engine

    private void Update()
    {
        System.Action a;
        lock(_joinLock)
        {
            a = _joinMainThread;
            _joinMainThread = null;
        }

        if(a != null)
        {
            a();
        }
    }

    private void ScriptStart(object token)
    {
        try
        {
            var script = _engine.CreateScriptSourceFromString(token as string);
            script.Execute(_mainScope);
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }

    #endregion

    #region Special Types

    public class ScriptScope
    {

        public ScriptEngine engine;

        public ScriptScope(ScriptEngine engine)
        {
            this.engine = engine;
        }
        
        public void write(string s)
        {
            if (engine._logCallback != null) engine._logCallback(s);
        }

        public void gotoUnityThread(System.Action callback)
        {
            if (callback == null) return;

            if (Thread.CurrentThread.ManagedThreadId == engine._mainThreadId)
            {
                callback();
            }
            else
            {
                lock(engine._joinLock)
                {
                    engine._joinMainThread += callback;
                    System.GC.Collect();
                }
            }
        }

        public void exitUnityThread(System.Action callback)
        {
            if (callback == null) return;

            if (Thread.CurrentThread.ManagedThreadId != engine._mainThreadId)
            {
                callback();
            }
            else
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    callback();
                }, null);
            }
        }

        public WaitForSeconds wait(float seconds)
        {
            return new WaitForSeconds(seconds);
        }
        
        public void coroutine(object f)
        {
            if (Thread.CurrentThread.ManagedThreadId != engine._mainThreadId)
            {
                this.gotoUnityThread(() =>
                {
                    this.coroutine(f);
                });
            }
            else
            {
                var e = f as System.Collections.IEnumerator;
                if (e == null) return;

                engine.StartCoroutine(e);
            }
        }

    }
    
    #endregion

}
