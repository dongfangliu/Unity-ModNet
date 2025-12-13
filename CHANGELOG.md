# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-12-13

### Added
- Initial release of MODNet for Unity
- Real-time portrait matting using MODNet neural network
- Unity Sentis backend for GPU-accelerated inference
- WebCam demo scene with real-time background removal
- Comprehensive bilingual documentation (English and Chinese)
- MODNetDetector core class for inference
- MODNetCompositor UI component for easy integration
- Debug visualization tools (MODNetDetectorDebug)
- Configuration examples for different quality/performance profiles
- MODNetConfig for automatic resolution and alignment calculations
- MODNetResourceSet ScriptableObject for easy resource management

### Features
- GPU compute shader-based preprocessing
- Automatic dimension alignment to 32-pixel boundaries
- Configurable inference resolution
- High-quality alpha matte output
- Real-time performance on modern GPUs
- Support for webcam and texture inputs

### Dependencies
- Unity 6000.0 or later
- Unity Sentis 2.3.0
- Klak.NNUtils 2.2.1
- Unity Burst 1.8.17
- Unity Collections 2.4.0

### Documentation
- English and Chinese documentation
- Configuration guide with performance optimization tips
- Troubleshooting guide
- API reference
- Quick reference for common configurations
- Getting started guide

## [Unreleased]

### Planned
- Editor tools for model import and configuration
- Additional sample scenes
- Performance profiling tools
- Support for custom model variants
- Mobile platform optimizations
