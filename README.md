# DictCombine

This application uses a partial sorting algorithm, splitting files into pieces that can fit in RAM, then collects a common file from these pieces, removing duplicates.  

You may encounter an error creating files in a temporary folder, although there is still space on the disk. This is because many Linux distributions mount the /tmp directory in RAM to improve its performance. In this case, there is an optional parameter that defines the path to the place where the folder with temporary files will be created.

```
Arguments: [path to directory with dictionaries] [path to output file] [buffer size optional] [path to temp directory optional]

Buffer size is the number of records stored in RAM from EACH file.
The more - the faster the program, but more RAM consumption. Default is 50000000.
The path to the directory with temporary files should be specified if the /tmp directory is mounted in RAM. The files necessary
for calculations will be created in this directory and will be automatically deleted after the program ends.
```

❗❗ **Use only full paths to files and directorys**

## Building for Linux
```
dotnet publish -p:PublishSingleFile=true -c Release --self-contained=false -r linux-x64
```

## Building for Windows
```
dotnet publish -p:PublishSingleFile=true -c Release --self-contained=false -r win-x64
```

## Dependencies
```
dotnet sdk >= 7.0.0
dotnet runtume >= 7.0.0
```

## Install dependencies for Arch/Manjaro

```
~# pacman -Sy dotnet-runtime dotnet-sdk
```
