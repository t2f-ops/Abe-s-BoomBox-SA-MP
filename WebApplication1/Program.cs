using System.ComponentModel;
using YoutubeExplode;
using System.Diagnostics;
using YoutubeExplode.Videos.Streams;
using System.Linq.Expressions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ルーティングを "{videoId}.mp3" に変更
app.MapGet("/{videoId}.mp3", async (HttpContext context,string videoId) =>
{
    try
    {
        var youtube = new YoutubeClient();
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
        var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        var streamUrl = streamInfo.Url;
        var ffmpeg = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
        
                Arguments = $"-i \"{streamUrl}\" -f mp3 -vn -acodec libmp3lame -ab 128k -ar 44100 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = false
            }
        };
        ffmpeg.Start();
        context.Response.ContentType = "audio/mpeg";
        await ffmpeg.StandardOutput.BaseStream.CopyToAsync(context.Response.Body);

        if (!ffmpeg.HasExited) ffmpeg.Kill();
    }
        catch (Exception ex)
    {
        Console.WriteLine($"[Error]: {ex.Message}");
        context.Response.StatusCode = 500;
    }
});

app.Run();