//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework.Procedure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(ProcedureComponent))]
    internal sealed class ProcedureComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty m_AvailableProcedureTypeNames = null;  //可获取的流程名字 数组
        private SerializedProperty m_EntranceProcedureTypeName = null;    //进入流程的名字

        private string[] m_ProcedureTypeNames = null;                     //在运行时可以得到的全部在 ProcedureBase名字
        private List<string> m_CurrentAvailableProcedureTypeNames = null; //当前可获得的流程类型名字
        private int m_EntranceProcedureIndex = -1;                        //流程进入的所在下标

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            
            ProcedureComponent t = (ProcedureComponent)target;

            if (string.IsNullOrEmpty(m_EntranceProcedureTypeName.stringValue))
            {
                EditorGUILayout.HelpBox("Entrance procedure is invalid.", MessageType.Error);
            }
            else if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("Current Procedure", t.CurrentProcedure == null ? "None" : t.CurrentProcedure.GetType().ToString());
            }
            
            //创建一组可禁用的组件        //编辑器当前是处于播放模式，还是即将切换到该模式？（只读）
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                GUILayout.Label("Available Procedures", EditorStyles.boldLabel);
                if (m_ProcedureTypeNames.Length > 0)
                {
                    EditorGUILayout.BeginVertical("box");
                    {
                        foreach (string procedureTypeName in m_ProcedureTypeNames)
                        {
                            bool selected = m_CurrentAvailableProcedureTypeNames.Contains(procedureTypeName);
                            if (selected != EditorGUILayout.ToggleLeft(procedureTypeName, selected))
                            {
                                if (!selected)
                                {
                                    m_CurrentAvailableProcedureTypeNames.Add(procedureTypeName);
                                    WriteAvailableProcedureTypeNames();
                                }
                                else if (procedureTypeName != m_EntranceProcedureTypeName.stringValue)
                                {
                                    m_CurrentAvailableProcedureTypeNames.Remove(procedureTypeName);
                                    WriteAvailableProcedureTypeNames();
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    //没有可用的流程 提示
                    EditorGUILayout.HelpBox("There is no available procedure.", MessageType.Warning);
                }
                
                if (m_CurrentAvailableProcedureTypeNames.Count > 0)
                {
                    //分隔器 跟上面绘制的面板属性隔开
                    EditorGUILayout.Separator();

                    //创建一个通用弹出选择字段。
                    int selectedIndex = EditorGUILayout.Popup("Entrance Procedure", m_EntranceProcedureIndex, m_CurrentAvailableProcedureTypeNames.ToArray());
                    
                    //如果当前选择的不是进入流程  就把新选择的改成进入流程 然后赋值给 ProcedureComponent.m_EntranceProcedure属性
                    if (selectedIndex != m_EntranceProcedureIndex)
                    {
                        m_EntranceProcedureIndex = selectedIndex;
                        m_EntranceProcedureTypeName.stringValue = m_CurrentAvailableProcedureTypeNames[selectedIndex];
                    }
                }
                else
                {
                    //给个没有选择进入流程的提示
                    EditorGUILayout.HelpBox("Select available procedures first.", MessageType.Info);
                }
            }
            
            //结束一组组件
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();
            
            //当编译完成时 我们刷新一次 获取全部的流程类
            RefreshTypeNames();
        }

        private void OnEnable()
        {
            m_AvailableProcedureTypeNames = serializedObject.FindProperty("m_AvailableProcedureTypeNames");
            m_EntranceProcedureTypeName = serializedObject.FindProperty("m_EntranceProcedureTypeName");

            RefreshTypeNames();
        }
        
        //更新流程名字
        private void RefreshTypeNames()
        {
            //得到全部的 ProcedureBase 类型的类名
            m_ProcedureTypeNames = Type.GetRuntimeTypeNames(typeof(ProcedureBase));
            ReadAvailableProcedureTypeNames();
            int oldCount = m_CurrentAvailableProcedureTypeNames.Count;
            m_CurrentAvailableProcedureTypeNames = m_CurrentAvailableProcedureTypeNames.Where(x => m_ProcedureTypeNames.Contains(x)).ToList();
            if (m_CurrentAvailableProcedureTypeNames.Count != oldCount)
            {
                WriteAvailableProcedureTypeNames();
            }
            else if (!string.IsNullOrEmpty(m_EntranceProcedureTypeName.stringValue))
            {
                m_EntranceProcedureIndex = m_CurrentAvailableProcedureTypeNames.IndexOf(m_EntranceProcedureTypeName.stringValue);
                if (m_EntranceProcedureIndex < 0)
                {
                    m_EntranceProcedureTypeName.stringValue = null;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        //读取可获取的流程类型
        private void ReadAvailableProcedureTypeNames()
        {
            m_CurrentAvailableProcedureTypeNames = new List<string>();
            
            //数组中元素的个数。 如果该属性包含多个对象，它将返回最小数量的元素。
            int count = m_AvailableProcedureTypeNames.arraySize;
            for (int i = 0; i < count; i++)
            {
                m_CurrentAvailableProcedureTypeNames.Add(m_AvailableProcedureTypeNames.GetArrayElementAtIndex(i).stringValue);
            }
        }
        
        //写入可获取的流程类型名字 也就是把这些流程 写到ProcedureComponent.m_AvailableProcedureTypeNames属性里面
        private void WriteAvailableProcedureTypeNames()
        {
            m_AvailableProcedureTypeNames.ClearArray();
            if (m_CurrentAvailableProcedureTypeNames == null)
            {
                return;
            }

            m_CurrentAvailableProcedureTypeNames.Sort();
            int count = m_CurrentAvailableProcedureTypeNames.Count;
            for (int i = 0; i < count; i++)
            {
                //在数组中的指定索引处插入空元素。
                m_AvailableProcedureTypeNames.InsertArrayElementAtIndex(i);
                
                //返回数组中指定索引处的元素。之后在赋值可得的的流程类型名字
                m_AvailableProcedureTypeNames.GetArrayElementAtIndex(i).stringValue = m_CurrentAvailableProcedureTypeNames[i];
            }

            //如果进入流程不为空
            if (!string.IsNullOrEmpty(m_EntranceProcedureTypeName.stringValue))
            {
                //通过值得到指定的索引
                m_EntranceProcedureIndex = m_CurrentAvailableProcedureTypeNames.IndexOf(m_EntranceProcedureTypeName.stringValue);
                if (m_EntranceProcedureIndex < 0)
                {
                    m_EntranceProcedureTypeName.stringValue = null;
                }
            }
        }
    }
}
