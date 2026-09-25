namespace GunSprint.Core
{
    /// <summary>
    /// Lifecycle states of a single run. Only the GameSession changes the state;
    /// everything else observes it through the RunStateSO.
    /// </summary>
    public enum GameState
    {
        Playing = 0,
        Paused = 1,
        LevelComplete = 2,
        LevelFailed = 3,
        RunFinished = 4
    }
}
