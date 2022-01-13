using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Game
{
    public static class XLuaEditorCreateFile
    {
        /* TODO未完善
        [MenuItem("Assets/Create/Lua File",false,3)]
        public static void CreateLuaFile()
        {
            CreateFile("lua");
        }

        [MenuItem("Assets/Create/Lua Text File",false,4)]
        public static void CreateLuaTextFile()
        {
            CreateFile("lua.txt");
        }
        
        public static void CreateFile(string fileEx)
        {
            //获取当前所选择的目录（相对于Assets的路径）
            var selectPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            var path = Application.dataPath.Replace("Assets", "") + "/";
            var newFileName = "new_" + fileEx + "." + fileEx;
            var newFilePath = selectPath + "/" + newFileName;
            var fullPath = path + newFilePath;
            //简单的重名处理
            if (File.Exists(fullPath))
            {
                var newName = "new_" + fileEx + "-" + UnityEngine.Random.Range(0, 100) + "." + fileEx;
                newFilePath = selectPath + "/" + newName;
                fullPath = fullPath.Replace(newFileName, newName);
            }
            
            //如果是空白文件，编码并没有设成UTF-8
            File.WriteAllText(fullPath, "-- test", Encoding.UTF8);
            AssetDatabase.Refresh();
            //选中新创建的文件
            var asset = AssetDatabase.LoadAssetAtPath(newFilePath, typeof(Object));
            Selection.activeObject = asset;
        }
        */
    }
}