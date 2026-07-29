using backend.Models.Enums;

namespace backend.Helpers;

public static class QueueTypeHelper
{
    public static QueueType QueueIdToQueueType(int queueId)
    {
        if (!Enum.IsDefined(typeof(QueueType), queueId))
            throw new ArgumentOutOfRangeException(nameof(queueId));

        return (QueueType)queueId;
    }

    public static int QueueTypeToQueueId(QueueType queueType)
    {
        if (!Enum.IsDefined(queueType))
            throw new ArgumentOutOfRangeException(nameof(queueType));

        return (int)queueType;
    }
}