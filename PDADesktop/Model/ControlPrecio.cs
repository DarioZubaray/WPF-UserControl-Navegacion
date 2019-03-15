﻿using System;
using System.ComponentModel;

namespace PDADesktop.Model
{
    public class ControlPrecio
    {
        public string EAN { get; set; }
        public string fecha {
            get
            {
                return FechaControl.ToString();
            }
            set
            {
                FechaControl = DateTime.ParseExact(value, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        public DateTime FechaControl { get; set; }
        public int TipoLectura { get; set; }
        public string Pasillo { get; set; }
        public TipoControlUbicacion ControlUbicacion { get; set; }
        public string IDEtiqueta { get; set; }
        public int CantidadEtiquetas { get; set; }
        public bool AlertaStock { get; set; }
        public string NumeroSecuencia { get; set; }

        public enum TipoControlUbicacion
        {
            UbicCorrecta = 0,
            UbicIncorrecta = 1,
            UbicNueva = 2
        }

        public enum TipoDeLectura
        {
            Sinlectura = 0,
            EAN_SOLO = 1,
            EAN_FECHA_OK = 2,
            EAN_FECHA_Vencida = 3
        }

    }
}