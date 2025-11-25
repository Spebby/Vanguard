using UnityEditor;
using UnityEngine;


// Borrowed from https://github.com/SebLague/Slime-Simulation/blob/main/Assets/Scripts/Slime/Editor/SlimeEditor.cs
namespace Editor {
    [CustomEditor(typeof(EnemySpawner))]
    public class EnemySpawnerEditor : UnityEditor.Editor {
        UnityEditor.Editor _settingsEditor;
        bool _settingsFoldout;
        bool _collectionFoldout;

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            EnemySpawner spawner = target as EnemySpawner;

            if (!spawner!.config) return;
            DrawSettingsEditor(spawner.config, ref _settingsFoldout, ref _settingsEditor);
            EditorPrefs.SetBool (nameof (_settingsFoldout), _settingsFoldout);
            
            if (!spawner!.collection) return;
            DrawSettingsEditor(spawner.collection, ref _collectionFoldout, ref _settingsEditor);
            EditorPrefs.SetBool (nameof (_collectionFoldout), _collectionFoldout);
        }

        static void DrawSettingsEditor(Object settings, ref bool foldout, ref UnityEditor.Editor editor) {
            if (!settings) return;
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            if (!foldout) return;
            CreateCachedEditor(settings, null, ref editor);
            editor.OnInspectorGUI();
        }

        void OnEnable () {
            _settingsFoldout = EditorPrefs.GetBool (nameof (_settingsFoldout), false);
            _collectionFoldout = EditorPrefs.GetBool (nameof (_collectionFoldout), false);
        }
    }
}