﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UI")]

namespace Dominio
{
    class Campaña : IEquatable<Campaña>
    {
        private int iCodigo;
        private int iInteravaloTiempo;
        private string iNombre;
        private List<RangoFecha> iListaRangosFecha;
        private List<Imagen> iListaImagenes;

        /// <summary>
        /// Constructor de la Campaña
        /// </summary>
        public Campaña()
        {

        }

        /// <summary>
        /// Get/Set del código de la campaña
        /// </summary>
        public int Codigo
        {
            get { return this.iCodigo; }
            set { this.iCodigo = value; }
        }

        /// <summary>
        /// Get/Set del intervalo de tiempo  de la campaña
        /// </summary>
        public int IntervaloTiempo
        {
            get { return this.iInteravaloTiempo; }
            set { this.iInteravaloTiempo = value; }
        }

        /// <summary>
        /// Get/Set del nombre de la campaña
        /// </summary>
        public string Nombre
        {
            get { return this.iNombre; }
            set { this.iNombre = value; }
        }

        /// <summary>
        /// Get/Set de la lista de Rangos de Fecha
        /// </summary>
        public List<RangoFecha> ListaRangosFecha
        {
            get { return this.iListaRangosFecha; }
            set { this.iListaRangosFecha = value; }
        }

        /// <summary>
        /// Get/Set de la lista de imágenes de la campaña
        /// </summary>
        public List<Imagen> ListaImagenes
        {
            get { return this.iListaImagenes; }
            set { this.iListaImagenes = value; }
        }


        /// <summary>
        /// Determina si dos campañas son iguales
        /// </summary>
        /// <param name="other">Otra campaña a comparar</param>
        /// <returns>Tipo de dato booleano que representa si dos campañas son iguales</returns>
        public bool Equals(Campaña other)
        {
            return this.Codigo == other.Codigo;
        }
    }
}
