﻿using log4net;
using Navegacion.Classes;
using Navegacion.View;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Navegacion.ViewModel
{
    class LoginViewModel
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public string usernameText { get; set; }

        private ICommand loginButtonCommand;
        public ICommand LoginButtonCommand
        {
            get
            {
                return loginButtonCommand;
            }
            set
            {
                loginButtonCommand = value;
            }
        }
        private bool canExecute = true;

        #region Constructor
        public LoginViewModel()
        {
            LoginButtonCommand = new RelayCommand(LoginPortalApi, param => this.canExecute);
        }
        #endregion

        public void LoginPortalApi(object obj)
        {
            logger.Info("login portal api");

            MainWindow window = (MainWindow) Application.Current.MainWindow;
            if ("dariojz".Equals(usernameText))
            {
                logger.Debug("Nombre no null: " + usernameText);
                DateTime fechaExpiracion = new DateTime(2020, 12, 31);
                string userCookie = "username=" + usernameText + ";expires=" + fechaExpiracion.ToString("r");
                string urlFromProperties = ConfigurationManager.AppSettings.Get("URL_COOKIE");
                string variablePublic = Environment.ExpandEnvironmentVariables(urlFromProperties);
                Uri cookieUri1 = new Uri(variablePublic);
                Application.SetCookie(cookieUri1, userCookie);

                string cookie = Application.GetCookie(cookieUri1);
                logger.Info("Cookie recibida: " + cookie);
                //aca deberia llamar al servicio de login
                //Redirecciona a centroActividades
                Uri uri = new Uri("View/CentroActividades.xaml", UriKind.Relative);
                window.frame.NavigationService.Navigate(uri);
            }
            else
            {
                //marcar como usuario y/o contraseña incorrectos
                LoginView loginview = (LoginView) window.frame.Content;
                //loginview.usernameText.BorderBrush = Brushes.Red;
                //loginview.mjsError.Content = "usuario y/o contraseña incorrectos";
                //loginview.mjsError.Visibility = Visibility.Visible;
                logger.Error("usuario y/o contraseña incorrectos");
            }
        }
    }
}
