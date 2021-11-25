using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFramework;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(ReferencePoolComponent))]
    public class ReferencePoolComponentInspector : GameFrameworkInspector
    {
        private readonly Dictionary<string, List<ReferencePoolInfo>> m_ReferencePoolInfos = new Dictionary<string, List<ReferencePoolInfo>>(StringComparer.Ordinal);
        private readonly HashSet<string> m_OpenedItems = new HashSet<string>(); //HashSet索引速度O（n）通过哈希算法  索引相对于list巨快

        private SerializedProperty m_EnableStrictCheck; //具体拿到的数据

        private bool m_ShowFullClassName = false;       //是否刷新所有类名 

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ReferencePoolComponent t = (ReferencePoolComponent)target;

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                //创建一个开关
                bool enableStrictCheck = EditorGUILayout.Toggle("Enable Strict Check", t.EnableStrictCheck);
                if (enableStrictCheck != t.EnableStrictCheck)
                {
                    t.EnableStrictCheck = enableStrictCheck;
                }

                EditorGUILayout.LabelField("Reference Pool Count", ReferencePool.Count.ToString());
                m_ShowFullClassName = EditorGUILayout.Toggle("Show Full Class Name", m_ShowFullClassName);

                //获取到引用池全部的数据
                m_ReferencePoolInfos.Clear();
                ReferencePoolInfo[] referencePoolInfos = ReferencePool.GetAllReferencePoolInfos();
                foreach (ReferencePoolInfo referencePoolInfo in referencePoolInfos)
                {
                    string assemblyName = referencePoolInfo.Type.Assembly.GetName().Name;
                    List<ReferencePoolInfo> results = null;
                    if (!m_ReferencePoolInfos.TryGetValue(assemblyName, out results))
                    {
                        results = new List<ReferencePoolInfo>();
                        m_ReferencePoolInfos.Add(assemblyName, results);
                    }

                    results.Add(referencePoolInfo);
                }

                //把引用池数据刷新到面板
                foreach (KeyValuePair<string, List<ReferencePoolInfo>> assemblyReferencePoolInfo in m_ReferencePoolInfos)
                {
                    //得到引用池key 是否搞定
                    bool lastState = m_OpenedItems.Contains(assemblyReferencePoolInfo.Key);

                    //创建一个左侧带有折叠箭头的标签
                    bool currentState = EditorGUILayout.Foldout(lastState, assemblyReferencePoolInfo.Key);
                    if (currentState != lastState)
                    {
                        if (currentState)
                        {
                            m_OpenedItems.Add(assemblyReferencePoolInfo.Key);
                        }
                        else
                        {
                            m_OpenedItems.Remove(assemblyReferencePoolInfo.Key);
                        }
                    }

                    if (currentState)
                    {
                        //开启一个垂直盒子
                        EditorGUILayout.BeginVertical("box");
                        {
                            EditorGUILayout.LabelField(m_ShowFullClassName ? "Full Class Name" : "Class Name", "Unused\tUsing\tAcquire\tRelease\tAdd\tRemove");
                            
                            //排序一下当前的类值
                            assemblyReferencePoolInfo.Value.Sort(Comparison);
                            
                            //绘制引用池信息
                            foreach (ReferencePoolInfo referencePoolInfo in assemblyReferencePoolInfo.Value)
                            {
                                DrawReferencePoolInfo(referencePoolInfo);
                            }

                            //保存文件按钮
                            if (GUILayout.Button("Export CSV Data"))
                            {
                                //生成一个保存按钮文件名
                                string exportFileName = EditorUtility.SaveFilePanel("Export CSV Data", string.Empty, Utility.Text.Format("Reference Pool Data - {0}.csv", assemblyReferencePoolInfo.Key), string.Empty);
                                if (!string.IsNullOrEmpty(exportFileName))
                                {
                                    try
                                    {
                                        int index = 0;
                                        string[] data = new string[assemblyReferencePoolInfo.Value.Count + 1];
                                        data[index++] = "Class Name,Full Class Name,Unused,Using,Acquire,Release,Add,Remove";
                                        foreach (ReferencePoolInfo referencePoolInfo in assemblyReferencePoolInfo.Value)
                                        {
                                            data[index++] = Utility.Text.Format("{0},{1},{2},{3},{4},{5},{6},{7}", 
                                            referencePoolInfo.Type.Name, 
                                            referencePoolInfo.Type.FullName, 
                                            referencePoolInfo.UnusedReferenceCount, 
                                            referencePoolInfo.UsingReferenceCount, 
                                            referencePoolInfo.AcquireReferenceCount, 
                                            referencePoolInfo.ReleaseReferenceCount, 
                                            referencePoolInfo.AddReferenceCount, 
                                            referencePoolInfo.RemoveReferenceCount);
                                        }

                                        File.WriteAllLines(exportFileName, data, Encoding.UTF8);
                                        Debug.Log(Utility.Text.Format("Export reference pool CSV data to '{0}' success.", exportFileName));
                                    }
                                    catch (Exception exception)
                                    {
                                        Debug.LogError(Utility.Text.Format("Export reference pool CSV data to '{0}' failure, exception is '{1}'.", exportFileName, exception));
                                    }
                                }
                            }
                            else
                            {
                                EditorGUILayout.EndVertical();

                                EditorGUILayout.Separator();
                            }
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.PropertyField(m_EnableStrictCheck);
            }
            
            //更新当前应用也就是当前脚本
            serializedObject.ApplyModifiedProperties();

            //重新绘制显示此编辑器的任意检视面板。
            Repaint();
        }

        private void OnEnable()
        {
            m_EnableStrictCheck = serializedObject.FindProperty("m_EnableStrictCheck");
        }
        
        //绘制引用词信息
        private void DrawReferencePoolInfo(ReferencePoolInfo referencePoolInfo)
        {
            EditorGUILayout.LabelField(m_ShowFullClassName ? referencePoolInfo.Type.FullName : referencePoolInfo.Type.Name,
            Utility.Text.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", 
            referencePoolInfo.UnusedReferenceCount, 
            referencePoolInfo.UsingReferenceCount, 
            referencePoolInfo.AcquireReferenceCount,
            referencePoolInfo.ReleaseReferenceCount, 
            referencePoolInfo.AddReferenceCount, 
            referencePoolInfo.RemoveReferenceCount));
        }

        //比较信息
        private int Comparison(ReferencePoolInfo a, ReferencePoolInfo b)
        {
            if (m_ShowFullClassName)
            {
                return a.Type.FullName.CompareTo(b.Type.FullName);
            }
            else
            {
                return a.Type.Name.CompareTo(b.Type.Name);
            }
        }
    }
}