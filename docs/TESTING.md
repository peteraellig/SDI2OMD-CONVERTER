# Verification: 1.0.0.6

## On-demand diagnostics and card information

- Capture starts with diagnostics disabled when the list is hidden.
- No ongoing counter output or status/log updates were observed while hidden.
- BM card info displayed four devices, live 720p50 signal, detected format,
  capture use on Duo (1) and output use on Duo (2).
- The card-info window refreshed while open. Close stopped the timer and all
  helper processes; no further refreshes occurred afterward.
- Show list enabled live diagnostics; Hide list disabled them again during capture.
- 567 video frames and 578 audio packets were received continuously during the
  GUI test, including opening/closing card info and toggling diagnostics.
- No change to the encoder quality or media queue was introduced by this feature.

## Interlaced support

- 1080i50: 1920x1080, 25 complete frames / 50 fields per second.
- 1080i60: 1920x1080, 30 complete frames / 60 fields per second.
- Capture uses the DeckLink display-mode frame rate and field dominance.
- Full-height UYVY buffers use the OMT Interlaced flag; no deinterlacing.
- Both modes passed High, Normal and Low loopback with 40 video frames and 40 audio packets per case. Alternating scanline values checked that both fields and their line order survived encoding/decoding.
- The installed Duo 2 accepted both modes. No matching physical video source was present, so end-to-end SDI interlaced picture quality remains unverified.

## Persistent receiver after quality changes

One receiver instance was retained across all sender restarts.

| Test | Result |
| --- | --- |
| Synthetic modes | 720p50, 1080i50, 1080i60 |
| Sequence per mode | Normal → High → Low → High → Low → Normal |
| Sender starts / reconnects | 18 / 17, passed |
| Minimum video/audio packets per start | 30 / 30 |
| Source discovered after every restart | Yes |
| First fresh video after startup | About 1.0–1.1 seconds locally |
| Receiver recreated or source reselected | No |
| GUI-driven DeckLink audio, same six steps | Passed, 30 packets per start |
| GUI-driven physical 720p50 SDI video and audio | Passed, six starts / five reconnects, at least 30 video and 30 audio packets per start |

The live 720p50 check was repeated after the SDI source became available on
6 October 2026. Normal → High → Low → High → Low → Normal succeeded with the
same receiver instance and source name. Video and audio returned in about
1.02–1.07 seconds after startup; discovery found the source after every start.
Observed compressed frame means were approximately 175–176 kB on High,
99–100 kB on Low and 149–155 kB on Normal. These sizes depend on image content.
Physical interlaced SDI video remains unverified.

These checks use the bundled OMT receiver SDK on the sender computer. Reception in vMix or another computer needs a check with that actual receiver and a matching live SDI source.

## Reproduce

Build the application first, then:

```powershell
dotnet build .\tests\ReconnectCheck.csproj -c Release
.\tests\bin\Release\net8.0-windows\ReconnectCheck.exe C:\Temp\omt-synthetic.txt --synthetic
```

For GUI-driven capture using the saved input, mode and sender name:

```powershell
.\tests\bin\Release\net8.0-windows\ReconnectCheck.exe C:\Temp\omt-capture.txt
```

The capture test requires a matching SDI source and a free input. Stop normal application capture first. Settings are restored afterward. The explicit `--audio-only` option allows an audio-only check and reports that physical video was not validated.

## UI regression

- Six mode choices, Help and version verified.
- Current `signal` correctly distinguished from cumulative `no signal` count.
- Screenshot rendered from the real Windows Forms application.
