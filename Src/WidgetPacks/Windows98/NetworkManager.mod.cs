using System.IO;

return (dynamic networkManager, dynamic ENV) =>
{
	networkManager.btn.ImageSrc = Path.Join(ENV.ASSETS_FOLDER, "wifi.ico");
};
