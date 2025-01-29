# FSX_SImConnect

This project emerged as an alternative to FSUIPC and Link2FS tool for it's use in the development of a flight simulator without the need to rely on the limited parameters offered by them.
For this I had to carry out an investigation on how FSX worked as well as the existence of its API and how to connect to it through a client. Once the connection with the API was achieved, it was possible to extract the data and send it to an Arduino platform to give it a more practical use.

## Overview
Microsoft Flight Simulator X (FSX) works internally by running the main game engine and exposing an API called SimConnect, which is a .dll file that allows external applications to interact with the simulator.
- FSX.exe (Main game): The core FSX executable runs the physics engine, rendering, AI traffic, weather, and other simulation aspects.
- SimConnect.dll (SimConnect API): SimConnect is the official API provided by FSX. It enables external applications, plugins, or tools to interact with the simulator.
Communication Between FSX and External Applications: Applications use SimConnect via the SimConnect SDK. It works via Inter-Process Communication (IPC) using Windows messaging or TCP/IP

## Project Setup

### SimConnect client

#### Step 1: Install Visual Studio
Download and Install Visual Studio (if not already installed).
Go to the Visual Studio website.
Choose Visual Studio 2019 or later.
During installation, make sure to select the .NET desktop development workload, which includes the necessary tools for C# development.

#### Step 2: Install the SimConnect SDK
In order to run this project, which is a SimConnect client, you will need the SimConnect SDK that comes with Flight Simulator X or Prepar3D. Here’s how you can get it:

For FSX:
FSX Steam Edition:

The SDK is typically located at C:\Program Files (x86)\Steam\steamapps\common\FSX\SDK\SimConnect SDK\.
[DIRECTORY OF .DLL FOUND IN: C:\Windows\assembly\GAC_32\Microsoft.FlightSimulator.SimConnect\10.0.60905.0__31bf3856ad364e35]
If it's not installed, you can manually install it from the SDK folder in your FSX directory.
FSX Boxed Edition:

The SDK is located at C:\Program Files (x86)\Microsoft Games\Microsoft Flight Simulator X SDK\SimConnect SDK\.
For Prepar3D:
The SDK is located at C:\Program Files (x86)\Lockheed Martin\Prepar3D vX\SDK\SimConnect SDK\ (replace vX with your version of Prepar3D, like v4 or v5).

#### Step 3: Add References to SimConnect.dll
To use SimConnect in C#, you need to reference SimConnect.dll. The .NET wrapper (Microsoft.FlightSimulator.SimConnect.dll) is already provided in the SDK. Follow these steps to reference it:

In Solution Explorer, right-click on your project and choose Add > Reference.
In the Reference Manager window, click Browse and navigate to the SimConnect SDK directory:
For FSX: C:\Program Files (x86)\Steam\steamapps\common\FSX\SDK\SimConnect SDK\lib\managed\Microsoft.FlightSimulator.SimConnect.dll
For Prepar3D: Navigate to the corresponding SimConnect SDK folder (e.g., lib\managed\Microsoft.FlightSimulator.SimConnect.dll).
Select Microsoft.FlightSimulator.SimConnect.dll and click OK to add it to your project.

### Arduino platform

An Arduino board must be connected to the USB port of your PC. If the client does not detect a connection it will raise an error on the terminal.

## Run project

> Note: Make sure an arduino board is connected to any USB port of your PC before Step 2

#### Step 1: Run FSX
Execute FSX.exe to run the main program, once initialized the API will start working.

#### Step 2: Run this project
By running this project, the client will check is FSX API is available and then if there is an open serial port to start sending data to Arduino.

> Note: This project has been designed to be used for an Aerospatiale AS350B model, thus all the inputs/outputs belong to this aircraft.


# References
- [Flight Simulator SDK Documentation](https://docs.flightsimulator.com/flighting/html/Programming_Tools/Programming_APIs.htm)
