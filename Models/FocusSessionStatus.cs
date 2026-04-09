namespace SkyFocus.Models;

public enum FocusSessionStatus
{
    Idle = 0,
    Running = 1,
    Paused = 2,
    Completed = 3,
    Abandoned = 4,
}