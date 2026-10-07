namespace SdiOmt;
internal sealed record HealthSample(long UptimeMs, long Captured, long Sent, long VideoLost,
    long AudioLostSamples, long Errors, bool Signal, long VideoAgeMs, long SendAgeMs,
    long SendBusyMs, long EncodeMs, long AudioQueueMs, long VideoQueueMs, long Connections);
internal sealed class LiveHealth
{
    private record Interval(long Time, long Video, long Audio, long Errors);
    private readonly Queue<Interval> recent=new();
    private HealthSample? previous;
    private bool warmedUp;
    public HealthSample? Latest { get; private set; }
    public long LastReceivedMs { get; private set; }
    public long StartupVideoLost { get; private set; }
    public long StartupAudioLostSamples { get; private set; }
    public long StartupErrors { get; private set; }
    public void Reset(){ recent.Clear(); previous=Latest=null; warmedUp=false; LastReceivedMs=0; StartupVideoLost=StartupAudioLostSamples=StartupErrors=0; }
    public void Add(HealthSample sample,long now)
    {
        if(previous is not null && sample.UptimeMs<previous.UptimeMs) Reset();
        var video=Math.Max(0,sample.VideoLost-(previous?.VideoLost??0));
        var audio=Math.Max(0,sample.AudioLostSamples-(previous?.AudioLostSamples??0));
        var errors=Math.Max(0,sample.Errors-(previous?.Errors??0));
        if(!warmedUp){StartupVideoLost+=video;StartupAudioLostSamples+=audio;StartupErrors+=errors;if(sample.UptimeMs>=2000)warmedUp=true;}
        else if(video+audio+errors>0)recent.Enqueue(new(now,video,audio,errors));
        previous=Latest=sample;LastReceivedMs=now;
    }
    public (long Video, double AudioMs, long Errors) Window(long now)
    {
        while(recent.Count>0 && now-recent.Peek().Time>=5000)recent.Dequeue();
        long video=0,audio=0,errors=0;
        foreach(var interval in recent){video+=interval.Video;audio+=interval.Audio;errors+=interval.Errors;}
        return (video,audio/48.0,errors);
    }
}
