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
    public partial class App : Application
    {
        #region attributes
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private UpdateManager updateManager { get; set; }
        public static App Instance { get; private set; }
        public Container Container { get; private set; }
        private static Mutex _mutex = null;
        public IDeviceHandler deviceHandler { get; private set; }
        #endregion

        #region constructor
        public App()
        {
            BannerApp.printBanner();
            Instance = this;
            Container = new Container(c =>
            {
                c.AddRegistry(new MyContainerInitializer());
            });
            deviceHandler = this.Container.GetInstance<IDeviceHandler>();
            logger.Info("Adaptador: " + deviceHandler.GetName());
        }
        #endregion

        #region startup
        protected override void OnStartup(StartupEventArgs e)
        {
            logger.Debug("Leyendo Default.dat");
            string x = deviceHandler.ReadDefaultDataFile();
            logger.Debug(x);
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

            logger.Debug("Checkeando conexión el servidor PDAExpress server");
            CheckServerStatus();

            logger.Debug("Checkear conexión con dispositivo");
            if (CheckDeviceConnected())
            {
                logger.Debug("Verificando version dispositivo");
                UpdateDeviceApp();
            }

            logger.Debug("Verificando datos guardados...");
            string isUserReminded = VerificarDatosGuardados();

            if (isUserReminded != null)
            {
                bool check = isUserReminded == "true";
                if(check)
                {
                    // user reminded

                    // getUserCredentials();
                    // AttemptAutoLoginPortalImagoSur();
                    if (GenerandoAleatoriedadDeCasosLogueados() == 1)
                    {
                        RedireccionarCentroActividades();
                    }
                    else
                    {
                        // fail to autoLogin
                        RedireccionarLogin();
                    }
                }
                else
                {
                    // no reminded user
                    RedireccionarLogin();
                }
           }
           else
           {
               RedireccionarLogin();
            }
        }
        #endregion

        #region unhandledException
        void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Error(e.ToString());
        }
        #endregion

        #region methods
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
            bool dispositivoConectado = deviceHandler.IsDeviceConnected();
            logger.Info("PDA is connected: " + dispositivoConectado);
            return dispositivoConectado;
        }

        private void UpdateDeviceApp()
        {
            logger.Info("UpdatePDAMotoApp: ");
            //1- obtener el archivo DEFAULT.DAT
            //2- si no existe crearlo
            //3- pegarle al endpoint getInfoVersion, comparar y evaluar
        }

        private string VerificarDatosGuardados()
        {
            //return CookieManager.getCookie(CookieManager.Cookie.recuerdame);
            return null;
        }

        private void RedireccionarCentroActividades()
        {
            logger.Info("Redireccionando al centro de actividades...");
            var mainWindow = this.Container.GetInstance<MainWindow>();
            Uri uri = new Uri(Constants.CENTRO_ACTIVIDADES_VIEW, UriKind.Relative);
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
