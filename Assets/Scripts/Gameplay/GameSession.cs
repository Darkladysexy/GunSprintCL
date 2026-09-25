using GunSprint.Audio;
using GunSprint.Core;
using GunSprint.Data;
using GunSprint.Events;
using GunSprint.Gameplay.Modes;
using UnityEngine;

namespace GunSprint.Gameplay
{
    /// <summary>
    /// Owns the lifecycle of one run in the gameplay scene: picks the mode director, tracks coins
    /// and distance, handles pause, and moves the run to its end state. Mode-specific rules live in
    /// the <see cref="GameModeDirector"/> implementations.
    /// Runs before other scripts so the RunStateSO is reset before any UI reads it.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public class GameSession : MonoBehaviour
    {
        [SerializeField] private PlayerProgressSO _progress;
        [SerializeField] private RunStateSO _runState;
        [SerializeField] private GunController _gun;
        [SerializeField] private TrackPath _track;

        [Tooltip("One director per game mode; the one matching the selected mode is used, the others are disabled.")]
        [SerializeField] private GameModeDirector[] _directors = new GameModeDirector[0];

        [SerializeField] private UpgradeSO _powerUpgrade;

        [Header("Events")]
        [SerializeField] private IntEventChannelSO _enemyKilled;
        [SerializeField] private VoidEventChannelSO _pauseRequested;
        [SerializeField] private VoidEventChannelSO _resumeRequested;

        [Header("Feedback")]
        [SerializeField] private AudioCueSO _winCue;
        [SerializeField] private AudioCueSO _loseCue;

        [Tooltip("Time scale of the short slow motion played when a level is won.")]
        [SerializeField, Range(0.05f, 1f)] private float _victorySlowMotion = 0.3f;

        private GameModeDirector _director;

        public PlayerProgressSO Progress => _progress;

        public RunStateSO RunState => _runState;

        public GunController Gun => _gun;

        public TrackPath Track => _track;

        private void Awake()
        {
            TimeScaleService.Reset();

            GameMode mode = _progress.SelectedMode;
            _director = SelectDirector(mode);
            _runState.BeginRun(mode, _progress.LevelIndex + 1, _progress.BestDistance);
        }

        private void OnEnable()
        {
            _enemyKilled.OnEventRaised += HandleEnemyKilled;
            _pauseRequested.OnEventRaised += Pause;
            _resumeRequested.OnEventRaised += Resume;
            _gun.Fell += HandleGunFell;
        }

        private void OnDisable()
        {
            _enemyKilled.OnEventRaised -= HandleEnemyKilled;
            _pauseRequested.OnEventRaised -= Pause;
            _resumeRequested.OnEventRaised -= Resume;
            _gun.Fell -= HandleGunFell;
        }

        private void Start()
        {
            int powerLevel = _progress.GetUpgradeLevel(_powerUpgrade);
            _gun.Shooter.SpeedMultiplier = _powerUpgrade.GetValue(powerLevel) / 100f;

            if (_director != null)
            {
                _director.BeginRun(this);
            }
            else
            {
                Debug.LogError($"[GameSession] No director for mode {_runState.Mode}.", this);
            }
        }

        private void Update()
        {
            if (_runState.IsPlaying)
            {
                _runState.ReportDistance(_track.GetDistance(_gun.transform.position));
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            // Leaving the app mid-run (call, home button) should never cost the player the run.
            if (isPaused)
            {
                Pause();
                _progress.Save();
            }
        }

        private void OnDestroy()
        {
            _progress.Save();
        }

        public void Pause()
        {
            if (_runState.State != GameState.Playing)
            {
                return;
            }

            TimeScaleService.SetPaused(true);
            _runState.SetState(GameState.Paused);
            _progress.Save();
        }

        public void Resume()
        {
            if (_runState.State != GameState.Paused)
            {
                return;
            }

            TimeScaleService.SetPaused(false);
            _runState.SetState(GameState.Playing);
        }

        /// <summary>Levels mode win: multiplies the coins earned in the level.</summary>
        public void CompleteLevel(int multiplier)
        {
            if (!TryEndRun())
            {
                return;
            }

            int bonus = _runState.CoinsEarned * (Mathf.Max(1, multiplier) - 1);
            _progress.AddCoins(bonus);
            _runState.AddCoinsEarned(bonus);
            _runState.SetLevelResult(multiplier);
            _progress.AdvanceLevel();
            _progress.Save();

            // A short slow motion on the final shot sells the victory; it is cleared by the next scene load.
            TimeScaleService.BeginSlowMotion(_victorySlowMotion);
            PlayCue(_winCue);
            _runState.SetState(GameState.LevelComplete);
        }

        /// <summary>Levels mode loss.</summary>
        public void FailLevel()
        {
            if (!TryEndRun())
            {
                return;
            }

            _progress.Save();
            PlayCue(_loseCue);
            _runState.SetState(GameState.LevelFailed);
        }

        /// <summary>Infinity mode end: stores the distance record.</summary>
        public void FinishRun()
        {
            if (!TryEndRun())
            {
                return;
            }

            bool isNewBest = _progress.TrySubmitDistance(_runState.Distance);
            _runState.SetRunResult(isNewBest);
            _progress.Save();
            PlayCue(_winCue);
            _runState.SetState(GameState.RunFinished);
        }

        private bool TryEndRun()
        {
            // Guards against double endings, e.g. two targets hit or "out of ammo" and "fell" in one frame.
            if (_runState.State != GameState.Playing)
            {
                return false;
            }

            TimeScaleService.SetPaused(false);
            if (_director != null)
            {
                _director.EndRun();
            }

            return true;
        }

        private void HandleEnemyKilled(int coins)
        {
            if (!_runState.IsPlaying)
            {
                return;
            }

            _progress.AddCoins(coins);
            _runState.AddCoinsEarned(coins);
        }

        private void HandleGunFell()
        {
            if (_runState.IsPlaying && _director != null)
            {
                _director.HandleGunFell();
            }
        }

        private GameModeDirector SelectDirector(GameMode mode)
        {
            GameModeDirector selected = null;
            foreach (GameModeDirector director in _directors)
            {
                if (director == null)
                {
                    continue;
                }

                bool isSelected = director.Mode == mode && selected == null;
                director.gameObject.SetActive(isSelected);
                if (isSelected)
                {
                    selected = director;
                }
            }

            return selected;
        }

        private static void PlayCue(AudioCueSO cue)
        {
            if (cue != null)
            {
                cue.Play();
            }
        }
    }
}
