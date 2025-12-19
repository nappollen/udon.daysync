# Nappollen Udon Day Sync

nappollen.udon.day-sync

## Description

A realistic day/night cycle system for VRChat using UdonSharp. This package calculates sun position based on real astronomical formulas, taking into account geographic location, seasons, and time of day.

### Features

- 🌍 **Configurable geographic position** - Latitude and longitude for realistic sun trajectory
- 🌅 **Real astronomical calculations** - Solar declination, equation of time, altitude and azimuth
- 🕐 **Time synchronization** - Based on local time or VRChat network time (UTC)
- 🔄 **Smooth interpolation** - Smooth sun movement with configurable speed
- 🌙 **True night** - Sun goes below the horizon for realistic darkness
- ✨ **Procedural Stars** - Stars appear at night with configurable density and brightness

## Installation

### Via Package Manager

1. Open the Package Manager window
2. Click the "+" button and select "Add package from git URL..."
3. Enter: `https://github.com/nappollen/udon.daysync.git`

### Via manifest.json

Add the following line to your `Packages/manifest.json`:

```json
"nappollen.udon.daysync": "https://github.com/nappollen/udon.daysync.git"
```

## Usage

### Basic Setup

1. Add the `DaySync` component to a GameObject in your scene
2. Assign your Directional Light (sun) to the "Sun" field
3. Configure the geographic position (latitude/longitude) of your world
4. Choose the time mode (Local or Network)

### Parameters

| Parameter | Description |
|-----------|-------------|
| **Sun** | The Directional Light representing the sun |
| **Lerp Movement** | Enable smooth movement interpolation |
| **Lerp Speed** | Interpolation speed (0.1 - 10) |
| **Latitude** | Latitude in degrees (-90 to 90). Positive = North, Negative = South |
| **Longitude** | Longitude in degrees (-180 to 180). Positive = East, Negative = West |
| **Time Based** | `Local` = system time, `Reference` = VRChat network time |
| **Time Zone** | UTC offset (only in Reference mode) |

### Example Coordinates

| Location | Latitude | Longitude |
|----------|----------|-----------|
| Paris, France | 48.86 | 2.35 |
| Tokyo, Japan | 35.68 | 139.69 |
| New York, USA | 40.71 | -74.01 |
| Sydney, Australia | -33.87 | 151.21 |

### Code Example

```csharp
using Nappollen.Udon.DaySync;

// Access current system time
var daySync = GetComponent<DaySync>();
DateTime currentTime = daySync.GetTime();
```

## Requirements

- Unity 2022.3 or later
- VRChat SDK3 - Worlds
- UdonSharp

## License

See [LICENSE](LICENSE) file for details.

## Changelog

See [CHANGELOG](CHANGELOG.md) file for details.

---

<sub>This package was created with [Nappollen's Packager](https://github.com/nappollen/packager)</sub>

