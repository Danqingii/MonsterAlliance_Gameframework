//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.FileSystem;
using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(FileSystemComponent))]
    internal sealed class FileSystemComponentInspector : GameFrameworkInspector
    {
        //得到 FileSystemComponent 的 m_FileSystemHelperTypeName指定信息 
        private readonly HelperInfo<FileSystemHelperBase> m_FileSystemHelperInfo = new HelperInfo<FileSystemHelperBase>("FileSystem");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            FileSystemComponent t = (FileSystemComponent)target;

            //创建一组可禁用的组件        //编辑器当前是处于播放模式，还是即将切换到该模式？（只读）
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                m_FileSystemHelperInfo.Draw();
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                //生成一个标签字段。（用于显示只读信息。）
                EditorGUILayout.LabelField("File System Count", t.Count.ToString());

                //得到全部的文件系统 全部绘制出来
                IFileSystem[] fileSystems = t.GetAllFileSystems();
                foreach (IFileSystem fileSystem in fileSystems)
                {
                    DrawFileSystem(fileSystem);
                }
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            m_FileSystemHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            //更新 Component 中Help的属性 类名以及类
            m_FileSystemHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawFileSystem(IFileSystem fileSystem)
        {
            EditorGUILayout.LabelField(fileSystem.FullPath, Utility.Text.Format("{0}, {1} / {2} Files", fileSystem.Access, fileSystem.FileCount, fileSystem.MaxFileCount));
        }
    }
}
