using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Satoshi_City_launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {

            WebClient webClient = new WebClient();
            webClient.DownloadFile(new Uri("https://onedrive.live.com/download?cid=E37477BC79C7D950&resid=E37477BC79C7D950%21161894&authkey=AL_AS-LFq1R0Ggk"), "DataFile.db");
            DatabaseFacade facade = new DatabaseFacade(new UserDataContext());
            facade.EnsureCreated();
        }
    }
}
