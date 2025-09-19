cd Src
dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained -c Release
cp Src\WidgetPacks Src\bin\Release\net*\win-x64\publish
Compress-Archive -Path Src\bin\Release\net*\win-x64\publish -DestinationPath Sambar-0.1.zip
