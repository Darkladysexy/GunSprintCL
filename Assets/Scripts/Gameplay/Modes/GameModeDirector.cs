using GunSprint.Core;
using UnityEngine;

namespace GunSprint.Gameplay.Modes
{
    /// <summary>
    /// Strategy for one game mode. The <see cref="GameSession"/> runs the generic lifecycle
    /// (pause, coins, distance, saving) and delegates everything mode-specific here:
    /// building the world and deciding when a run is won, lost or finished.
    /// Adding a mode means adding a director, without touching the session.
    /// </summary>
    public abstract class GameModeDirector : MonoBehaviour
    {
        public abstract GameMode Mode { get; }

        /// <summary>Builds the world and places the gun. Called once from the session's Start.</summary>
        public abstract void BeginRun(GameSession session);

        /// <summary>Called when the gun falls below the kill height.</summary>
        public abstract void HandleGunFell();

        /// <summary>Called once when the run ends for any reason; stop spawning here.</summary>
        public virtual void EndRun()
        {
        }
    }
}
