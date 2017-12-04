using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpeedRoadTexMgr
{
    private static volatile SpeedRoadTexMgr _instance;
    private static object _lock = new object();
    Dictionary<string, Texture2D> map = new Dictionary<string, Texture2D>();
  
    public static SpeedRoadTexMgr Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new SpeedRoadTexMgr();
                }
            }
            return _instance;
        }
    }

    private SpeedRoadTexMgr() { }
    public void Init()
    {
        string fullPath = "Assets//Resources//roadtex//";
        //获取指定路径下面的所有资源文件  
        if (Directory.Exists(fullPath))
        {
            DirectoryInfo direction = new DirectoryInfo(fullPath);
            FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Name.EndsWith(".meta"))
                {
                    continue;
                }
                string k = files[i].Name.Split('.')[0];
                string unityname =  files[i].Directory.Name + "\\" + k;
                Texture2D t = (Texture2D)Resources.Load(unityname);
                map[k] = t;
            }
        }
    }

    public Texture2D GetTex(string name)
    {
        Texture2D t = null;
        if (map.ContainsKey(name))
        {
            t = map[name];
        }
        return t;
    }
}
