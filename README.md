# RS485/232 Demo Driver - Crestron Home

Simple implementation of the RS485/232 TX/RX Driver in Crestron Home

## Demo

### Tile (Arrived)

When the driver received "1"/0x31 (receiving data must be end with the newline character "\n"/0x0A)

![arrived](https://raw.githubusercontent.com/jonathanchan1234/CrestronHome-RS485/master/docs/arrived.jpeg)

### Tile (Not Arrived)

When the driver received any data other than "1"/0x31 (receiving data must be end with the newline character "\n"/0x0A)

![not_arrived](https://raw.githubusercontent.com/jonathanchan1234/CrestronHome-RS485/master/docs/not_arrived.jpeg)

### Main Layout

The "Data Received" text display will display the data received in ascii (receiving data must be end with the newline character "\n"/0x0A)
Pressing the "Send Command" button would send the string in the "Command To Be Sent" text entry.

![main_layout](https://raw.githubusercontent.com/jonathanchan1234/CrestronHome-RS485/master/docs/main_layout.jpeg)

## Build The Project

***For each visual studio project, remember to import the NuGet Crestron Library and the required SDK following the instruction [here](<https://sdkcon78221.crestron.com/sdk/Crestron_Certified_Drivers_SDK/Content/Topics/Create-a-Project.htm>).***

1. Open Visual Studio 2019 and open the RS485CrestronHome (*RS485CrestronHome.sln*) project and build the project solution.
2. Open the RS485Gateway (*RS485Gateway.sln*) project.
3. Add the reference to the *RS485CretronHome.dll*.
4. Build the RS485Gateway project solution. Run the ManifestUtil.exe to generate two pkg files (*RS485Gateway.pkg*, *RS485CrestronHome.pkg*)
5. Move the *RS485Gateway.pkg* to the ThridPartyDriver/Import Directory of your Crestron Home Controller using Crestron Toolbox.

** For RS232, please change the "protocol" field to 0 in RS485Gateway.json

## Project Structure

    ├──RS485Gateway                         # Implementation of the Gateway Driver
        ├── RS485Gateway
            ├── ARS485Gateway.cs            # Entry point of the driver
            ├── RS485Gateway.json           # Driver Definition File
            ├── CallLiftSerialProtocol.cs   # Business Logic in processing serial data
            ├── bin/Debug/IncludeInPkg      # UIDefinition and translation files
    ├──RS485CrestronHome
        ├── RS485CrestronHome
            ├── RS485CrestronHome.cs       # Entry point of the Home Extension Driver 
