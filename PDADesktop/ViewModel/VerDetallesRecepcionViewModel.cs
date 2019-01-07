﻿using log4net;
using MahApps.Metro.Controls.Dialogs;
using PDADesktop.Classes;
using PDADesktop.Classes.Devices;
using PDADesktop.Classes.Utils;
using PDADesktop.Model;
using PDADesktop.Model.Dto;
using PDADesktop.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PDADesktop.ViewModel
{
    class VerDetallesRecepcionViewModel : ViewModelBase
    {
        #region Attributes
        #region Commons Attributes
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IDialogCoordinator dialogCoordinator;
        private IDeviceHandler deviceHandler { get; set; }

        private ObservableCollection<Recepcion> receptions;
        public ObservableCollection<Recepcion> Receptions
        {
            get
            {
                return receptions;
            }
            set
            {
                receptions = value;
                OnPropertyChanged();
            }
        }

        private Recepcion selectedReception;
        public Recepcion SelectedReception
        {
            get
            {
                return selectedReception;
            }
            set
            {
                selectedReception = value;
                if (selectedReception != null)
                {
                    ReceptionEnableEdit = true;
                }
                else
                {
                    ReceptionEnableEdit = false;
                }
                OnPropertyChanged();
            }
        }

        private bool receptionEnableEdit;
        public bool ReceptionEnableEdit
        {
            get
            {
                return receptionEnableEdit;
            }
            set
            {
                receptionEnableEdit = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Loading Panel Attributes
        private bool panelLoading;
        public bool PanelLoading
        {
            get
            {
                return panelLoading;
            }
            set
            {
                panelLoading = value;
                OnPropertyChanged();
            }
        }
        private string panelMainMessage;
        public string PanelMainMessage
        {
            get
            {
                return panelMainMessage;
            }
            set
            {
                panelMainMessage = value;
                OnPropertyChanged();
            }
        }
        private string panelSubMessage;
        public string PanelSubMessage
        {
            get
            {
                return panelSubMessage;
            }
            set
            {
                panelSubMessage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Worker Attributes
        private readonly BackgroundWorker discardReceptionsWorker = new BackgroundWorker();
        private readonly BackgroundWorker retryInformReceptionsWorker = new BackgroundWorker();
        #endregion

        #region Dispatcher Attributes
        private Dispatcher dispatcher;
        #endregion

        #region Commands Attributes
        private ICommand verDetallesRecepcionLoadedEvent;
        public ICommand VerDetallesRecepcionLoadedEvent
        {
            get
            {
                return verDetallesRecepcionLoadedEvent;
            }
            set
            {
                verDetallesRecepcionLoadedEvent = value;
            }
        }

        private ICommand discardAllCommand;
        public ICommand DiscardAllCommand
        {
            get
            {
                return discardAllCommand;
            }
            set
            {
                discardAllCommand = value;
            }
        }
        private ICommand cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                return cancelCommand;
            }
            set
            {
                cancelCommand = value;
            }
        }
        private ICommand retryCommand;
        public ICommand RetryCommand
        {
            get
            {
                return retryCommand;
            }
            set
            {
                retryCommand = value;
            }
        }
        private ICommand panelCloseCommand;
        public ICommand PanelCloseCommand
        {
            get
            {
                return panelCloseCommand;
            }
            set
            {
                panelCloseCommand = value;
            }
        }
        #endregion
        #endregion

        #region Constructor
        public VerDetallesRecepcionViewModel(IDialogCoordinator instance)
        {
            BannerApp.PrintSeeDetailsReception();
            dialogCoordinator = instance;
            deviceHandler = App.Instance.deviceHandler;
            dispatcher = App.Instance.Dispatcher;

            DisplayWaitingPanel("Cargando...");
            ReceptionEnableEdit = false;

            VerDetallesRecepcionLoadedEvent = new RelayCommand(VerDetallesRecepcionLoadedEventAction);

            discardReceptionsWorker.DoWork += discardReceptionsWorker_DoWork;
            discardReceptionsWorker.RunWorkerCompleted += discardReceptionsWorker_RunWorkerCompleted;
            retryInformReceptionsWorker.DoWork += retryInformReceptionsWorker_DoWork;
            retryInformReceptionsWorker.RunWorkerCompleted += retryInformReceptionsWorker_RunWorkerCompleted;

            DiscardAllCommand = new RelayCommand(DiscardAllAction);
            CancelCommand = new RelayCommand(CancelAction);
            RetryCommand = new RelayCommand(RetryAction);
            PanelCloseCommand = new RelayCommand(PanelCloseAction);
        }
        #endregion

        #region Event Method
        public void VerDetallesRecepcionLoadedEventAction(object sender)
        {
            logger.Debug("Ver detalles recepcion => Loaded Event");
            string batchId = MyAppProperties.currentBatchId;
            ListView listView = HttpWebClientUtil.LoadReceptionsGrid(Convert.ToInt64(batchId));
            Receptions = ListViewUtils.ParserRecepcionDataGrid(listView);
            dispatcher.BeginInvoke(new Action(() => HidingWaitingPanel()));
        }
        #endregion

        #region Workers
        #region Discard Reception Worker
        private void discardReceptionsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            logger.Debug("discard receptions => Do Work");
            long syncId = MyAppProperties.SeeDetailsReception_syncId;
            long batchId = MyAppProperties.SeeDetailsReception_batchId;
            Dictionary<string, string> responseDiscard = HttpWebClientUtil.DiscardReceptions(batchId.ToString(), syncId.ToString());
            if(responseDiscard.ContainsKey("descargarPedidos"))
            {
                var descargarPedidos = responseDiscard["descargarPedidos"];
                if (descargarPedidos.Equals("true"))
                {
                    bool thereAreInformed = responseDiscard["hayInformadas"].Equals("true");
                    var syncIdResponse = responseDiscard["idSincronizacion"] as string;
                    UpdateOrder(syncIdResponse, thereAreInformed);
                }
            }

            string storeId = MyAppProperties.storeId;
            ActionResultDto unlockDevice = deviceHandler.ControlDeviceLock(syncId, storeId);
            if (unlockDevice.success)
            {
                deviceHandler.ChangeSynchronizationState(Constants.ESTADO_SINCRO_FIN);
            }
        }

        private void UpdateOrder(string syncId, bool thereAreInformed)
        {
            try
            {
                string storeId = MyAppProperties.storeId;
                string userName = MyAppProperties.username;
                bool downloadOrders = HttpWebClientUtil.SearchDATsMasterFile(Convert.ToInt32(Constants.PEDIDOS_CODE), storeId, userName);
                if (downloadOrders)
                {
                    ArchivosDATUtils.createOrdersFiles();
                }

                if (thereAreInformed)
                {
                    SynchronizationStateUtil.SetPrintReceptions(Convert.ToInt64(syncId));
                }
                else
                {
                    SynchronizationStateUtil.SetSentGenesixState(Convert.ToInt64(syncId));
                }
            }
            catch
            {
                SynchronizationStateUtil.SetRetry3GeneralState(Convert.ToInt64(syncId));
            }
        }

        private void discardReceptionsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            logger.Debug("discard receptions => Run Worker Completed");
            dispatcher.BeginInvoke(new Action(() =>
            {
                HidingWaitingPanel();
                RedirectToActivityCenterView();
            }));
        }
        #endregion

        #region Retry Inform Receptions Worker
        private void retryInformReceptionsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            logger.Debug("retry inform receptions => Do Work");
            long syncId = MyAppProperties.SeeDetailsReception_syncId;
            int activityId = Convert.ToInt32(Constants.RECEP_CODE);
            ButtonStateUtils.RetryInformToGenesix(syncId, activityId);
        }
        private void retryInformReceptionsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            logger.Debug("retry inform receptions => Run Worker Completed");
            dispatcher.BeginInvoke(new Action(() =>
            {
                HidingWaitingPanel();
                RedirectToActivityCenterView();
            }));
        }
        #endregion
        #endregion

        #region Action Methods
        private async void DiscardAllAction(object sender)
        {
            string messageToAskToUser = "Se descartarán todas las Recepciones pendientes de informar. ¿Desea continuar?";
            bool userAnswer = await AskUserMetroDialog(messageToAskToUser);

            if (userAnswer)
            {
                DisplayWaitingPanel("Espere por favor", "Descartando recepciones");
                discardReceptionsWorker.RunWorkerAsync();
            }
        }
        private void CancelAction(object sender)
        {
            DisplayWaitingPanel("Espere por favor");
            RedirectToActivityCenterView();
        }
        private void RetryAction(object sender)
        {
            DisplayWaitingPanel("Espere por favor", "Reintentando informar recepciones");
            retryInformReceptionsWorker.RunWorkerAsync();
        }

        private void PanelCloseAction(object sender)
        {
            HidingWaitingPanel();
        }
        #endregion

        #region Commons Methods
        private void RedirectToActivityCenterView()
        {
            MainWindow window = (MainWindow)Application.Current.MainWindow;
            Uri uri = new Uri(Constants.CENTRO_ACTIVIDADES_VIEW, UriKind.Relative);
            window.frame.NavigationService.Navigate(uri);
        }
        #endregion

        #region Metro Dialog Methods
        private async Task<bool> AskUserMetroDialog(string message, string title = "Aviso")
        {
            MessageDialogStyle messageDialogStyle = MessageDialogStyle.AffirmativeAndNegative;
            bool userResponse = await ShowMetroDialog(messageDialogStyle, message, title);
            return userResponse;
        }

        private async Task<bool> ShowMetroDialog(MessageDialogStyle messageDialogStyle, string message, string title = "Aviso")
        {
            MetroDialogSettings metroDialogSettings = new MetroDialogSettings();
            metroDialogSettings.AffirmativeButtonText = "Aceptar";
            metroDialogSettings.NegativeButtonText = "Cancelar";
            MessageDialogResult userResponse = await dialogCoordinator.ShowMessageAsync(this, title, message, messageDialogStyle, metroDialogSettings);
            return userResponse == MessageDialogResult.Affirmative;
        }
        #endregion

        #region Panel Methods
        public void DisplayWaitingPanel(string mainMessage, string subMessage = "")
        {
            PanelLoading = true;
            PanelMainMessage = mainMessage;
            PanelSubMessage = subMessage;
        }
        public void HidingWaitingPanel()
        {
            PanelLoading = false;
            PanelMainMessage = "";
            PanelSubMessage = "";
        }

        #endregion
    }
}
