# Third-party components

The project's original source code and artwork are licensed under [0BSD](LICENSE).

## Open Media Transport

The bundled Windows x64 SDK is Open Media Transport v1.0.0.19:

- `third_party/omt/Libraries/Winx64/libomt.h`
- `third_party/omt/Libraries/Winx64/libomt.lib`
- `third_party/omt/Libraries/Winx64/libomt.dll`
- `third_party/omt/Libraries/Winx64/libvmx.dll`

These files retain their upstream MIT license. The complete license is in
[third_party/omt/LICENSE.txt](third_party/omt/LICENSE.txt) and is included in
published applications as `OMT-LICENSE.txt`. Keep this notice when redistributing
the bundled libraries.

Upstream: https://github.com/openmediatransport/libomtnet
and https://github.com/openmediatransport/libomt.

## Blackmagic Desktop Video

Blackmagic Desktop Video is installed separately by the user. Its driver and
DeckLink COM type library are not included in this repository. Build-time COM
definitions are generated from the installed driver.

## Microsoft runtimes

ClickOnce includes the .NET runtime and checks for the Visual C++ runtime.
Those runtimes retain their respective Microsoft terms; the project's 0BSD
license does not replace third-party licenses.
