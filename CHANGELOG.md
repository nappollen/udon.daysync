# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2025-12-19

### Added
- **Procedural Stars** - Added procedural stars to the sky shader that appear at night
- **Star Configuration** - Added parameters for star size, density, and brightness

### Changed
- Fixed sun position calculation for extreme latitudes
- Improved performance of sun movement interpolation

## [1.1.1] - 2025-12-19

### Changed
- Fix typo in README
- Update dependencies
- Minor code optimizations

## [1.1.0] - 2025-12-19

### Added

- **Realistic geographic position** - Added latitude and longitude parameters for astronomically accurate sun trajectory
- **Solar declination calculation** - Takes into account Earth's axial tilt (~23.45°) to simulate seasons
- **Equation of time** - Correction for Earth's elliptical orbit
- **Altitude and Azimuth** - Precise calculation of the sun's position in the sky
- **Configurable interpolation speed** - New `lerpSpeed` parameter to adjust movement smoothness
- **True night** - Altitude amplification so the sun goes well below the horizon

### Changed

- Complete refactoring of sun rotation calculation
- Improved custom editor with organized sections
- Updated README documentation

## [1.0.0] - 2025-12-19

### Added

- Initial release
- Basic day/night cycle based on time
- Support for local and VRChat network time
- Smooth sun movement interpolation
- Custom editor for configuration
- Package template created with `nappollen.packager`

