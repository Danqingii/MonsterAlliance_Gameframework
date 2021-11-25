using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor;

namespace Game.Editor
{
    [CustomEditor(typeof(AStarPath))]
    internal sealed class AStarInspector:GameFrameworkInspector
    {
        private SerializedProperty m_Is2D = null;
        private SerializedProperty m_Width = null;
        private SerializedProperty m_Depth = null;
        private SerializedProperty m_NodeSize = null;
        private SerializedProperty m_Conter = null;
        private SerializedProperty m_Rotation = null;
        private SerializedProperty m_IsDrawMesh = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            AStarPath t = (AStarPath)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(m_Is2D);
                EditorGUILayout.PropertyField(m_Width);
                EditorGUILayout.PropertyField(m_Depth);
                EditorGUILayout.PropertyField(m_NodeSize);
                EditorGUILayout.PropertyField(m_Conter);
                EditorGUILayout.PropertyField(m_Rotation);
                EditorGUILayout.PropertyField(m_IsDrawMesh);
            }
            EditorGUI.EndDisabledGroup();

            //开始绘制
            if (GUILayout.Button("Scan"))
            {
                t.Scan();
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
            m_Is2D = serializedObject.FindProperty("m_Is2D");
            m_Width = serializedObject.FindProperty("m_Width");
            m_Depth = serializedObject.FindProperty("m_Depth");
            m_NodeSize = serializedObject.FindProperty("m_NodeSize");
            m_Conter = serializedObject.FindProperty("m_Conter");
            m_Rotation = serializedObject.FindProperty("m_Rotation");
            m_IsDrawMesh = serializedObject.FindProperty("m_IsDrawMesh");

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}