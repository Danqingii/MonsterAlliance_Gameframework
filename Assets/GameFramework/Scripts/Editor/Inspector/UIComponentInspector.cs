//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEditor;
using UnityGameFramework.Runtime;

namespace UnityGameFramework.Editor
{
    [CustomEditor(typeof(UIComponent))]
    internal sealed class UIComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty m_EnableOpenUIFormSuccessEvent = null;
        private SerializedProperty m_EnableOpenUIFormFailureEvent = null;
        private SerializedProperty m_EnableOpenUIFormUpdateEvent = null;
        private SerializedProperty m_EnableOpenUIFormDependencyAssetEvent = null;
        private SerializedProperty m_EnableCloseUIFormCompleteEvent = null;
        private SerializedProperty m_InstanceAutoReleaseInterval = null;
        private SerializedProperty m_InstanceCapacity = null;
        private SerializedProperty m_InstanceExpireTime = null;
        private SerializedProperty m_InstancePriority = null;
        private SerializedProperty m_InstanceRoot = null;
        private SerializedProperty m_UIGroups = null;

        private HelperInfo<UIFormHelperBase> m_UIFormHelperInfo = new HelperInfo<UIFormHelperBase>("UIForm");
        private HelperInfo<UIGroupHelperBase> m_UIGroupHelperInfo = new HelperInfo<UIGroupHelperBase>("UIGroup");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            UIComponent t = (UIComponent)target;

           
            //创建一组可禁用的控件。      //编辑器当前是处于播放模式，还是即将切换到该模式？
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(m_EnableOpenUIFormSuccessEvent);
                EditorGUILayout.PropertyField(m_EnableOpenUIFormFailureEvent);
                EditorGUILayout.PropertyField(m_EnableOpenUIFormUpdateEvent);
                EditorGUILayout.PropertyField(m_EnableOpenUIFormDependencyAssetEvent);
                EditorGUILayout.PropertyField(m_EnableCloseUIFormCompleteEvent);
            }
            EditorGUI.EndDisabledGroup();

            //创建一个用于输入浮点数的延迟文本字段。 用于改变面实例对象池自动释放可释放对象的间隔秒数。
            float instanceAutoReleaseInterval = EditorGUILayout.DelayedFloatField("Instance Auto Release Interval", m_InstanceAutoReleaseInterval.floatValue);
            if (instanceAutoReleaseInterval != m_InstanceAutoReleaseInterval.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceAutoReleaseInterval = instanceAutoReleaseInterval;
                }
                else
                {
                    m_InstanceAutoReleaseInterval.floatValue = instanceAutoReleaseInterval;
                }
            }

            //创建一个用于输入整数的延迟文本字段。 用于改变界面实例对象池的容量。
            int instanceCapacity = EditorGUILayout.DelayedIntField("Instance Capacity", m_InstanceCapacity.intValue);
            if (instanceCapacity != m_InstanceCapacity.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceCapacity = instanceCapacity;
                }
                else
                {
                    m_InstanceCapacity.intValue = instanceCapacity;
                }
            }

            //创建一个用于输入浮点数的延迟文本字段。 用于改变界面实例对象池对象过期秒数。
            float instanceExpireTime = EditorGUILayout.DelayedFloatField("Instance Expire Time", m_InstanceExpireTime.floatValue);
            if (instanceExpireTime != m_InstanceExpireTime.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceExpireTime = instanceExpireTime;
                }
                else
                {
                    m_InstanceExpireTime.floatValue = instanceExpireTime;
                }
            }

            //创建一个用于输入整数的延迟文本字段。 用于改变界面实例对象池的优先级。
            int instancePriority = EditorGUILayout.DelayedIntField("Instance Priority", m_InstancePriority.intValue);
            if (instancePriority != m_InstancePriority.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstancePriority = instancePriority;
                }
                else
                {
                    m_InstancePriority.intValue = instancePriority;
                }
            }

            //创建可禁用的组件  在播放的状态下绘制一次
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(m_InstanceRoot);
                m_UIFormHelperInfo.Draw();
                m_UIGroupHelperInfo.Draw();
                EditorGUILayout.PropertyField(m_UIGroups, true);
            }
            EditorGUI.EndDisabledGroup();

            //绘制一个文本标签 显示组的数量
            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("UI Group Count", t.UIGroupCount.ToString());
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
            m_EnableOpenUIFormSuccessEvent = serializedObject.FindProperty("m_EnableOpenUIFormSuccessEvent");
            m_EnableOpenUIFormFailureEvent = serializedObject.FindProperty("m_EnableOpenUIFormFailureEvent");
            m_EnableOpenUIFormUpdateEvent = serializedObject.FindProperty("m_EnableOpenUIFormUpdateEvent");
            m_EnableOpenUIFormDependencyAssetEvent = serializedObject.FindProperty("m_EnableOpenUIFormDependencyAssetEvent");
            m_EnableCloseUIFormCompleteEvent = serializedObject.FindProperty("m_EnableCloseUIFormCompleteEvent");
            m_InstanceAutoReleaseInterval = serializedObject.FindProperty("m_InstanceAutoReleaseInterval");
            m_InstanceCapacity = serializedObject.FindProperty("m_InstanceCapacity");
            m_InstanceExpireTime = serializedObject.FindProperty("m_InstanceExpireTime");
            m_InstancePriority = serializedObject.FindProperty("m_InstancePriority");
            m_InstanceRoot = serializedObject.FindProperty("m_InstanceRoot");
            m_UIGroups = serializedObject.FindProperty("m_UIGroups");

            m_UIFormHelperInfo.Init(serializedObject);
            m_UIGroupHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            m_UIFormHelperInfo.Refresh();
            m_UIGroupHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
