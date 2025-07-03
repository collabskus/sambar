using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace sambar;

public partial class Api {

    public async Task LaunchUri(string uri) {

        await Launcher.LaunchUriAsync(new Uri(uri));
    }

    public void StartMenu()
    {
        nint hWnd = User32.FindWindow("Shell_TrayWnd", null);
        User32.SendMessage(hWnd, (uint)WINDOWMESSAGE.WM_SYSCOMMAND, (nint)SYSCOMMAND.SC_TASKLIST, 0);
    }

    public void ActionCenter()
    {
        string actionCenterUri = "ms-actioncenter:controlcenter/&showFooter=true";
        LaunchUri(actionCenterUri).Wait();
    }

    public void NotificationMenu()
    {
        string notificationsMenuUri = "ms-actioncenter:controlcenter/&showFooter=true";
        LaunchUri(notificationsMenuUri ).Wait();
    }
}

