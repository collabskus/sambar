mkdir builds
cd Src

# light
dotnet publish -r win-x64
cp -r WidgetPacks\* bin\Release\net9.0-windows10.0.26100.0\win-x64\publish\WidgetPacks
Compress-Archive -Path bin\Release\net9.0-windows10.0.26100.0\win-x64\publish\* -DestinationPath ..\builds\Sambar-0.1.zip

# self-contained
# dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained -c Release
# Compress-Archive -Path bin\Release\net9.0-windows10.0.26100.0\win-x64\publish\* -DestinationPath ..\builds\Sambar-0.1-self-contained.zip

