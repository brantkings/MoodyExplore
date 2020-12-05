using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LHH.Unity.Editor
{
    [CustomEditor(typeof(object), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class BehaviourButtonsEditor : UnityEditor.Editor
    {
        private static object[] emptyParamList = new object[0];

        private IList<MethodInfo> methods = new List<MethodInfo>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (methods.Count > 0)
            {
                ShowMethodButtons();
            }
        }

        private void OnEnable()
        {
            methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m =>
             m.GetCustomAttributes(typeof(LHH.Unity.ButtonAttribute), false).Length >= 1 &&
             m.GetParameters().Length == 0 &&
             !m.ContainsGenericParameters
            ).ToList();
        }

        private void ShowMethodButtons()
        {
            foreach (MethodInfo method in methods)
            {
                if (GUILayout.Button(method.Name))
                {
                    method.Invoke(target, emptyParamList);
                }
            }
        }
    }
}

