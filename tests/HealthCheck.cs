using SdiOmt;
static HealthSample Sample(long uptime,long lost,long audio=0,long errors=0)=>new(uptime,100,100,lost,audio,errors,true,10,10,0,3,20,0,2);
static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
var health=new LiveHealth();
health.Add(Sample(1000,6,480,2),1000);
health.Add(Sample(2000,8,960,3),2000);
Check(health.Window(2000)==(0L,0.0,0L),"Startup warning must not latch");
Check(health.StartupVideoLost==8 && health.StartupAudioLostSamples==960 && health.StartupErrors==3,"Startup totals");
health.Add(Sample(3000,10,1440,4),3000);
Check(health.Window(3000)==(2L,10.0,1L),"Recent deltas");
health.Add(Sample(4000,10,1440,4),4000);
Check(health.Window(7999)==(2L,10.0,1L),"Loss stays for five seconds");
Check(health.Window(8000)==(0L,0.0,0L),"Loss expires without receiving new samples");
health.Add(Sample(1000,1),9000);
Check(health.StartupVideoLost==1 && health.Window(9000)==(0L,0.0,0L),"New session reset");
health.Reset();Check(health.Latest is null && health.StartupVideoLost==0,"Explicit reset");
Console.WriteLine("PASS: startup isolation, recent deltas, automatic expiry and session reset");
