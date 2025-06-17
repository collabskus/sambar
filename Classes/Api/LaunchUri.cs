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

}
