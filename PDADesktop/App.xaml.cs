﻿using log4net;
using PDADesktop.Classes;
using PDADesktop.Model;
using PDADesktop.View;
using Squirrel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows;

namespace PDADesktop
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private MainWindow mainWindowView;
        private UpdateManager updateManager;

        public MainWindow MainWindowView
        {
            get
            {
                return mainWindowView;
            }
            set
            {
                mainWindowView = value;
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            /*
             * 1- verificar actualizaciones con squirrel
             * 2- checkear conexion PDAExpress server
             * 3- checkear conexion PDAMoto
             * 4- verificar datos guardados
             * 5- iniciar ventana
             */

            CheckUpdates();
            string urlServerStatus = "http://localhost:8080/pdaexpress/pdadesktopdemo/serverConexionStatus.action";
            bool serverStatus = HttpWebClient.getHttpWebServerConexionStatus(urlServerStatus);
            logger.Debug("Conexion pdaexpress server " + serverStatus);

            //TODO manejar timeout, reconexion
            string urlSincronizacion = "http://localhost:8080/pdaexpress/pdadesktopdemo/getSincronizacionActual.action";
            string queryParam = "?idSucursal=706&idLote=71006";
            List<Sincronizacion> sincro = HttpWebClient.GetHttpWebSincronizacion(urlSincronizacion+queryParam);
            logger.Info(sincro);

            int respuestaDll = MotoApi.isDeviceConnected();
            bool boolValue = respuestaDll != 0;
            logger.Info("PDA is connected: " + boolValue);

            logger.Info("Verificando datos guardados...");
            string usuario = VerificarDatosGuardados();
            if(usuario != null)
            {
                logger.Info("cookie de usuario encontrada: " + usuario);
            }

            /* 3* verificar cookies guardadas y loguear ó redirigir al login*/

            Random rnd = new Random();
            // Genera un numero mayor a 0 y menor a 2 
            int numRandom = rnd.Next(0, 2);

            logger.Debug("Numero aleatorio generado: " + numRandom);
            if(numRandom == 1)
            {
                logger.Info("Hemos recordado al usuario, logueando...");

                MainWindowView = new MainWindow();
                Uri uri = new Uri("View/CentroActividades.xaml", UriKind.Relative);
                MainWindowView.frame.NavigationService.Navigate(uri);
                MainWindowView.Show();
            }
            else
            {
                logger.Info("No hay datos guardados con un usuario válido, llendo al login");
                MainWindowView = new MainWindow();
                MainWindowView.Show();
            }
        }

        async void CheckUpdates()
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
            DisposeUpdateManager(updateManager);
        }

        public static void DisposeUpdateManager(UpdateManager updateManager)
        {
            logger.Info("dispose update manager\n\n");
            updateManager.Dispose();
        }

        private string VerificarDatosGuardados()
        {
            return CookieManager.getCookie(CookieManager.Cookie.usuario);
        }

    }
}
