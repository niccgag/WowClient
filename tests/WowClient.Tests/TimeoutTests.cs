using System.Diagnostics;
using System.Net;
using WowClient.Tests.Helpers;

namespace WowClient.Tests;

public sealed class TimeoutTests
{
    [Fact]
    public async Task TimeoutCoversResponseBodyReads()
    {
        var handler = new FakeHttpMessageHandler();
        handler.EnqueueToken();
        handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new StallingStream()),
        });
        var client = TestClient.Create(handler, options => options.Timeout = TimeSpan.FromMilliseconds(100));

        var stopwatch = Stopwatch.StartNew();
        await Assert.ThrowsAsync<TaskCanceledException>(() => client.GameData.GetItemAsync(1));
        stopwatch.Stop();

        Assert.True(
            stopwatch.Elapsed < TimeSpan.FromSeconds(5),
            $"Expected the configured timeout to abort the stalled body read, but it took {stopwatch.Elapsed}.");
    }

    [Fact]
    public async Task TimeoutCoversOAuthBodyReads()
    {
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new StallingStream()),
        });
        var client = TestClient.Create(handler, options => options.Timeout = TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAsync<TaskCanceledException>(() => client.GameData.GetItemAsync(1));

        Assert.DoesNotContain(
            handler.Requests,
            request => request.Uri.AbsolutePath != "/token");
    }

    private sealed class StallingStream : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException("The stalled stream is asynchronous.");

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return 0;
        }

        public override Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken) =>
            ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
