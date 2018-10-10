﻿using log4net;
using PDADesktop.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Configuration;
using System.Net;
using System.IO;
using System.Text;
using PDADesktop.Model.Dto;

namespace PDADesktop.Classes.Utils
{
    class HttpWebClientUtil
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Requests
        private static string SendHttpGetRequest(string urlPath)
        {
            string response = null;
            string urlAuthority = ConfigurationManager.AppSettings.Get("SERVER_HOST_PROTOCOL_IP_PORT");
            try
            {
                logger.Debug("Enviando petición a " + urlAuthority + urlPath);
                using (var client = new PDAWebClient(20000))
                {
                    response = client.DownloadString(urlAuthority + urlPath);
                    if (response.Length < 100)
                    {
                        logger.Debug("response: " + response);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.GetType() + " - " + e.Message);
                ShowErrorMessage(e);
            }
            return response;
        }

        private static string SendHttpPostRequest(string urlPath, string jsonBody)
        {
            string result = null;
            string urlAuthority = ConfigurationManager.AppSettings.Get("SERVER_HOST_PROTOCOL_IP_PORT");
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(urlAuthority + urlPath);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(jsonBody);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            return result;
        }

        private static string SendFileHttpRequest(string filePath, string url)
        {
            WebResponse response = null;
            try
            {
                filePath = TextUtils.ExpandEnviromentVariable(filePath);
                string urlAuthority = ConfigurationManager.AppSettings.Get("SERVER_HOST_PROTOCOL_IP_PORT");
                string sWebAddress = urlAuthority + url;

                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(sWebAddress);
                wr.Timeout = 1000 * 15;
                wr.ContentType = "multipart/form-data; boundary=" + boundary;
                wr.Method = "POST";
                wr.KeepAlive = true;
                wr.Credentials = CredentialCache.DefaultCredentials;
                Stream stream = wr.GetRequestStream();

                stream.Write(boundarybytes, 0, boundarybytes.Length);
                byte[] formitembytes = Encoding.UTF8.GetBytes(filePath);
                stream.Write(formitembytes, 0, formitembytes.Length);
                stream.Write(boundarybytes, 0, boundarybytes.Length);
                string headerTemplate = "Content-Disposition: form-data; name=\"archivo\"; filename=\"{0}\"\r\nContent-Type: application/octet-stream\r\n\r\n";
                string header = string.Format(headerTemplate, Path.GetFileName(filePath));
                byte[] headerbytes = Encoding.UTF8.GetBytes(header);
                stream.Write(headerbytes, 0, headerbytes.Length);

                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    stream.Write(buffer, 0, bytesRead);
                fileStream.Close();

                byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                stream.Write(trailer, 0, trailer.Length);
                stream.Close();

                response = wr.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(responseStream);
                string responseData = streamReader.ReadToEnd();
                return responseData;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }
        #endregion

        public static void ShowErrorMessage(Exception e)
        {
            string message = e.GetType() + " - " + e.Message;
            string caption = "Error de comunicacion con PDA Express Server";
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        internal static List<Accion> GetAllActions()
        {
            string urlAcciones = ConfigurationManager.AppSettings.Get(Constants.API_GET_ALL_ACCIONES);
            var responseAcciones = SendHttpGetRequest(urlAcciones);
            if (responseAcciones != null)
            {
                return JsonUtils.GetListAcciones(responseAcciones);
            }
            else
            {
                return null;
            }
        }

        internal static List<Actividad> GetActivitiesByActionId(List<Accion> actions)
        {
            string actionsIds = TextUtils.ParseListAccion2String(actions);
            string jsonBody = "{ \"idAcciones\": " + actionsIds.ToString() + "}";

            var urlActivities = ConfigurationManager.AppSettings.Get(Constants.API_GET_ACTIVIDADES);
            string responseActivities = HttpWebClientUtil.SendHttpPostRequest(urlActivities, jsonBody);
            logger.Debug(responseActivities);
            return JsonUtils.GetListActividades(responseActivities);
        }

        internal static bool DownloadFileFromServer(string urlPath, string filenameAndExtension, string destino)
        {
            var client = new WebClient();
            string userAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
            client.Headers.Add("user-agent", userAgent);
            string urlAuthority = ConfigurationManager.AppSettings.Get("SERVER_HOST_PROTOCOL_IP_PORT");
            try
            {
                logger.Debug("Enviando petición a " + urlAuthority + urlPath);
                destino = TextUtils.ExpandEnviromentVariable(destino);
                FileUtils.VerifyFoldersOrCreate(destino);
                logger.Debug("Descargando en: " + destino + filenameAndExtension);
                client.DownloadFile(urlAuthority + urlPath, destino + filenameAndExtension);
                return true;
            }
            catch (Exception e)
            {
                logger.Error(e.GetType() + " - " + e.Message);
                ShowErrorMessage(e);
                return false;
            }
        }

        internal static Boolean GetHttpWebServerConexionStatus()
        {
            Boolean conexionStatus = false;
            string urlServerStatus = ConfigurationManager.AppSettings.Get("API_SERVER_CONEXION_STATUS");
            string response = SendHttpGetRequest(urlServerStatus);
            if (response != null)
            {
                conexionStatus = response.Length > 0 ? true : false;
            }
            return conexionStatus;
        }

        internal static int GetCurrentBatchId(string storeId)
        {
            int batchId = 0;
            string urlPathGetCurrentBatchId = ConfigurationManager.AppSettings.Get("API_SYNC_ID_LOTE");
            string urlPath_urlQuery = String.Format("{0}?idSucursal={1}", urlPathGetCurrentBatchId, storeId);
            string responseGetCurrentBatchId = SendHttpGetRequest(urlPath_urlQuery);
            if (responseGetCurrentBatchId != null && !responseGetCurrentBatchId.Equals("null"))
            {
                if (responseGetCurrentBatchId.Contains("\""))
                {
                    responseGetCurrentBatchId = responseGetCurrentBatchId.Replace("\"", "");
                }
                batchId = Convert.ToInt32(responseGetCurrentBatchId);
            }
            return batchId;
        }

        internal static List<Sincronizacion> GetHttpWebSynchronizations(string urlPath, string storeId, string batchId)
        {
            List<Sincronizacion> syncs = null;
            string urlPath_urlQuery = String.Format("{0}?idSucursal={1}&idLote={2}", urlPath, storeId, batchId);
            string response = SendHttpGetRequest(urlPath_urlQuery);
            if (response != null)
            {
                syncs = JsonUtils.GetListSinchronization(response);
            }

            return syncs;
        }

        internal static List<string> GetAdjustmentsTypes()
        {
            List<string> adjustmentsTypes = null;
            string urlPathGetAdjustmentsTypes = ConfigurationManager.AppSettings.Get("API_GET_TIPOS_AJUSTES");
            string responseGetAdjustmentsTypes = SendHttpGetRequest(urlPathGetAdjustmentsTypes);
            if (responseGetAdjustmentsTypes != null)
            {
                adjustmentsTypes = JsonUtils.GetListStringOfAdjustment(responseGetAdjustmentsTypes);
            }
            return adjustmentsTypes;
        }

        internal static bool CheckInformedReceptions(string idSincronizacion)
        {
            bool informedReceptions = false;
            string urlPathInformedReceptions = ConfigurationManager.AppSettings.Get("API_BUSCAR_RECEPCIONES_INFORMADAS");
            string urlPath_urlQuery = String.Format("{0}?idSincronizacion={1}", urlPathInformedReceptions, idSincronizacion);
            string response = SendHttpGetRequest(urlPath_urlQuery);
            if (response != null)
            {
                informedReceptions = response.Equals("\"1\"") ? true : false;
            }
            return informedReceptions;
        }

        internal static bool BuscarMaestrosDAT(int activityId, string storeId)
        {
            string masterFile = ArchivosDATUtils.GetDataFileNameByIdActividad(activityId);
            string urlPathMasterFile = ConfigurationManager.AppSettings.Get("API_MAESTRO_URLPATH");
            urlPathMasterFile = String.Format(urlPathMasterFile, masterFile);
            string urlPath_urlQuery = String.Format("{0}?idSucursal={1}", urlPathMasterFile, storeId);
            string slashFilenameAndExtension = FileUtils.WrapSlashAndDATExtension(masterFile);
            string publicPathData = ConfigurationManager.AppSettings.Get(Constants.PUBLIC_PATH_DATA);

            return DownloadFileFromServer(urlPath_urlQuery, slashFilenameAndExtension, publicPathData);
        }

        internal static List<VersionDispositivo> GetInfoVersions(int device, Boolean enabled)
        {
            string urlGetInfoVersions = ConfigurationManager.AppSettings.Get(Constants.API_GET_INFO_VERSION);
            string queryParams = "?dispositivo={0}&habilitada={1}";
            queryParams = String.Format(queryParams, device, enabled);
            var responseInfoVersions = HttpWebClientUtil.SendHttpGetRequest(urlGetInfoVersions + queryParams);
            if (responseInfoVersions != null)
            {
                logger.Debug("respuesta recibida de GetInfoVersiones");
                logger.Debug(responseInfoVersions);
                List<VersionDispositivo> infoVersions = JsonUtils.GetVersionDispositivo(responseInfoVersions);
                return new List<VersionDispositivo>(infoVersions);
            }
            else
            {
                return new List<VersionDispositivo>();
            }
        }

        internal static void DownloadDevicePrograms(string versionFileId, string name)
        {
            string urlDownloadFile = ConfigurationManager.AppSettings.Get(Constants.API_DOWNLOAD_PROGRAM_FILE);
            string queryParams = "?idVersionArchivo={0}";
            queryParams = String.Format(queryParams, versionFileId);
            
            string publicPathBin = ConfigurationManager.AppSettings.Get(Constants.PUBLIC_PATH_BIN);
            DownloadFileFromServer(urlDownloadFile+queryParams, FileUtils.PrependSlash(name), publicPathBin);
        }

        internal static ActionResultDto VerifyNewBatch(string storeId)
        {
            string urlVerifyNewBatch = ConfigurationManager.AppSettings.Get(Constants.API_VERIFY_NEW_BATCH);
            string queryParams = "?idSucursal=" + storeId;
            string verifyNewBatchResponse = SendHttpGetRequest(urlVerifyNewBatch + queryParams);
            ActionResultDto actionResult = JsonUtils.GetActionResult(verifyNewBatchResponse);
            return actionResult;
        }

        internal static string GetLastVersionProgramFileFromServer(string queryParams)
        {
            string urlLastVersion = ConfigurationManager.AppSettings.Get(Constants.API_GET_LAST_VERSION_FILE_PROGRAM);
            return SendHttpGetRequest(urlLastVersion + queryParams);
        }

        internal static string SetSentGenesixState(string queryParams)
        {
            string urlSentGX = ConfigurationManager.AppSettings.Get("API_SET_SENT_GX");
            return SendHttpGetRequest(urlSentGX + queryParams);
        }

        internal static string SetReceivedDeviceState(string queryParams)
        {
            string urlReceivedFromDevice = ConfigurationManager.AppSettings.Get("API_SET_RECEIVED_PDA");
            return SendHttpGetRequest(urlReceivedFromDevice + queryParams);
        }

        internal static List<Sincronizacion> CreateNewBatch(string storeId, bool isCompleted)
        {
            string urlCreateNewBatch = ConfigurationManager.AppSettings.Get(Constants.API_CREATE_NEW_BATCH);
            string jsonBody = "{ \"idSucursal\":" + storeId + ", \"idAcciones\": [{}]";
            if(isCompleted)
            {
                jsonBody = String.Format(jsonBody, "1, 2");
            }
            else
            {
                jsonBody = String.Format(jsonBody, "1");
            }
            string responseCreateNewBatch = SendHttpPostRequest(urlCreateNewBatch, jsonBody);
            List<Sincronizacion> newSync = JsonUtils.GetListSinchronization(responseCreateNewBatch);
            return newSync;
        }
    }
}
