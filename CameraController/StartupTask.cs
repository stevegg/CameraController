using Windows.ApplicationModel.Background;
using Devkoes.Restup.WebServer.Http;
using Devkoes.Restup.WebServer.Rest;
using Devkoes.Restup.WebServer.File;
using CameraController.Controllers;
using Windows.UI.Xaml;
using System;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace CameraController
{
    public sealed class StartupTask : IBackgroundTask
    {
        private HttpServer _httpServer;

        private BackgroundTaskDeferral _deferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            //timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromMilliseconds(60000);

            var httpServer = new HttpServer(8800);
            _httpServer = httpServer;

            var restRouteHandler = new RestRouteHandler();
            restRouteHandler.RegisterController<CameraServiceController>();

            httpServer.RegisterRoute("api", restRouteHandler);
            httpServer.RegisterRoute(new StaticFileRouteHandler(@"Web"));

            await httpServer.StartServerAsync();
        }

        private void Timer_Tick(object sender, object e)
        {
            UpdateCameras();
        }

        private void UpdateCameras()
        {
            System.Diagnostics.Debug.WriteLine("Updating cameras...");


            System.Diagnostics.Debug.WriteLine("Done.");
        }
    }
}

