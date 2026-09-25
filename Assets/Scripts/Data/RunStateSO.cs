using System;
using GunSprint.Core;
using UnityEngine;

namespace GunSprint.Data
{
    /// <summary>
    /// Shared, observable state of the current run (ScriptableObject variable pattern).
    /// The GameSession and the mode directors write it; the gun and the UI read it and
    /// subscribe to its events, so the UI prefab needs no reference to any scene object.
    /// Events only fire when a value actually changes, which keeps UI rebuilds to a minimum.
    /// </summary>
    [CreateAssetMenu(fileName = "RunState", menuName = "GunSprint/Data/Run State")]
    public class RunStateSO : ScriptableObject
    {
        public event Action<GameState> StateChanged;
        public event Action<int> AmmoChanged;
        public event Action<int> DistanceChanged;
        public event Action<int> EnemiesRemainingChanged;
        public event Action<int> CoinsEarnedChanged;
        public event Action BestDistanceBeaten;

        public GameMode Mode { get; private set; }

        public GameState State { get; private set; }

        /// <summary>One-based level number shown to the player (Levels mode).</summary>
        public int LevelNumber { get; private set; }

        public bool UsesAmmo { get; private set; }

        public int Ammo { get; private set; }

        /// <summary>Furthest distance reached along the track, in meters.</summary>
        public float Distance { get; private set; }

        public int DistanceMeters { get; private set; }

        /// <summary>Record at the start of the run (Infinity mode).</summary>
        public float BestDistance { get; private set; }

        public bool HasBeatenBest { get; private set; }

        public int EnemiesRemaining { get; private set; }

        /// <summary>Coins earned during this run, including the finish multiplier bonus.</summary>
        public int CoinsEarned { get; private set; }

        public int Multiplier { get; private set; }

        public bool IsNewBest { get; private set; }

        public bool IsPlaying => State == GameState.Playing;

        /// <summary>
        /// Resets every value for a new run. Assets keep their memory between Play sessions
        /// in the Editor, so a run must always start here.
        /// </summary>
        public void BeginRun(GameMode mode, int levelNumber, float bestDistance)
        {
            Mode = mode;
            LevelNumber = levelNumber;
            BestDistance = bestDistance;
            UsesAmmo = false;
            Ammo = 0;
            Distance = 0f;
            DistanceMeters = 0;
            HasBeatenBest = false;
            EnemiesRemaining = 0;
            CoinsEarned = 0;
            Multiplier = 1;
            IsNewBest = false;
            State = GameState.Playing;
            StateChanged?.Invoke(State);
        }

        public void SetState(GameState state)
        {
            if (State == state)
            {
                return;
            }

            State = state;
            StateChanged?.Invoke(state);
        }

        /// <summary>Enables limited ammo (Infinity mode) with the given amount.</summary>
        public void SetAmmo(int amount)
        {
            UsesAmmo = true;
            Ammo = Mathf.Max(0, amount);
            AmmoChanged?.Invoke(Ammo);
        }

        public void AddAmmo(int amount)
        {
            if (UsesAmmo && amount > 0)
            {
                SetAmmo(Ammo + amount);
            }
        }

        /// <summary>Returns true when a shot may be fired; always true without limited ammo.</summary>
        public bool TryConsumeAmmo()
        {
            if (!UsesAmmo)
            {
                return true;
            }

            if (Ammo <= 0)
            {
                return false;
            }

            Ammo--;
            AmmoChanged?.Invoke(Ammo);
            return true;
        }

        /// <summary>Records progress along the track; only the furthest point counts.</summary>
        public void ReportDistance(float meters)
        {
            if (meters <= Distance)
            {
                return;
            }

            Distance = meters;
            int wholeMeters = Mathf.FloorToInt(meters);
            if (wholeMeters != DistanceMeters)
            {
                DistanceMeters = wholeMeters;
                DistanceChanged?.Invoke(wholeMeters);
            }

            if (!HasBeatenBest && BestDistance > 0f && meters > BestDistance)
            {
                HasBeatenBest = true;
                BestDistanceBeaten?.Invoke();
            }
        }

        public void SetEnemiesRemaining(int count)
        {
            if (EnemiesRemaining == count)
            {
                return;
            }

            EnemiesRemaining = count;
            EnemiesRemainingChanged?.Invoke(count);
        }

        public void AddCoinsEarned(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CoinsEarned += amount;
            CoinsEarnedChanged?.Invoke(CoinsEarned);
        }

        public void SetLevelResult(int multiplier)
        {
            Multiplier = Mathf.Max(1, multiplier);
        }

        public void SetRunResult(bool isNewBest)
        {
            IsNewBest = isNewBest;
        }
    }
}
