The pack contains 24 prefabs in the particle sphere style.
Each effect has glow and distortion parameters.
All effects are using 2D sprites.

Package works on unity 5.3+ with any rendering path, color space and api (DirectX 9/11/12, OpenGL, openGLES 2.0/3.0).

For using just drag and drop prefab on scene. Also you can use effects in runtime. Like a "Instantiate(EffectPrefab, position, rotation);"

Demo version includes new unity posteffect bloom from this page https://www.assetstore.unity3d.com/en/#!/content/51515
Just use bloom posteffect on camera. 
NOTE! Camera should have active HDR! (on forward rendering you need disable anti aliasing for correct HDR working)

Settings for bloom

Threshold is 2
Soft knee is 0
Radius is 7
Intencity is 1
High Quality is true
Anti Flicker is true
