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

## 1.0.0.8 overload handling (7 October 2026)

- Native deterministic queue test passed: one fresh pending video, preserved audio through short stalls, bounded audio sample/packet capacity, age expiry after long stalls and worker wakeup on stop.
- Health rollup test passed: startup counters separated, five-second deltas expire without further samples, new session and explicit reset.
- Synthetic 720p50 Normal / Mono 1 loopback passed: 25 video frames and 25 audio packets, received audio samples checked.
- Initial tests without a connected SDI source: six GUI-driven capture starts (Normal → High → Low → High → Low → Normal) with one persistent OMT receiver passed for audio packets; discovery succeeded after every start. Live health, hidden detailed logging and resource metrics were checked. No physical SDI input was connected: received audio may be silence, and physical video/real audio continuity was not validated.
- Native capture in 720p50 and 1080i50 correctly reported no input, no captured video, and zero video-loss/error counters. DeckLink's device-status API reported 1080i50 even without connected input; active capture flags are used for live signal state.
- Actual form rendered at 700/800 pixels with metrics inside the client area.

Reproduce queue test using CMake/CTest (`ctest --test-dir build -C Release`). Health rollup: `dotnet run --project tests/HealthCheck.csproj -c Release`. GUI health integration: add `--health` to ReconnectCheck; use `--audio-only` only when explicitly checking packet reception without valid video. `--1080i50` temporarily selects that capture format. Settings are restored after the test. `--ui` renders the idle form without opening capture.

The queue tests simulate delayed consumption without saturating the workstation. No claim is made about physical SDI continuity under CPU saturation, latency across the network, or vMix reception for this revision.

## Local UI preview: fixed window and status strips (7 October 2026)

- Main window uses a fixed border with maximizing disabled. Previous saved sizes/maximized state are ignored.
- Two bottom StatusStrip controls, each with six aligned cells, passed overflow/client-area checks.
- Separate DiagnosticsForm opens, closes and reopens with the log buffer preserved and the main size unchanged.
- Live 720p50 GUI test passed six starts / five reconnects with one persistent OMT receiver (Normal → High → Low → High → Low → Normal), including video/audio metadata and discovery checks.
- During both Normal phases, opening diagnostics enabled detailed Captured output. Closing stopped detailed output without stopping capture or changing main-window size. System and stream cells continued updating.
- Frontend and test build completed with zero warnings/errors. Native encoder behavior is unchanged for this UI preview.
- Initially verified as a local preview; the tested layout is included in the authorized 1.0.0.8 release.

## Final 1.0.0.8 verification

The connected 720p50 changing-picture source passed six starts and five reconnects with one persistent receiver. During both Normal phases, diagnostics were opened and closed: detailed logging stopped, transmission continued and window size remained fixed. All live counters were zero at the health checkpoints.

| 30-second phase | Received video | Audio packets | Received timestamp gaps | Internal discarded video | Discarded audio | Errors |
| --- | ---: | ---: | --- | ---: | ---: | ---: |
| Normal | 1455 | 1455 | None | 1 | 0 | 0 |
| High | 1495 | 1500 | None | 0 | 0 | 0 |
| Low | 1495 | 1499 | None | 0 | 0 | 0 |

Picture-data samples changed in every phase. All received audio samples were silent. These checks use a local OMT receiver and do not validate audible tone, remote/vMix reception or CPU saturation. The single internal Normal-phase discard caused no gap after the first received video frame; its exact time was not recorded. Encoder-process CPU averaged approximately 1.7% across logical processors. Send-call durations are not end-to-end latency measurements.

The final compact layout includes the user's designer adjustments and left-aligned sender name. InitializeComponent uses standard individual control declarations, properties and event handlers; loops/factory helpers were removed. Runtime metrics/settings loading are suppressed in design mode. Build and rendered UI checks pass; the user subsequently edited the layout in Visual Studio and rebuilt it.
