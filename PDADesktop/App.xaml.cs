﻿using log4net;
using PDADesktop.Classes;
using PDADesktop.Classes.Devices;
using PDADesktop.View;
using Squirrel;
using StructureMap;
using System;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace PDADesktop
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private UpdateManager updateManager;

        public static App Instance { get; private set; }
        public Container Container { get; private set; }
        private static Mutex _mutex = null;
        public IDeviceHandler deviceHandler { get; private set; }
        public App()
        {
            BannerApp.printBanner();
            Instance = this;
            Container = new Container(c =>
            {
                c.AddRegistry(new MyContainerInitializer());
            });
            deviceHandler = this.Container.GetInstance<IDeviceHandler>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            CheckApplicationRunning();
            base.OnStartup(e);
            string sucursalHarcodeada = "706";
            MyAppProperties.idSucursal = sucursalHarcodeada;

            /*
             * 0- checkear que la aplicacion no este ejecutandose
             * 1- verificar en segundo plano actualizaciones con squirrel
             * 2- checkear conexion PDAExpress server
             * 3- checkear conexion PDAMoto
             * 4- verificar datos guardados
             * 5- iniciar ventana
             * 6- Version PDAMoto
            */

            logger.Debug("Verificando en segundo plano actualizaciones con squirrel.window");
            UpdateApp();
            //AlertBarWpf para una segunda instancia

            logger.Debug("Checkeando conexión el servidor PDAExpress server");
            CheckServerStatus();

            logger.Debug("Checkear conexión con PDAMoto");
            if (CheckDeviceConnected())
            {
                UpdatePDAMotoApp();
            }

            logger.Debug("Verificando datos guardados...");
            string isUserReminded = VerificarDatosGuardados();

            if (isUserReminded != null)
            {
                bool check = isUserReminded == "true";
                if(check)
                {
                    // user reminded
                    // =======================================
                    // obtain username and pass
                    // getUserCredentials();

                    // Attempt to login through imagosur-portal
                    // AttemptLoginPortalImagoSur();


                    if (GenerandoAleatoriedadDeCasosLogueados() == 1)
                    {
                        RedireccionarCentroActividades();
                    }
                    else
                    {
                        RedireccionarLogin();
                    }
                }
                else
                {
                    // no user reminded
                    RedireccionarLogin();
                }
           }
           else
           {
               RedireccionarLogin();
            }
        }

        void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Error(e.ToString());
        }

        private void CheckApplicationRunning()
        {
            const string appName = "PDADesktop";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application
                Application.Current.Shutdown();
            }
        }

        #region Methods
        private int GenerandoAleatoriedadDeCasosLogueados()
        {
            Random rnd = new Random();
            int numRandom = rnd.Next(0, 2);
            logger.Debug("Numero aleatorio generado: " + numRandom);
            return numRandom;
        }

        async void UpdateApp()
        {
            string hostIpPort = ConfigurationManager.AppSettings.Get("SERVER_HOST_PROTOCOL_IP_PORT");
            string urlOrPath = hostIpPort + ConfigurationManager.AppSettings.Get("URL_UPDATE");
            try
            {
                using (updateManager = new UpdateManager(urlOrPath))
                {
                    logger.Info("buscando actualización");
                    await updateManager.UpdateApp();
                    logger.Info("actualización finalizada");
                }
            }
            catch (Exception e)
            {
                logger.Error("Error al buscar actualizaciones a " + urlOrPath);
                logger.Error(e.GetBaseException().ToString());
            }
        }
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            logger.Info("dispose update manager");
            updateManager.Dispose();
        }

        private void CheckServerStatus()
        {
            bool serverStatus = HttpWebClient.getHttpWebServerConexionStatus();
            logger.Info("Conexión pdaexpress server " + serverStatus);
        }

        private bool CheckDeviceConnected()
        {
            bool dispositivoConectado = deviceHandler.isDeviceConnected();
            logger.Info("PDA is connected: " + dispositivoConectado);
            return dispositivoConectado;
        }

        private void UpdatePDAMotoApp()
        {
            logger.Info("UpdatePDAMotoApp: ");
        }

        private string VerificarDatosGuardados()
        {
            return CookieManager.getCookie(CookieManager.Cookie.recuerdame);
        }

        private void RedireccionarCentroActividades()
        {
            logger.Info("Redireccionando al centro de actividades...");
            var mainWindow = this.Container.GetInstance<MainWindow>();
            Uri uri = new Uri("View/CentroActividadesView.xaml", UriKind.Relative);
            mainWindow.frame.NavigationService.Navigate(uri);
            mainWindow.Show();
        }

        private void RedireccionarLogin()
        {
            logger.Info("No hay datos guardados con un usuario válido, redireccionando al login");
            var mainWindow = this.Container.GetInstance<MainWindow>();
            mainWindow.Show();
        }
        #endregion 
    }
}
