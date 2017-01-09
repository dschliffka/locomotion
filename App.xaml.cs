using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Locomotion
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public NetworkManager networkManager = NetworkManager.InstanceCreator();
        public ProfileManager profileManager = ProfileManager.InstanceCreator();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            profileManager.parseProfiles();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            profileManager.writeToFile();
        }
    }
}
