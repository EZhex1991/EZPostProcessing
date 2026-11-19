# EZPostProcessing

[中文](README.md)

EZPostProcessing provides custom post-processing effects for Unity's Post-Processing Stack.

Minimum supported Unity version: `2020.3`.

## Effects

### EZColorBasedOutline

Adds an outline based on color tolerance.

![EZColorBasedOutline](Documentation~/Images/EZColorBasedOutline.png)

### EZDepthBasedGradient

Applies a gradient based on scene depth.

![EZDepthBasedGradient](Documentation~/Images/EZDepthBasedGradient.png)

### EZDepthBasedOutline

Adds an outline based on scene depth.

![EZDepthBasedOutline](Documentation~/Images/EZDepthBasedOutline-0.png)
![EZDepthBasedOutline](Documentation~/Images/EZDepthBasedOutline-1.png)

### EZDistortion

Applies screen-space or layer-based distortion.

![EZDistortion](Documentation~/Images/EZDistortion-Layer.png)
![EZDistortion](Documentation~/Images/EZDistortion-Screen.png)

### EZGlow

Provides outer and inner glow effects.

![EZGlow](Documentation~/Images/EZGlow-Outer.png)
![EZGlow](Documentation~/Images/EZGlow-Inner.png)

## Usage

Install Unity's `com.unity.postprocessing` package, then add the EZPostProcessing effects to a Post-process Volume in the Inspector.

The demo scene and supporting assets can be imported from **Window > Package Manager > EZPostProcessing > Samples**.

## Directory structure

```text
EZPostProcessing/
├── Runtime/              Runtime post-processing effects and parameters
├── Editor/               Custom inspectors and editor decorators
├── Shaders/              Shaders used by the effects
├── Samples~/Demo/        Demo scene, materials, textures, and profiles
├── Documentation~/Images Documentation images
├── package.json          Unity Package Manager metadata
├── README.md             Chinese documentation
├── README_EN.md          English documentation
├── CHANGELOG.md          Release history
└── LICENSE               MIT license
```

## License

MIT License. See [LICENSE](LICENSE).
