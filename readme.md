# LED Cube

__This Project is in the early development stage.__

This repository contains multiple C# Applications, that are designed to Create, Edit, Save and Stream Animations and FrameSequences to a Led-Cube.

## Project Structure
### Cube.Core
The core library of this project. Contains Models and Domain specific Classes. 

### Cube.Animator
An WPF-Application to Create, Edit and Save static animations.

### Cube.Streamer
An Application that can stream frames and animations via network to the LedCube. It can be extended with different modules, that allow Funktions like FFT-Spectrum visualisation and game streaming.