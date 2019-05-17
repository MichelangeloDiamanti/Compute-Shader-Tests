# Compute-Shader-Tests

This Unity project is a test-bed for compute shaders. I've made it because I've run into a problem that I cannot figure out. The compute shader seems to crash on some input, maybe it is related to the size of the buffer. To checkout the code:

- $ git clone git@github.com:MichelangeloDiamanti/Compute-Shader-Tests.git
- Open the folder with Unity, it was developed with version 2019.1.xxxx
- Open the "SampleScene" in Assets/Scenes
- Enable the GameObject that you're interested in testing (only one active at a time)
- Hit play.
- If the ComputeShader completed its execution successfully you should have the output in Assets/Resources/OutputData/
- Otherwise the editor will hang/crash

