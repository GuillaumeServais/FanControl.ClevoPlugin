# FanControl.ClevoPlugin

Plugin for [FanControl](https://github.com/Rem0o/FanControl.Releases) that provides experimental fan control support for a Clevo-based laptop.

This project exposes:

- CPU fan control and RPM
- GPU fan control


## Supported Devices

Tested hardware:

- Manufacturer: Notebook
- Model: NJ50_70CU
- BIOS: INSYDE Corp. 1.07.05, 2019-10-24
- SMBIOS: 3.2
- Embedded Controller: 7.07
- CPU: Intel Core i3-10110U
- Windows HAL: 10.0.19041.6456

This plugin is highly model-specific. It may not work on other Clevo models, BIOS versions, EC versions, or OEM rebrands.

The plugin exposes a GPU fan control sensor because the original [ClevoFanControl](https://github.com/djsubtronic/ClevoFanControl/tree/master) project uses fan 1 for CPU and fan 2 for GPU. The tested machine does not have a dedicated GPU fan, so GPU fan control has not been verified on this hardware.

## Installation

1. Download a release package from the GitHub Releases page.
2. Extract the package.
3. Copy the contents of the extracted `release` folder into FanControl's `Plugins` folder.

Expected layout:

```text
FanControl\Plugins\FanControl.ClevoPlugin.dll
FanControl\Plugins\FanControl.ClevoPlugin\FanControl.ClevoHelper.exe
FanControl\Plugins\FanControl.ClevoPlugin\ClevoEcInfo.dll
FanControl\Plugins\FanControl.ClevoPlugin\InsydeDCHU.dll
```

4. Install the NTPort driver if it is not already installed.
5. Start FanControl as administrator.

## Usage

After FanControl starts, the plugin should expose:

- `Clevo CPU Fan Control`
- `Clevo GPU Fan Control`
- `Clevo CPU Fan RPM`

Create a normal FanControl curve using your existing CPU temperature sensor, then target `Clevo CPU Fan Control`.

## How it Works

FanControl is a 64-bit process, while the working `ClevoEcInfo.dll` found in the original ClevoFanControl project is 32-bit. Because a 64-bit process cannot load a 32-bit DLL directly, this project uses a small 32-bit helper process:

```text
FanControl x64
  -> FanControl.ClevoPlugin.dll x64
      -> FanControl.ClevoHelper.exe x86
          -> ClevoEcInfo.dll x86
```

Fan speed control is sent to the helper over a local loopback TCP connection.

Fan RPM is read from `InsydeDCHU.dll` used by the Microsoft Store "Fan Speed Setting" application. On the tested machine, the useful data is returned by `GetDCHU_Data_Buffer(0x0C, buffer)`. The RPM is derived from the big-endian 16-bit value at offset `0x02` with this formula:

```text
RPM = (32768 * 60 / N) * 1.096
```

The `1.096` correction factor was derived experimentally by comparing the plugin probe output with the FanSpeedSetting RPM value on the tested NJ50_70CU laptop.

## Build

### Requirements

- Windows
- Visual Studio / Visual Studio Build Tools with .NET Framework 4.8 build tools
- .NET SDK
- FanControl's `C:\Program Files (x86)\FanControl\FanControl.Plugins.dll`.

### Local dependency setup

Copy the required local DLLs into the `Dlls` folder:

```text
Dlls\FanControl.Plugins.dll
```

### Build command

Run:

```cmd
build-release.cmd
```

The script uses this MSBuild path when available:

```text
C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe
```

If that path does not exist, it falls back to `msbuild` from `PATH`.

The script creates a `release` folder at the repository root:

```text
release\FanControl.ClevoPlugin.dll
release\FanControl.ClevoPlugin\FanControl.ClevoHelper.exe
release\FanControl.ClevoPlugin\ClevoEcInfo.dll
release\FanControl.ClevoPlugin\InsydeDCHU.dll
```

## Known Limitations

- This is experimental hardware-control software.
- It was tested on one machine only.
- The GPU fan control sensor is included for compatibility with ClevoFanControl's two-fan model, but it was not verified on the tested laptop.
- RPM reading depends on the `InsydeDCHU.dll` (used by `FanSpeedSetting.exe`) behavior and may not work on other models.
- Fan control requires administrator privileges and the NTPort driver.

## Sources and Credits

- FanControl plugin system: https://github.com/Rem0o/FanControl.Releases
- Original Clevo fan control logic and `ClevoEcInfo.dll` usage: https://github.com/djsubtronic/ClevoFanControl
- RPM source investigated on the tested laptop: Microsoft Store [Fan Speed Setting](https://apps.microsoft.com/detail/9P6LJMR12RN1?hl=neutral&gl=FR&ocid=pdpshare) from `CLEVO CO.`

## Disclaimer

Use this plugin at your own risk. Incorrect fan control can cause overheating or hardware damage. Always test carefully and keep safe fallback curves in FanControl.
