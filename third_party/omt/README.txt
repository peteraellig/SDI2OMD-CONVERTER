================================================
Open Media Transport (OMT) Binary Release README
================================================

This is a prebuild binary release of the Open Media Transport libraries and codecs for Windows and MacOS platforms.

LICENSE
------------------------------------
See LICENSE.txt

SYSTEM REQUIREMENTS
------------------------------------
WINDOWS
* Windows 10 or higher (x64)
* Windows 11 or higher (ARM64)

MACOS
* MacOS Sequoia (15) or higher with Apple Silicon (ARM64)

For other platforms such as Linus, refer to relevant source code repository for build instructions.

CONTENTS
------------------------------------
[libvmx]

Shared library for the VMX Video Codec
This is required when encoding/decoding video, optional when only using audio or metadata.

[libomtnet]

.NET Standard library implementing the OMT protocol

[libomt]

Shared library implementing the OMT protocol for C/C++ applications
Use the included libomt.h and libomt.lib

[omtplugin]

Open Media Transport plugin for OBS
Installation instructions can be found in the README.md file in the omtplugin github repository.

[OMTDiscoveryServer]

Discovery Server for Open Media Transport
For more information, see the README.md file in the OMTDiscoveryServer githug repository.
