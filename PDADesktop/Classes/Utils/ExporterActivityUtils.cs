﻿using PDADesktop.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace PDADesktop.Classes.Utils
{
    public class ExporterActivityUtils
    {
        public const string PIPE_DELIMITER = "|";
        public const string NEW_LINE = "\r\n";

        public static string ExportCTRUBIC(ObservableCollection<ControlPrecio> priceControlContent)
        {
            StringBuilder sb = new StringBuilder();
            foreach(ControlPrecio ctrubic in priceControlContent)
            {
                sb.Append(ctrubic.EAN + PIPE_DELIMITER);
                string customDate = DateTime.ParseExact(ctrubic.fecha, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyyMMddHHmmss");
                sb.Append(customDate + PIPE_DELIMITER);
                sb.Append(ctrubic.TipoLectura + PIPE_DELIMITER);
                sb.Append(ctrubic.Pasillo + PIPE_DELIMITER);
                sb.Append((int)ctrubic.ControlUbicacion + PIPE_DELIMITER);
                sb.Append(ctrubic.IDEtiqueta + PIPE_DELIMITER);
                sb.Append(ctrubic.CantidadEtiquetas + PIPE_DELIMITER);
                sb.Append(ctrubic.AlertaStock ? "1" : "0" + PIPE_DELIMITER);
                sb.Append(ctrubic.NumeroSecuencia + NEW_LINE);
            }
            return sb.ToString();
        }

        public static string ExportAJUSTES(ObservableCollection<Ajustes> adjustmentContent)
        {
            StringBuilder sb = new StringBuilder();
            foreach(Ajustes ajuste in adjustmentContent)
            {
                sb.Append(ajuste.ean + PIPE_DELIMITER);
                sb.Append(ajuste.fechaAjuste + PIPE_DELIMITER);
                sb.Append(ajuste.motivo + PIPE_DELIMITER);
                sb.Append(ajuste.perfilGenesix + PIPE_DELIMITER);
                sb.Append(ajuste.cantidad + NEW_LINE);
            }
            return sb.ToString();
        }

        public static string ExportRECEP(ObservableCollection<ArticuloRecepcion> receptionContent)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ArticuloRecepcion articuloRecepcion in receptionContent)
            {
                Recepcion recepcion = articuloRecepcion.recepcion;
                sb.Append(recepcion.fechaRecep + PIPE_DELIMITER);
                sb.Append(recepcion.numeroPedido + PIPE_DELIMITER);
                sb.Append(recepcion.numeroProveedor + PIPE_DELIMITER);
                sb.Append(recepcion.centroEmisor + PIPE_DELIMITER);
                sb.Append(recepcion.numeroRemito + PIPE_DELIMITER);
                sb.Append(recepcion.fechaRem + PIPE_DELIMITER);
                sb.Append(articuloRecepcion.EAN + PIPE_DELIMITER);
                sb.Append(articuloRecepcion.unidadesRecibidas + PIPE_DELIMITER);
                sb.Append(recepcion.descripcionProveedor + NEW_LINE);
            }
            return sb.ToString();
        }

        public static string ExportETIQ(ObservableCollection<Etiqueta> labelContent)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Etiqueta etiqueta in labelContent)
            {
                sb.Append(etiqueta.EAN + PIPE_DELIMITER);
                sb.Append(etiqueta.Fecha + PIPE_DELIMITER);
                sb.Append(etiqueta.CodigoEtiqueta + NEW_LINE);
            }
            return sb.ToString();
        }
    }
}