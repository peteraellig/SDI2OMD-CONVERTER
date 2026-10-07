# Low-latency overload handling (1.0.0.8)

The capture callback copies SDK-owned data and returns without waiting for OMT encoding. A single sender worker owns encoding/sending; it never holds the queue lock while calling OMT.

| Situation | Behavior | Effect |
| --- | --- | --- |
| Encoding temporarily falls behind | One waiting video frame is replaced by each newer frame | Motion may skip; old pictures cannot accumulate |
| Short encoding stall | Audio uses its own 120 ms / 5,760 sample buffer | Audio can remain continuous if sending catches up before expiry |
| Sustained overload | Oldest audio exceeding capacity or 120 ms wall age is discarded | Possible audible gap; prevents steadily increasing delay |
| Stale queued video | Discard after two complete frame periods | Fresh picture preferred over delayed playback |
| SDI video disappears | Pending video cleared; audio still processed if supplied by DeckLink | Missing video warning; no deliberate audio shutdown |
| Capture packet cannot be read | Packet skipped and capture error counted | Recovery continues on subsequent packets |
| Out of memory / sender exception | Queue closed, fatal error surfaced, capture stopped | Explicit failure; manual restart required |
| Stop | Discard pending audio/video and wake worker | No draining a stale backlog on shutdown |
| Sender call hangs | Health reports send stalled; Stop kills child after five seconds if needed | Cannot interrupt an in-progress library call safely |

Video expiry is 40 ms at 50 fps, approximately 33.3 ms at 60 fps, 80 ms at 1080i50 and approximately 66.7 ms at 1080i60 (whole-frame periods). Capacity excludes the packet already being sent. Original capture timestamps are preserved; no artificial catch-up timestamps, silence insertion or audio mixing are applied.

Audio packets larger than 120 ms are trimmed to their newest samples. At most 64 queued audio packets guard against pathological tiny packets. A dropped video frame means either a local queue replacement/expiry or a gap detected between valid capture timestamps. Signal absence, intentional stopping and receiver/network-side loss are not added to that counter.

## Useful live indicators

- System CPU percentage and physical RAM percentage/free space, plus encoder-process CPU and working set. Process CPU is normalized across logical processors.
- Local video losses, discarded audio duration and recoverable errors within the last five seconds. These expire even after stopping.
- Startup losses tracked separately until the first telemetry sample at or after two seconds. They do not latch a warning for the session.
- Current signal/fresh-frame state, no receiver, stale telemetry and stalled send are separate current-state warnings. Refresh is once per second; stall threshold is 500 ms and telemetry freshness is 2.5 s.
- Current waiting-video age, queued audio duration and last OMT send-call duration. These are not end-to-end latency measurements.
- Session/startup totals are printed on opening Show list. Detailed logging remains disabled while hidden; health counters remain active.

The small audio reserve is a tradeoff: preserve sound through brief video-encoding stalls while keeping latency bounded. It is not a 120 ms fixed delay; normal audio is sent as soon as the worker is available. Sustained insufficient CPU or a blocked OMT call can interrupt both sound and picture. No automatic quality changes or restart loops are applied.

## Verification

`overload_queue_test` exercises video replacement, audio retention through short stalls, long-stall expiry, capacity bounds and stop wakeup without stressing the user's CPU. `HealthCheck` verifies startup separation, five-second expiry and session reset. Hardware integration and receiver tests are recorded in TESTING.md with their actual scope.
