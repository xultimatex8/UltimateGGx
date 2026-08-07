using backend.Helpers;
using backend.Models.Enums;
using AwesomeAssertions;

namespace backend.Tests.Helpers;

public class QueueTypeHelperTests
{
    [Theory]
    [InlineData(400, QueueType.DRAFT_PICK)]
    [InlineData(420, QueueType.RANKED_SOLO)]
    [InlineData(440, QueueType.RANKED_FLEX)]
    public void QueueIdToQueueType_WithValidId_ReturnsExpectedQueueType(int queueId, QueueType expected)
    {
        QueueType result = QueueTypeHelper.QueueIdToQueueType(queueId);

        result.Should().Be(expected);
    }

    [Fact]
    public void QueueIdToQueueType_WithUnknownId_ThrowsArgumentOutOfRangeException()
    {
        Action act = () => QueueTypeHelper.QueueIdToQueueType(999);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(QueueType.DRAFT_PICK, 400)]
    [InlineData(QueueType.RANKED_SOLO, 420)]
    [InlineData(QueueType.RANKED_FLEX, 440)]
    public void QueueTypeToQueueId_WithValidQueueType_ReturnsExpectedId(QueueType queueType, int expected)
    {
        int result = QueueTypeHelper.QueueTypeToQueueId(queueType);

        result.Should().Be(expected);
    }

    [Fact]
    public void QueueTypeToQueueId_WithUndefinedEnumValue_ThrowsArgumentOutOfRangeException()
    {
        var invalidQueueType = (QueueType)9999;

        Action act = () => QueueTypeHelper.QueueTypeToQueueId(invalidQueueType);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}