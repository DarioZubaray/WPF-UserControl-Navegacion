﻿using log4net;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using PDADesktop.Classes;
using PDADesktop.Classes.Devices;
using PDADesktop.Model;
using PDADesktop.Classes.Utils;
using PDADesktop.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PDADesktop.Model.Dto;

namespace PDADesktop.ViewModel
{
    class CentroActividadesViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Commands
        private ICommand exitButtonCommand;
        public ICommand ExitButtonCommand
        {
            get
            {
                return exitButtonCommand;
            }
            set
            {
                exitButtonCommand = value;
            }
        }

        private ICommand sincronizarCommand;
        public ICommand SincronizarCommand
        {
            get
            {
                return sincronizarCommand;
            }
            set
            {
                sincronizarCommand = value;
            }
        }

        private ICommand informarCommand;
        public ICommand InformarCommand
        {
            get
            {
                return informarCommand;
            }
            set
            {
                informarCommand = value;
            }
        }

        private ICommand verAjustesCommand;
        public ICommand VerAjustesCommand
        {
            get
            {
                return verAjustesCommand;
            }
            set
            {
                verAjustesCommand = value;
            }
        }

        private ICommand centroActividadesLoadedCommand;
        public ICommand CentroActividadesLoadedCommand
        {
            get
            {
                return centroActividadesLoadedCommand;
            }
            set
            {
                centroActividadesLoadedCommand = value;
            }
        }

        private ICommand anteriorCommand;
        public ICommand AnteriorCommand
        {
            get
            {
                return anteriorCommand;
            }
            set
            {
                anteriorCommand = value;
            }
        }

        private ICommand siguienteCommand;
        public ICommand SiguienteCommand
        {
            get
            {
                return siguienteCommand;
            }
            set
            {
                siguienteCommand = value;
            }
        }

        private ICommand buscarCommand;
        public ICommand BuscarCommand
        {
            get
            {
                return buscarCommand;
            }
            set
            {
                buscarCommand = value;
            }
        }

        private ICommand ultimaCommand;
        public ICommand UltimaCommand
        {
            get
            {
                return ultimaCommand;
            }
            set
            {
                ultimaCommand = value;
            }
        }

        private ICommand estadoGenesixCommand;
        public ICommand EstadoGenesixCommand
        {
            get
            {
                return estadoGenesixCommand;
            }
            set
            {
                estadoGenesixCommand = value;
            }
        }

        private ICommand estadoPDACommand;
        public ICommand EstadoPDACommand
        {
            get
            {
                return estadoPDACommand;
            }
            set
            {
                estadoPDACommand = value;
            }
        }

        private ICommand estadoGeneralCommand;
        public ICommand EstadoGeneralCommand
        {
            get
            {
                return estadoGeneralCommand;
            }
            set
            {
                estadoGeneralCommand = value;
            }
        }

        private ICommand showPanelCommand;
        public ICommand ShowPanelCommand
        {
            get
            {
                return showPanelCommand;
            }
            set
            {
                showPanelCommand = value;
            }
        }

        private ICommand hidePanelCommand;
        public ICommand HidePanelCommand
        {
            get
            {
                return hidePanelCommand;
            }
            set
            {
                hidePanelCommand = value;
            }
        }

        private ICommand changeMainMessageCommand;
        public ICommand ChangeMainMessageCommand
        {
            get
            {
                return changeMainMessageCommand;
            }
            set
            {
                changeMainMessageCommand = value;
            }
        }

        private ICommand changeSubMessageCommand;
        public ICommand ChangeSubMessageCommand
        {
            get
            {
                return changeSubMessageCommand;
            }
            set
            {
                changeSubMessageCommand = value;
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

        private bool canExecute = true;
        #endregion

        #region Attributes
        private readonly BackgroundWorker loadCentroActividadesWorker = new BackgroundWorker();
        private readonly BackgroundWorker sincronizarWorker = new BackgroundWorker();
        private readonly BackgroundWorker informarWorker = new BackgroundWorker();

        private string _txt_sincronizacion;
        public string txt_sincronizacion
        {
            get
            {
                return _txt_sincronizacion;
            }
            set
            {
                _txt_sincronizacion = value;
                OnPropertyChanged();
            }
        }

        private string _label_version;
        public string label_version
        {
            get
            {
                return _label_version;
            }
            set
            {
                _label_version = value;
            }
        }
        private string _label_usuario;
        public string label_usuario
        {
            get
            {
                return _label_usuario;
            }
            set
            {
                _label_usuario = value;
            }
        }
        private string _label_sucursal;
        public string label_sucursal
        {
            get
            {
                return _label_sucursal;
            }
            set
            {
                _label_sucursal = value;
            }
        }

        // Atributos del Spiner
        private bool _panelLoading;
        public bool PanelLoading
        {
            get
            {
                return _panelLoading;
            }
            set
            {
                _panelLoading = value;
                OnPropertyChanged();
            }
        }
        private string _panelMainMessage;
        public string PanelMainMessage
        {
            get
            {
                return _panelMainMessage;
            }
            set
            {
                _panelMainMessage = value;
                OnPropertyChanged();
            }
        }
        private string _panelSubMessage;
        public string PanelSubMessage
        {
            get
            {
                return _panelSubMessage;
            }
            set
            {
                _panelSubMessage = value;
                OnPropertyChanged();
            }
        }

        //Con ObservableCollection no se actualiza la grilla :\
        private List<SincronizacionDtoDataGrid> _sincronizaciones;
        public List<SincronizacionDtoDataGrid> sincronizaciones
        {
            get
            {
                return _sincronizaciones;
            }
            set
            {
                _sincronizaciones = value;
                OnPropertyChanged();
            }
        }
        private SincronizacionDtoDataGrid selectedSynchronization;
        public SincronizacionDtoDataGrid SelectedSynchronization
        {
            get
            {
                return selectedSynchronization;
            }
            set
            {
                selectedSynchronization = value;
                MyAppProperties.SelectedSynchronization = selectedSynchronization;
                OnPropertyChanged();
            }
        }

        private Badged badge_verAjustes;
        public Badged Badge_verAjustes
        {
            get
            {
                return badge_verAjustes;
            }
            set
            {
                badge_verAjustes = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public CentroActividadesViewModel()
        {
            PanelLoading = true;
            PanelMainMessage = "Cargando...";
            setInfoLabels();
            ExitButtonCommand = new RelayCommand(ExitPortalApi, param => this.canExecute);
            SincronizarCommand = new RelayCommand(SincronizarTodosLosDatos, param => this.canExecute);
            InformarCommand = new RelayCommand(InformarGenesix, param => this.canExecute);
            VerAjustesCommand = new RelayCommand(VerAjustes, param => this.canExecute);
            CentroActividadesLoadedCommand = new RelayCommand(CentroActividadesLoaded, param => this.canExecute);
            AnteriorCommand = new RelayCommand(SincronizacionAnterior, param => this.canExecute);
            SiguienteCommand = new RelayCommand(SincronizacionSiguiente, param => this.canExecute);
            BuscarCommand = new RelayCommand(BuscarSincronizaciones, param => this.canExecute);
            UltimaCommand = new RelayCommand(IrUltimaSincronizacion, param => this.canExecute);
            EstadoGenesixCommand = new RelayCommand(BotonEstadoGenesix, param => this.canExecute);
            EstadoPDACommand = new RelayCommand(BotonEstadoPDA, param => this.canExecute);
            EstadoGeneralCommand = new RelayCommand(BotonEstadoGeneral, param => this.canExecute);

            loadCentroActividadesWorker.DoWork += loadCentroActividadesWorker_DoWork;
            loadCentroActividadesWorker.RunWorkerCompleted += loadCentroActividadesWorker_RunWorkerCompleted;
            
            sincronizarWorker.DoWork += sincronizarWorker_DoWork;
            sincronizarWorker.RunWorkerCompleted += sincronizarWorker_RunWorkerCompleted;

            informarWorker.DoWork += informarWorker_DoWork;
            informarWorker.RunWorkerCompleted += informarWorker_RunWorkerCompleted;

            ShowPanelCommand = new RelayCommand(MostrarPanel, param => this.canExecute);
            HidePanelCommand = new RelayCommand(OcultarPanel, param => this.canExecute);
            ChangeMainMessageCommand = new RelayCommand(CambiarMainMensage, param => this.canExecute);
            ChangeSubMessageCommand = new RelayCommand(CambiarSubMensage, param => this.canExecute);
            PanelCloseCommand = new RelayCommand(CerrarPanel, param => this.canExecute);
        }
        #endregion

        #region Worker Method
        private void loadCentroActividadesWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string currentMessage = "loadCentroActividades Worker ->doWork";
            logger.Debug(currentMessage);

            currentMessage = "Verificando conexión con el servidor PDAExpress...";
            NotifyCurrentMessage(currentMessage);
            bool serverStatus = CheckServerStatus();

            currentMessage = "Obteniendo actividades de todas las acciones...";
            NotifyCurrentMessage(currentMessage);
            GetActividadesByAllAccionesAsync();

            string _sucursal = MyAppProperties.idSucursal;

            currentMessage = "Obteniendo el id del último lote para sucursal " + _sucursal + " ...";
            NotifyCurrentMessage(currentMessage);
            int idLoteActual = HttpWebClientUtil.GetIdLoteActual(_sucursal);

            currentMessage = "Obteniendo detalles de sincronización para lote " + idLoteActual + " ...";
            NotifyCurrentMessage(currentMessage);
            GetUltimaSincronizacion(idLoteActual, _sucursal);

            currentMessage = "Checkeando conexión con el dispositivo...";
            NotifyCurrentMessage(currentMessage);
            bool isConneted = CheckDeviceConnected();

            if(isConneted)
            {
                currentMessage = "Leyendo Ajustes realizados...";
                NotifyCurrentMessage(currentMessage);
                CreateBadgeVerAjustes();

                currentMessage = "Leyendo configuración del dispositivo...";
                NotifyCurrentMessage(currentMessage);
                CopyDeviceMainDataFileToPublic();

                currentMessage = "Controlando componentes del dispositivo...";
                NotifyCurrentMessage(currentMessage);
                ControlDevicePrograms();

                currentMessage = "Verificando sucursal configurada en el dispositivo...";
                NotifyCurrentMessage(currentMessage);
                bool needAssociateBranchOffice = CheckBranchOfficeDevice();
                if (needAssociateBranchOffice)
                {
                    currentMessage = "Asociando sucursal del dispositivo al n°" + _sucursal + " ...";
                    NotifyCurrentMessage(currentMessage);
                    AssociateCurrentBranchOffice(_sucursal);
                }

                currentMessage = "Borrando datos antiguos ...";
                NotifyCurrentMessage(currentMessage);
                CheckLastSynchronizationDateFromDefault();

                currentMessage = "Actualizando archivo principal de configuración del dispositivo ...";
                NotifyCurrentMessage(currentMessage);
                UpdateDeviceMainFile(_sucursal);
            }
            else
            {
                //Dispositivo No conectado
            }

        }

        public void NotifyCurrentMessage(string currentMessage)
        {
            logger.Debug(currentMessage);
            PanelSubMessage = currentMessage;
            Thread.Sleep(300);
        }

        private bool CheckServerStatus()
        {
            return HttpWebClientUtil.GetHttpWebServerConexionStatus();
        }

        public void GetActividadesByAllAccionesAsync()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;

                GetActividadesByAllAcciones();
            }).Start();
        }

        public List<Actividad> GetActividadesByAllAcciones()
        {
            //TODO llevar al Utils la logica de conexion
            string urlAcciones = ConfigurationManager.AppSettings.Get(Constants.API_GET_ALL_ACCIONES);
            var responseAcciones = HttpWebClientUtil.SendHttpGetRequest(urlAcciones);
            if (responseAcciones != null)
            {
                List<Accion> acciones = JsonConvert.DeserializeObject<List<Accion>>(responseAcciones);
                MyAppProperties.accionesDisponibles = acciones;
                string idAcciones = TextUtils.ParseListAccion2String(acciones);
                string jsonBody = "{ \"idAcciones\": " + idAcciones.ToString() + "}";

                var urlActividades = ConfigurationManager.AppSettings.Get(Constants.API_GET_ACTIVIDADES);
                string responseActividades = HttpWebClientUtil.SendHttpPostRequest(urlActividades, jsonBody);
                logger.Debug(responseActividades);
                List<Actividad> actividades = JsonConvert.DeserializeObject<List<Actividad>>(responseActividades);
                MyAppProperties.actividadesDisponibles = actividades;
                return actividades;
            }
            else
            {
                return new List<Actividad>();
            }
        }

        public void GetUltimaSincronizacion(int idLoteActual, string _sucursal)
        {
            if (idLoteActual != 0)
            {
                MyAppProperties.idLoteActual = idLoteActual.ToString();
                List<Sincronizacion> listaSincronizaciones = null;
                string urlPath = ConfigurationManager.AppSettings.Get(Constants.API_SYNC_ULTIMA);
                listaSincronizaciones = HttpWebClientUtil.GetHttpWebSincronizacion(urlPath, _sucursal, idLoteActual.ToString());
                if (listaSincronizaciones != null && listaSincronizaciones.Count != 0)
                {
                    this.sincronizaciones = SincronizacionDtoDataGrid.refreshDataGrid(listaSincronizaciones);
                    ActualizarLoteActual(sincronizaciones);
                }
            }
        }

        private bool CheckDeviceConnected()
        {
            bool dispositivoConectado = App.Instance.deviceHandler.IsDeviceConnected();
            logger.Info("Device is connected: " + dispositivoConectado);
            return dispositivoConectado;
        }

        public void CreateBadgeVerAjustes()
        {
            var dispatcher = Application.Current.Dispatcher;
            dispatcher.BeginInvoke(new Action(() =>
            {
                Button botonVerAjustes = new Button();
                botonVerAjustes.Name = "button_verAjustes";
                botonVerAjustes.Content = "Ver Ajustes";
                botonVerAjustes.HorizontalAlignment = HorizontalAlignment.Left;
                botonVerAjustes.VerticalAlignment = VerticalAlignment.Top;
                botonVerAjustes.ToolTip = "Ver los ajustes realizados.";
                botonVerAjustes.Command = this.VerAjustesCommand;

                Badged badge = new Badged();
                bool estadoDevice = App.Instance.deviceHandler.IsDeviceConnected();
                if (estadoDevice)
                {
                    string DeviceAjusteFile = App.Instance.deviceHandler.ReadAdjustmentsDataFile();
                    if (DeviceAjusteFile != null)
                    {
                        ObservableCollection<Ajustes> ajustes = JsonConvert.DeserializeObject<ObservableCollection<Ajustes>>(DeviceAjusteFile);
                        if (ajustes != null && ajustes.Count > 0)
                        {
                            badge.Badge = ajustes.Count;
                        }
                        else
                        {
                            badge.Badge = 0;
                            botonVerAjustes.IsEnabled = false;
                        }
                    }
                    else
                    {
                        badge.Badge = 0;
                        botonVerAjustes.IsEnabled = false;
                    }
                }
                else
                {
                    badge.Badge = "NO PDA";
                    badge.BadgeColorZoneMode = ColorZoneMode.Dark;
                    botonVerAjustes.IsEnabled = false;
                }
                badge.Content = botonVerAjustes;
                Badge_verAjustes = badge;
            }));
        }

        private void CopyDeviceMainDataFileToPublic()
        {
            string publicPathData = ConfigurationManager.AppSettings.Get(Constants.PUBLIC_PATH_DATA);
            string publicPathDataExtended = TextUtils.ExpandEnviromentVariable(publicPathData);
            string filenameAndExtension = ConfigurationManager.AppSettings.Get(Constants.DAT_FILE_DEFAULT);
            IDeviceHandler deviceHandler = App.Instance.deviceHandler;
            ResultFileOperation deviceCopyResult = deviceHandler.CopyDeviceFileToPublicData(publicPathDataExtended, filenameAndExtension);
            if(deviceCopyResult.Equals(ResultFileOperation.OK))
            {
                logger.Debug("Archivo copiado con éxito.");
            }
            else
            {
                string errorCopyDeviceFile = "Error al leer el archivo DEFAULT, Regenerándolo...";
                NotifyCurrentMessage(errorCopyDeviceFile);
                deviceHandler.CreateEmptyDefaultDataFile();
            }
        }

        private void ControlDevicePrograms()
        {
            int dispositivo = 2;
            bool habilitada = true;
            List<VersionDispositivo> versionesActualizadas =  HttpWebClientUtil.GetInfoVersiones(dispositivo, habilitada);
            if(versionesActualizadas != null)
            {
                VersionDispositivo ultimaVersionServidor = versionesActualizadas[0];
                string serverVersion = ultimaVersionServidor.version;
                logger.Debug("Ultima versión en el servidor: " + serverVersion);

                IDeviceHandler deviceHandler = App.Instance.deviceHandler;
                string deviceMainVersion =  deviceHandler.ReadVersionDeviceProgramFileFromDefaultData();
                logger.Debug("Versión del dispositivo: " + deviceMainVersion);

                if(!serverVersion.Equals(deviceMainVersion))
                {
                    NotifyCurrentMessage("Descargando componentes del dispositivo...");
                    HashSet<VersionArchivo> versionesArchivos = (HashSet<VersionArchivo>)ultimaVersionServidor.versionesArchivos;
                    foreach(VersionArchivo versionArchivo in versionesArchivos)
                    {
                        string nombre = versionArchivo.nombre;
                        logger.Debug("Descargando " + nombre);
                        long idVersionArchivo = versionArchivo.idVersion;
                        HttpWebClientUtil.DownloadDevicePrograms(idVersionArchivo, nombre);

                        string destinationDirectory = ConfigurationManager.AppSettings.Get(Constants.DEVICE_RELPATH_BIN);
                        string filenameAndExtension = FileUtils.PrependSlash(nombre);
                        ResultFileOperation copyResult = deviceHandler.CopyPublicBinFileToDevice(destinationDirectory, filenameAndExtension);
                        logger.Debug("Resultado de copiar el archivo " + copyResult.ToString());
                    }
                }
            }
        }

        private bool CheckBranchOfficeDevice()
        {
            bool needAssociate = false;
            IDeviceHandler deviceHandler = App.Instance.deviceHandler;
            string sucursalFromDevice = deviceHandler.ReadBranchOfficeFromDefaultData();
            string sucursalFromLogin = MyAppProperties.idSucursal;
            if(!TextUtils.CompareBranchOffice(sucursalFromDevice, sucursalFromLogin))
            {
                needAssociate = true;
            }
            return needAssociate;
        }

        private void AssociateCurrentBranchOffice(string sucursal)
        {
            //escribir el idSucursal de la session al DEFAULT.DAT en public
            FileUtils.UpdateDefaultDatFileInPublic(sucursal, DeviceMainData.POSITION_SUCURSAL);
            //borrar todos los archivos del dispositivo y public salvo el mismo DEFAULT...
            DeleteAllPreviousFiles();
        }

        private void DeleteAllPreviousFiles()
        {
            List<Actividad> actividades = MyAppProperties.actividadesDisponibles;
            foreach(Actividad actividad in actividades)
            {
                long idActividadActual = actividad.idActividad;
                string filename = ArchivosDATUtils.GetDataFileNameByIdActividad((int)idActividadActual);
                App.Instance.deviceHandler.DeleteDeviceAndPublicDataFiles(filename);
            }
        }

        private void CheckLastSynchronizationDateFromDefault()
        {
            IDeviceHandler deviceHandler = App.Instance.deviceHandler;
            string synchronizationDefault = deviceHandler.ReadSynchronizationDateFromDefaultData();
            bool isGreatherThanToday = DateTimeUtils.IsGreatherThanToday(synchronizationDefault);
            if(isGreatherThanToday)
            {
                DeleteAllPreviousFiles();
            }
        }

        private void UpdateDeviceMainFile(string sucursal)
        {
            FileUtils.UpdateDeviceMainFile(sucursal);
        }

        private void loadCentroActividadesWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            logger.Debug("loadCentroActividades Worker ->runWorkedCompleted");
            PanelLoading = false;
        }

        private void sincronizarWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            logger.Debug("sincronizar Worker ->doWork");
            BannerApp.printSynchronization();
            // buscar datos antiguos
            /*
             * 1-obtener el default.DAT
             *      si no existe crearlo y construirlo
             * 2-obtener la fecha de ultima sincronizacion
             *      si es mayor a 1 dia, consultar descartar datos antiguos
            */

            // buscar recepciones informadas pendientes
            string idSucursal = MyAppProperties.idSucursal;
            string idSincronizacion = HttpWebClientUtil.GetIdLoteActual(idSucursal).ToString();
            bool recepcionesInformadas = HttpWebClientUtil.CheckRecepcionesInformadas(idSincronizacion);
            logger.Info("recepciones Informadas pendientes: " + (recepcionesInformadas ? "NO" : "SI"));
            // Consultar por un SI-No si desea continuar
            bool confirmaDescartarRecepciones = true;
            if(!recepcionesInformadas)
            {
                string message = "Existen recepciones pendientes de informar, ¿Desea continuar y descartar las mismas?";
                string caption = "Aviso";
                MessageBoxResult result = MessageBox.Show(message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
                if (result.Equals(MessageBoxResult.Cancel))
                {
                    confirmaDescartarRecepciones = false;
                }
                else
                {
                    //borrar archivos accion informar ?
                }
            }
            if(confirmaDescartarRecepciones)
            {
                List<Actividad> actividades = MyAppProperties.actividadesDisponibles;
                foreach(Actividad actividad in actividades)
                {
                    if (Constants.DESCARGAR_GENESIX.Equals(actividad.accion.idAccion))
                    {
                        bool descargaMaestroCorrecta = HttpWebClientUtil.BuscarMaestrosDAT((int)actividad.idActividad, idSucursal);
                        PanelSubMessage = "Descargando " + actividad.descripcion.ToString();
                        Thread.Sleep(500);
                        if(descargaMaestroCorrecta)
                        {
                            if(actividad.idActividad.Equals(Constants.ubicart))
                            {
                                logger.Debug("Ubicart -> creando Archivos PAS");
                                ArchivosDATUtils.crearArchivoPAS();
                            }
                            if(actividad.idActividad.Equals(Constants.pedidos))
                            {
                                logger.Debug("Pedidos -> creando Archivos Pedidos");
                                ArchivosDATUtils.crearArchivosPedidos();
                            }

                            string destinationDirectory = ConfigurationManager.AppSettings.Get(Constants.DEVICE_RELPATH_DATA);
                            string filename = ArchivosDATUtils.GetDataFileNameByIdActividad((int)actividad.idActividad);
                            string filenameAndExtension = FileUtils.WrapSlashAndDATExtension(filename);

                            ResultFileOperation result = App.Instance.deviceHandler.CopyPublicDataFileToDevice(destinationDirectory, filenameAndExtension);
                            logger.Debug("result: " + result);
                            PanelSubMessage = "Moviendo al dispositivo";
                            Thread.Sleep(500);
                        }
                    }
                }
            }
        }

        private void sincronizarWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            logger.Debug("sincronizar Worker ->runWorkedCompleted");
            var dispatcher = Application.Current.Dispatcher;
            dispatcher.BeginInvoke(new Action(() =>
            {
                PanelLoading = false;
            }));
            informarWorker.RunWorkerAsync();
        }

        private void informarWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            logger.Debug("informar Worker ->doWork");
            BannerApp.printInformGX();
            //1- Buscar las actividades de informar
            List<Actividad> actividades = MyAppProperties.actividadesDisponibles;
            List<long> actividadesInformar = new List<long>();
            logger.Debug("Buscando las actividad a informar");
            foreach(Actividad actividad in actividades)
            {
                long idAccion = actividad.accion.idAccion;
                if (idAccion.Equals(Constants.INFORMAR_GENESIX))
                {
                    actividadesInformar.Add(actividad.idActividad);
                }
            }
            string urlSincronizacion = ConfigurationManager.AppSettings.Get("API_SYNC_ACTUAL");
            string idSucursal = MyAppProperties.idSucursal;
            string idLote = MyAppProperties.idLoteActual;
            List<Sincronizacion> sincronizaciones = HttpWebClientUtil.GetHttpWebSincronizacion(urlSincronizacion, idSucursal, idLote);
            string sourceDirectory = ConfigurationManager.AppSettings.Get(Constants.DEVICE_RELPATH_DATA);
            foreach(long actividad in actividadesInformar)
            {
                //2- mover las actividades a public
                ArchivoActividad archivoActividad = ArchivosDATUtils.GetArchivoActividadByIdActividad(Convert.ToInt32(actividad));
                ArchivoActividadAttributes archivoActividadAtributos = ArchivosDATUtils.GetAAAttributes(archivoActividad);
                string FilenameAndExtension = FileUtils.PrependSlash(archivoActividadAtributos.nombreArchivo);
                logger.Debug("Buscando archivo : " + FilenameAndExtension);
                ResultFileOperation copyResult = App.Instance.deviceHandler.CopyDeviceFileToPublicData(sourceDirectory, FilenameAndExtension);
                if(copyResult.Equals(ResultFileOperation.OK))
                {
                    //3- enviar los archivos por post al server
                    logger.Debug("Reultado de copiar a public - OK");
                    string filePath = ConfigurationManager.AppSettings.Get(Constants.PUBLIC_PATH_DATA) + FilenameAndExtension;
                    string urlSubirArchivo = ConfigurationManager.AppSettings.Get("API_SUBIR_ARCHIVO");
                    string sincronizacionActividad = "";
                    foreach(Sincronizacion sincronizacion in sincronizaciones)
                    {
                        if(sincronizacion.actividad.idActividad.Equals(actividad))
                        {
                            sincronizacionActividad = sincronizacion.idSincronizacion.ToString();
                            break;
                        }
                    }
                    string parametros = "?idActividad={0}&idSincronizacion={1}&registros={2}";
                    var lineCount = FileUtils.CountRegistryWithinFile(filePath);
                    parametros = String.Format(parametros, actividad, sincronizacionActividad, lineCount);
                    logger.Debug("Subiendo Archivo: " + urlSubirArchivo);
                    string respuesta = HttpWebClientUtil.SendFileHttpRequest(filePath, urlSubirArchivo+parametros);
                    logger.Debug(respuesta);

                    //4- executar el action de informar
                    string urlInformarGX = ConfigurationManager.AppSettings.Get("API_INFORMAR_GX");
                    string responseInformarGX = HttpWebClientUtil.SendHttpGetRequest(urlInformarGX + "?idSucursal=" + idSucursal);
                    logger.Debug(responseInformarGX);
                }
                else
                {
                    logger.Debug("Copia de archivo no OK: " + copyResult.ToString());
                    logger.Debug("Skipeando actividad: " + actividad);
                }
            }


        }

        private void informarWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            logger.Debug("informar Worker ->runWorkedCompleted");
            var dispatcher = Application.Current.Dispatcher;
            dispatcher.BeginInvoke(new Action(() =>
            {
                PanelLoading = false;
            }));
        }

        #endregion

        #region Methods
        public void setInfoLabels()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            label_version = assembly.GetName().Version.ToString(3);
            // de donde obtenemos el usuario y sucursal (?)
            label_usuario = "Admin";
            label_sucursal = MyAppProperties.idSucursal;
            logger.Debug("setInfoLabels[version: " + label_version
                + ", usuario: " + label_usuario + ", sucursal: " + label_sucursal + "]");
        }

        public void ExitPortalApi(object obj)
        {
            logger.Info("exit portal api");
            //aca deberia invocar el logout al portal?
            MainWindow window = (MainWindow) Application.Current.MainWindow;
            Uri uri = new Uri(Constants.LOGIN_VIEW, UriKind.Relative);
            window.frame.NavigationService.Navigate(uri);
        }

        public void SincronizarTodosLosDatos(object obj)
        {
            PanelLoading = true;
            PanelMainMessage = "Espere por favor";
            logger.Info("Sincronizando todos los datos");
            sincronizarWorker.RunWorkerAsync();
        }

        public void InformarGenesix(object obj)
        {
            PanelLoading = true;
            logger.Info("Informando a genesix");
            informarWorker.RunWorkerAsync();
        }

        public void VerAjustes(object obj)
        {
            logger.Debug("Viendo ajustes realizados");
            bool estadoDevice = App.Instance.deviceHandler.IsDeviceConnected();
            if(estadoDevice)
            {
                string motoApiReadDataFile = App.Instance.deviceHandler.ReadAdjustmentsDataFile();
                //por aca sabemos si hay ajustes realizados y de continuar

                List<Ajustes> ajustes = JsonConvert.DeserializeObject<List<Ajustes>>(motoApiReadDataFile);
                if(ajustes != null && ajustes.Count > 0)
                {
                    logger.Debug("hay ajustes realizados");
                    logger.Debug("Dato que sera de vital importancia: ajustes.Count es el numnero del badge");

                    bool isWindowOpen = false;
                    foreach (Window w in Application.Current.Windows)
                    {
                        if (w is VerAjustesView)
                        {
                            isWindowOpen = true;
                            w.Activate();
                        }
                    }

                    if (!isWindowOpen)
                    {
                        VerAjustesView newwindow = new VerAjustesView();
                        newwindow.Show();
                    }
                }
                else
                {
                    //else mostramos un mensaje que no hay datos
                    logger.Debug("No, no hay ajustes hecho, para que habran pulsado en ver ajustes, por curiosidad?");
                    AvisoAlUsuario("No se encontraron Ajustes Realizados");
                }

            }
            else
            {
                AvisoAlUsuario("No se detecta conexion con la PDA");
            }
        }

        public void CentroActividadesLoaded(object obj)
        {
            loadCentroActividadesWorker.RunWorkerAsync();
        }

        public void SincronizacionAnterior(object obj)
        {
            List<Sincronizacion> listaSincronizaciones = null;
            logger.Info("<- Ejecutando la vista anterior a la actual de la grilla");
            string urlSincronizacionAnterior = ConfigurationManager.AppSettings.Get(Constants.API_SYNC_ANTERIOR);
            string _sucursal = MyAppProperties.idSucursal;
            string _idLote = MyAppProperties.idLoteActual;
            listaSincronizaciones = HttpWebClientUtil.GetHttpWebSincronizacion(urlSincronizacionAnterior, _sucursal, _idLote);
            if(listaSincronizaciones != null && listaSincronizaciones.Count != 0)
            {
                logger.Debug(listaSincronizaciones);
                this.sincronizaciones = SincronizacionDtoDataGrid.refreshDataGrid(listaSincronizaciones);
                ActualizarLoteActual(sincronizaciones);
            }
        }

        public void SincronizacionSiguiente(object obj)
        {
            List<Sincronizacion> listaSincronizaciones = null;
            logger.Info("-> Solicitando la vista siguiente de la grilla, si es que la hay...");
            string urlSincronizacionAnterior = ConfigurationManager.AppSettings.Get(Constants.API_SYNC_SIGUIENTE);
            string _sucursal = MyAppProperties.idSucursal;
            string _idLote = MyAppProperties.idLoteActual;
            listaSincronizaciones = HttpWebClientUtil.GetHttpWebSincronizacion(urlSincronizacionAnterior, _sucursal, _idLote);
            if (listaSincronizaciones != null && listaSincronizaciones.Count != 0)
            {
                logger.Debug(listaSincronizaciones);
                this.sincronizaciones = SincronizacionDtoDataGrid.refreshDataGrid(listaSincronizaciones);
                ActualizarLoteActual(sincronizaciones);
            }
        }

        public void BuscarSincronizaciones(object obj)
        {
            logger.Info("Buscando sincronizaciones");
            BuscarLotesView buscarLotesView = new BuscarLotesView();
            buscarLotesView.ShowDialog();
        }

        public void IrUltimaSincronizacion(object obj)
        {
            List<Sincronizacion> listaSincronizaciones = null;
            logger.Info("Llendo a la ultima sincronizacion");
            string urlSincronizacionAnterior = ConfigurationManager.AppSettings.Get(Constants.API_SYNC_ULTIMA);
            string _sucursal = MyAppProperties.idSucursal;
            string _idLote = MyAppProperties.idLoteActual;
            listaSincronizaciones = HttpWebClientUtil.GetHttpWebSincronizacion(urlSincronizacionAnterior, _sucursal, _idLote);
            if (listaSincronizaciones != null && listaSincronizaciones.Count != 0)
            {
                logger.Debug(listaSincronizaciones);
                this.sincronizaciones = SincronizacionDtoDataGrid.refreshDataGrid(listaSincronizaciones);
                ActualizarLoteActual(sincronizaciones);
            }
        }

        public void ActualizarLoteActual(List<SincronizacionDtoDataGrid> sincronizaciones)
        {
            if(sincronizaciones != null && sincronizaciones.Count != 0)
            {
                var s = sincronizaciones[0] as SincronizacionDtoDataGrid;
                string idLoteActual = s.lote;
                MyAppProperties.idLoteActual = idLoteActual;
                ActualizarTxt_Sincronizar(s);
            }
        }

        public void ActualizarTxt_Sincronizar(SincronizacionDtoDataGrid sincronizacion)
        {
            string lote = sincronizacion.lote;
            string fecha = sincronizacion.fecha;
            txt_sincronizacion = " (" + lote + ") " + fecha;
        }

        #region Button States
        public static void BotonEstadoGenesix(object obj)
        {
            logger.Info("Boton estado genesix: " + obj);
            logger.Info(MyAppProperties.SelectedSynchronization.actividad);
        }
        public static void BotonEstadoPDA(object obj)
        {
            logger.Info("Boton estado pda");
            logger.Info(MyAppProperties.SelectedSynchronization.actividad);
        }
        public static void BotonEstadoGeneral(object obj)
        {
            logger.Info("Boton estado general");
            logger.Info(MyAppProperties.SelectedSynchronization.actividad);
        }
        #endregion

        #region Panel Methods
        public void MostrarPanel(object obj)
        {
            PanelLoading = true;
        }
        public void OcultarPanel(object obj)
        {
            PanelLoading = false;
        }
        public void CambiarMainMensage(object obj)
        {
            PanelMainMessage = "Espere por favor";
        }
        public void CambiarSubMensage(object obj)
        {
            PanelSubMessage = "";
        }
        public void CerrarPanel(object obj)
        {
            PanelLoading = false;
        }
        #endregion

        
        public void AvisoAlUsuario(string mensaje)
        {
            string message = mensaje;
            string caption = "Aviso!";
            MessageBoxButton messageBoxButton = MessageBoxButton.OK;
            MessageBoxImage messageBoxImage = MessageBoxImage.Error;
            MessageBoxResult result = MessageBox.Show(message, caption, messageBoxButton, messageBoxImage);
            if (result == MessageBoxResult.OK)
            {
                logger.Debug("Informando al usuario: " + mensaje);
            }
        }
        #endregion
    }
}
