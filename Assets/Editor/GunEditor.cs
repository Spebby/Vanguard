using UnityEditor;
using UnityEngine;


// Borrowed from https://github.com/SebLague/Slime-Simulation/blob/main/Assets/Scripts/Slime/Editor/SlimeEditor.cs
namespace Editor {
    [CustomEditor(typeof(Gun))]
    public class GunEditor : UnityEditor.Editor {
        UnityEditor.Editor _settingsEditor;
        bool _settingsFoldout;

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            Gun gun = target as Gun;

            if (!gun!.config) return;
            DrawSettingsEditor(gun.config, ref _settingsFoldout, ref _settingsEditor);
            EditorPrefs.SetBool (nameof (_settingsFoldout), _settingsFoldout);
        }

        void DrawSettingsEditor(Object settings, ref bool foldout, ref UnityEditor.Editor editor) {
            if (!settings) return;
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            if (!foldout) return;
            CreateCachedEditor(settings, null, ref editor);
            editor.OnInspectorGUI();
        }

        void OnEnable () {
            _settingsFoldout = EditorPrefs.GetBool (nameof (_settingsFoldout), false);
        }
    }
}