using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class PythonLoader : MonoBehaviour
{
    void Start()
    {
        string script;
        var filename = Application.dataPath + "/test.py";

        using (StreamReader sr = new StreamReader(filename, System.Text.Encoding.UTF8))
        {
            script = sr.ReadToEnd();
        }

        var scriptEngine = IronPython.Hosting.Python.CreateEngine();
        var scriptScope = scriptEngine.CreateScope();
        var scriptSource = scriptEngine.CreateScriptSourceFromString(script);

        scriptSource.Execute(scriptScope);
        // var print_message = scriptScope.GetVariable<Func<object>>("say_hello"); // 找不到此函数。。。
        // print_message();
    }
}