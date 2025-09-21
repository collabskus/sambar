mkdir builds
mkdir zips
rm -Recurse -Force -Confirm:$false builds\*
cd Src

# light
dotnet publish -r win-x64
rm -Recurse -Force -Confirm:$false bin\Release\net9.0-windows10.0.26100.0\win-x64\publish\WidgetPacks
cp -r WidgetPacks bin\Release\net9.0-windows10.0.26100.0\win-x64\publish\WidgetPacks
cp -r bin\Release\net9.0-windows10.0.26100.0\win-x64\publish\* ..\builds
Compress-Archive -Path ..\builds\* -DestinationPath ..\zips\sambar.zip

# self-contained
# dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained -c Release
# Compress-Archive -Path bin\Release\net9.0-windows10.0.26100.0\win-x64\publish\* -DestinationPath ..\builds\Sambar-0.1-self-contained.zip

cd ..
