using System;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEditor;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 生成XLua配置
    /// </summary>
    public static class XLuaConfigGenerator
    {
        private static string ReadPath =  $"{Application.dataPath}/GameMain/Scripts/LuaScripts";  //读取Lua脚本路径
        private static string GeneratePath =  $"{Application.dataPath}/GameMain/Configs";         //生成配置路径
        private const string LuaScriptNameConfig = "LuaScriptNameConfig.json";
        
        private static readonly List<string> m_CheckLuaScriptNames = new List<string>();

        [MenuItem("Game/Generate LuaConfig")]
        public static void GenerateLuaConfig()
        {
            m_CheckLuaScriptNames.Clear();
            if (Directory.Exists(ReadPath))
            {
                DirectoryInfo direction = new DirectoryInfo(ReadPath);  
                FileInfo[] files = direction.GetFiles("*",SearchOption.AllDirectories);
  
                for(int i = 0 ; i < files.Length ; i++)
                {  
                    //只有.lua文件可以被 添加进入  如果要.lua.txt 可以直接写
                    if (files[i].Name.EndsWith(".lua"))
                    {  
                        string[] paths = files[i].FullName.Split("LuaScripts");
                        string path = paths[1].Substring(1, paths[1].Length - 1); 
                        string scriptName = path.Replace("\\","/").Split('.')[0];
                    
                        //具体通过核查解析过的名字路径
                        m_CheckLuaScriptNames.Add(scriptName);
                    }
                }  
            }

            if (m_CheckLuaScriptNames.Count > 0)
            {
                if (Directory.Exists(ReadPath))
                {
                    string configPath = $"{GeneratePath}/{LuaScriptNameConfig}";
                    if (Directory.Exists(configPath))
                    {
                        Directory.Delete(configPath);
                    }
                    else
                    {
                        string json = JsonMapper.ToJson(m_CheckLuaScriptNames.ToArray());
                        try
                        {
                            StreamWriter sw = new StreamWriter(configPath);
                            sw.Write(json);
                            sw.Close();
                            
                            AssetDatabase.Refresh();
                            Debug.Log($"生成Lua配置成功 : {json}");
                        }
                        catch (Exception e)
                        {
                            Debug.Log($"生成Lua配置失败 : {e}");
                        }
                    }
                }
            }
        }
    }
}
