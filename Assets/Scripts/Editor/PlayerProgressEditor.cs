using GunSprint.Data;
using UnityEditor;
using UnityEngine;

namespace GunSprint.Editor
{
    /// <summary>
    /// Shows the saved progress and offers test cheats. These tools used to be keyboard shortcuts
    /// inside the game (N wiped the save!), which could ship to players; now they are Editor-only.
    /// </summary>
    [CustomEditor(typeof(PlayerProgressSO))]
    public class PlayerProgressEditor : UnityEditor.Editor
    {
        private const int k_CheatCoins = 1000;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var progress = (PlayerProgressSO)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Saved progress (PlayerPrefs)", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.IntField("Coins", progress.Coins);
                EditorGUILayout.IntField("Next level", progress.LevelIndex + 1);
                EditorGUILayout.FloatField("Best distance (m)", progress.BestDistance);
                EditorGUILayout.Toggle("Sound on", progress.IsSoundOn);
                EditorGUILayout.EnumPopup("Last mode", progress.SelectedMode);
                EditorGUILayout.TextField("Equipped gun", progress.EquippedGunId);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button($"Add {k_CheatCoins} coins"))
            {
                progress.AddCoins(k_CheatCoins);
                progress.Save();
            }

            if (GUILayout.Button("Reset all progress")
                && EditorUtility.DisplayDialog("Reset progress", "Delete coins, level, record, upgrades and guns?", "Reset", "Cancel"))
            {
                progress.ResetAll(FindAssets<UpgradeSO>(), FindAssets<GunDefinitionSO>());
            }
        }

        private static T[] FindAssets<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            var assets = new T[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                assets[i] = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[i]));
            }

            return assets;
        }
    }
}
