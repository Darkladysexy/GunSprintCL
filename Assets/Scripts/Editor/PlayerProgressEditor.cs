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
            }

            EditorGUILayout.Space();
            if (GUILayout.Button($"Add {k_CheatCoins} coins"))
            {
                progress.AddCoins(k_CheatCoins);
                progress.Save();
            }

            if (GUILayout.Button("Reset all progress")
                && EditorUtility.DisplayDialog("Reset progress", "Delete coins, level, record and upgrades?", "Reset", "Cancel"))
            {
                progress.ResetAll(FindUpgrades());
            }
        }

        private static UpgradeSO[] FindUpgrades()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(UpgradeSO));
            var upgrades = new UpgradeSO[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                upgrades[i] = AssetDatabase.LoadAssetAtPath<UpgradeSO>(AssetDatabase.GUIDToAssetPath(guids[i]));
            }

            return upgrades;
        }
    }
}
