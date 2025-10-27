namespace VirtualQueue.Domain.Enums;

public enum QueueStatus
{
    Waiting = 0,
    Serving = 1,
    Released = 2,
    Dropped = 3
}
