# build-tool for Unity
Tools to automate and streamline Unity builds, initial repository is [unity-build-tools](https://github.com/attilioHimeki/unity-build-tools") by attilioHimeki.

## Enhanced Build Tools

The enhanced build tool lets you setup multiple builds at once, save them for later use, and batch them.

Normally, for each build you'd need to switch platform, change the scripting define symbols manually, wait for compilation, and repeat. For a medium-large project that supports multiple platforms and stores, such as Oath to the Stars, that means a long time wasted waiting for Unity to do its work.

Using this tool, all the setup work will be done just once, and the necessary builds will be generated just by pressing a button, with no further input required.

## Advanced Features

### Addressables
The tool includes support to build addressables

### Command-Line

The tool includes Shell and Bash scripts to create builds via CLI, allowing easy integration to Jenkins, Travis and other CI/CD build systems. You can take a look in the CommandLine folder to get started.
There are scripts to install your preferred Unity version in the build server, run your batched builds, and optionally create UnityPackages if you're working on an extension or library rather than an executable.

## Usage and license
While I'm mostly using this for internal development, you're more than welcome to use this in your Unity project, following the conditions and limitations specified in the LICENSE. 
Also, while not necessary, please make sure to credit me and send your feedback.
