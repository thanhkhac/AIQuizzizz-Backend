using CleanArchitectureBase.Application.Common.Settings;
using CleanArchitectureBase.Infrastructure.MediaStorage;
using Microsoft.Extensions.Options;

namespace CleanArchitectureBase.Application.UnitTests.Tests;

public class S3PresignedUrlSignerTests
{
    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static S3PresignedUrlSigner Create(DateTimeOffset now, string endpoint = "http://100.86.165.118:8090") =>
        new(Options.Create(new MediaSettings
        {
            PublicS3Endpoint = endpoint,
            S3Bucket = "aiquizz-media",
            S3Region = "us-east-1",
            S3AccessKey = "AKIDEXAMPLE",
            S3SecretKey = "wJalrXUtnFEMI/K7MDENG+bPxRfiCYEXAMPLEKEY",
            PresignWindowMinutes = 5
        }), new FixedTimeProvider(now));

    [Test]
    public void Url_HasSigV4QueryAndPublicHost()
    {
        var url = Create(new DateTimeOffset(2026, 10, 2, 8, 3, 0, TimeSpan.Zero)).GetUrl("img/abc.jpg", TimeSpan.FromMinutes(15));

        url.Should().StartWith("http://100.86.165.118:8090/aiquizz-media/img/abc.jpg?");
        url.Should().Contain("X-Amz-Algorithm=AWS4-HMAC-SHA256");
        url.Should().Contain("X-Amz-Credential=AKIDEXAMPLE%2F20261002%2Fus-east-1%2Fs3%2Faws4_request");
        url.Should().Contain("X-Amz-Date=20261002T080000Z"); // làm tròn theo cửa sổ 5 phút
        url.Should().Contain("X-Amz-Expires=1200"); // 15 phút + 1 cửa sổ
        url.Should().Contain("X-Amz-SignedHeaders=host");
        url.Should().MatchRegex("X-Amz-Signature=[0-9a-f]{64}$");
    }

    [Test]
    public void SameWindow_SameUrl_ForBrowserCaching()
    {
        var u1 = Create(new DateTimeOffset(2026, 10, 2, 8, 0, 10, TimeSpan.Zero)).GetUrl("vid/x.mp4", TimeSpan.FromHours(2));
        var u2 = Create(new DateTimeOffset(2026, 10, 2, 8, 4, 50, TimeSpan.Zero)).GetUrl("vid/x.mp4", TimeSpan.FromHours(2));
        var u3 = Create(new DateTimeOffset(2026, 10, 2, 8, 5, 1, TimeSpan.Zero)).GetUrl("vid/x.mp4", TimeSpan.FromHours(2));

        u1.Should().Be(u2);
        u3.Should().NotBe(u1);
    }

    [Test]
    public void DifferentKeys_DifferentSignatures()
    {
        var signer = Create(DateTimeOffset.UtcNow);
        signer.GetUrl("img/a.jpg", TimeSpan.FromMinutes(5)).Should().NotBe(signer.GetUrl("img/b.jpg", TimeSpan.FromMinutes(5)));
    }
}
