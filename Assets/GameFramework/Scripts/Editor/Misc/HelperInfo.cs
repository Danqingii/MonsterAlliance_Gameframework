//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UnityGameFramework.Editor
{
    /// <summary>
    /// Helper类的 指定信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class HelperInfo<T> where T : MonoBehaviour
    {
        private const string CustomOptionName = "<Custom>";

        private readonly string m_Name;

        private SerializedProperty m_HelperTypeName; //这个是help的名字
        private SerializedProperty m_CustomHelper;   //这个是具体的定制组件
        private string[] m_HelperTypeNames;          //实现了IHelp的全部接口的类名
        private int m_HelperTypeNameIndex;

        public HelperInfo(string name)
        {
            m_Name = name;

            m_HelperTypeName = null;
            m_CustomHelper = null;
            m_HelperTypeNames = null;
            m_HelperTypeNameIndex = 0;
        }

        public void Init(SerializedObject serializedObject)
        {
            m_HelperTypeName = serializedObject.FindProperty(Utility.Text.Format("m_{0}HelperTypeName", m_Name));
            m_CustomHelper = serializedObject.FindProperty(Utility.Text.Format("m_Custom{0}Helper", m_Name));
        }

        public void Draw()
        {
            //得到一个显示的名字
            string displayName = FieldNameForDisplay(m_Name);
            
            //创建一个通用弹出选择字段。 以参数形式获取当前所选的索引，并返回用户选择的索引。
            int selectedIndex = EditorGUILayout.Popup(Utility.Text.Format("{0} Helper", displayName), m_HelperTypeNameIndex, m_HelperTypeNames);
            if (selectedIndex != m_HelperTypeNameIndex)
            {
                m_HelperTypeNameIndex = selectedIndex;
                
                //Component上的 m_"xx"HelperTypeName 属性赋值  false=null  true=就返回这个实现了IHelp接口的FullName
                m_HelperTypeName.stringValue = selectedIndex <= 0 ? null : m_HelperTypeNames[selectedIndex];
            }
            
            
            if (m_HelperTypeNameIndex <= 0)
            {
                //因为没有选择实现IHelp接口的类 为 SerializedProperty 生成一个字段。
                EditorGUILayout.PropertyField(m_CustomHelper);
                if (m_CustomHelper.objectReferenceValue == null)
                {
                    //提示没有选择可用的IHelp实现
                    EditorGUILayout.HelpBox(Utility.Text.Format("You must set Custom {0} Helper.", displayName), MessageType.Error);
                }
            }
        }

        //更新
        public void Refresh()
        {
            //默认的话 0下标的是用来提示报错的
            List<string> helperTypeNameList = new List<string>
            {
                CustomOptionName
            };

            //得到这个实现了IHelp的全部子类 添加到数组中
            helperTypeNameList.AddRange(Type.GetRuntimeTypeNames(typeof(T)));
            m_HelperTypeNames = helperTypeNameList.ToArray();

            //如果位于 Component上的 m_"xx"HelperTypeName的属性 不为空
            m_HelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_HelperTypeName.stringValue))
            {
                //得到这个IHelp实现类 所在数组中的下标 重新赋值
                m_HelperTypeNameIndex = helperTypeNameList.IndexOf(m_HelperTypeName.stringValue);
                if (m_HelperTypeNameIndex <= 0)
                {
                    m_HelperTypeNameIndex = 0;
                    m_HelperTypeName.stringValue = null;
                }
            }
        }

        private string FieldNameForDisplay(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            string str = Regex.Replace(fieldName, @"^m_", string.Empty);
            str = Regex.Replace(str, @"((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", @" $1").TrimStart();
            return str;
        }
    }
}
