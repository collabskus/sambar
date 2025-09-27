using System.IO;

return (dynamic startButton, dynamic ENV) =>
{
	startButton.btn.ImageSrc = Path.Join(ENV.ASSETS_FOLDER, "windows.ico");
};
