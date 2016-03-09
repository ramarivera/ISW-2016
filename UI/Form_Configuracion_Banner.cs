﻿using System;
using System.Windows.Forms;
using Servicios;
using System.Collections.Generic;
using UI.Tipos;
using System.Drawing;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace UI
{
    public partial class Form_Configuracion_Banner : Form
    {
        #region Variables
        /// <summary>
        /// Banner sobre el cual trabajar
        /// </summary>
        private Banner iBanner;
        /// <summary>
        /// Cantidad de Rangos Fecha agregados
        /// </summary>
        private int iCantRangosFecha;
        /// <summary>
        /// Determina si los rangos fecha están completos
        /// </summary>
        private bool iRangosFechaCompletos;
        /// <summary>
        /// Variable booleana que determina cuando cerrar la ventana de forma permanente o cuando preguntar para guardar
        /// </summary>
        private bool iCerrarCodigo;
        /// <summary>
        /// Variable booleana que determina si se activa o no el handler del SelectionChanged Event
        /// Para ello verifica que la fila actual no sea la última sino el CurrentRow del SelectionChange lanza excepción
        /// </summary>
        private bool iSCEActive;
        /// <summary>
        /// Variable que determina que hace la ventana
        /// </summary>
        private Form_Banner.delegado iFuncionVentana;
        #endregion

        #region Región: Inicialización y Carga
        /// <summary>
        /// Constructor de la ventana
        /// </summary>
        /// <param name="funcionVentana">Función que realizará la ventana a la hora de presionar boton Aceptar</param>
        /// <param name="pBanner">Banner sobre el cual trabajar, sino es nulo</param>
        public Form_Configuracion_Banner(Form_Banner.delegado funcionVentana, Banner pBanner = null)
        {
            InitializeComponent();
            this.ConfiguracionInicialDataGridView();
            bool auxiliar = pBanner != null;
            if (auxiliar)
            {
                this.iBanner = pBanner;
            }
            else
            {
                this.ChangeEnableGroupBoxHorario(false);
                this.button_EliminarFecha.Enabled = false;
                this.iBanner = new Banner();
                this.iBanner.URL = "";
                this.iBanner.Texto = "";
            }
            this.iSCEActive = true;
            this.ConfigInicialForms();
            this.Configuracion(auxiliar);
            this.iFuncionVentana = funcionVentana;
            this.ActualizarListaFechas();
            this.textBox_Nombre.Focus();
        }

        /// <summary>
        /// Configura forms de la ventana en el inicio
        /// </summary>
        private void ConfigInicialForms()
        {
            this.iCerrarCodigo = false;
            this.CancelButton = this.button_Cancelar;
            this.AcceptButton = this.button_Aceptar;
            this.iCantRangosFecha = this.iBanner.ListaRangosFecha.Count;
            this.textBox_Nombre.Text = this.iBanner.Nombre;
            if (this.iBanner.URL != "")
            {
                this.radioButton_FuenteRSS.Checked = true;
                this.groupBox_TextoFijo.Enabled = false;
                this.textBox_URL.Text = this.iBanner.URL;
            }
            else
            {
                this.radioButton_TextoFijo.Checked = true;
                this.groupBox_RSS.Enabled = false;
                this.textBox_TextoFijo.Text = this.iBanner.Texto;
            }
            this.button_AgregarHora.Image = ImagenServices.CambiarTamañoImagen(Properties.Resources.Modificar, this.button_AgregarHora.Size.Width, this.button_AgregarHora.Size.Height);
        }

        /// <summary>
        /// Determina los valores de ciertas variables y la imagen inicial de los parte inferior del form
        /// </summary>
        /// <param name="value">Valor a configurar</param>
        private void Configuracion(bool value)
        {
            this.iRangosFechaCompletos = value;
            this.timer_Prueba.Enabled = value;
            this.button_Aceptar.Enabled = value;
            this.CampoCompleto(this.pictureBox_ComprobacionNombre, value);
            this.CampoCompleto(this.pictureBox_ComprobacionTipo, value);
            this.CampoCompleto(this.pictureBox_ComprobacionRF, value);
            this.CampoCompleto(this.pictureBox_ComprobacionRH, value);
        }
        #endregion

        #region Región: Eventos Comunes
        /// <summary>
        /// Evento que surge al hacer clic sobre el botón Cancelar
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void button_Cancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Evento que surge al hacer clic sobre el botón Aceptar
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void button_Aceptar_Click(object sender, EventArgs e)
        {
            this.iBanner.Nombre = this.textBox_Nombre.Text;
            if (this.radioButton_TextoFijo.Checked)
            {
                this.iBanner.URL = "";
                this.iBanner.Texto = this.textBox_TextoFijo.Text;
            }
            else
            {
                this.iBanner.URL = this.textBox_URL.Text;
                this.iBanner.Texto = this.label_ValorPrueba.Text;
            }
            this.backgroundWorker_BotonAceptar.RunWorkerAsync(this.iBanner);
            ((Form_Banner)this.Owner).EnEspera(false);
            ((Form_Banner)this.Owner).HijoCerrandose();
        }

        /// <summary>
        /// Evento que surge antes de que la ventana se cierre
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void Form_Configuracion_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((!this.iCerrarCodigo) && (e.CloseReason == CloseReason.UserClosing))
            {
                DialogResult result = MessageBox.Show("¿Está seguro que desea regresar sin guardar? Se perderán los datos", "Atención", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Yes)
                {
                    ((Form_Banner)this.Owner).HijoCerrandose();
                }
                else
                {
                    //Cancela el evento de cerrar la ventana
                    e.Cancel = true;
                }
            }
            else
            {
                ((Form_Banner)this.Owner).HijoCerrandose();
            }
        }
        #endregion

        #region Región: Métodos Extra Comunes
        /// <summary>
        /// Activa el botón Aceptar si todos los Campos están llenos
        /// </summary>
        private void ActivarAceptar()
        {
            bool valorFinal = (this.textBox_Nombre.Text != "") &&
                              (((textBox_URL.Text != "") && this.radioButton_FuenteRSS.Checked) ||
                              ((textBox_TextoFijo.Text != "") && this.radioButton_TextoFijo.Checked)) &&
                              (this.iBanner.ListaRangosFecha.Count > 0) &&
                              this.iRangosFechaCompletos;
            this.button_Aceptar.Enabled = valorFinal;
        }

        /// <summary>
        /// Determina el ícono que representa el estado del campo
        /// </summary>
        /// <param name="pPictureBox">Form que contiene la imagen</param>
        /// <param name="value">Valor booleano que representa si está o no completo el campo correspondiente</param>
        private void CampoCompleto(PictureBox pPictureBox, bool value)
        {
            int anchoComun = pPictureBox.Width;
            int altoComun = pPictureBox.Height;
            if (value)
            {
                pPictureBox.Image = ImagenServices.CambiarTamañoImagen(Properties.Resources.greenTick, anchoComun, altoComun);

            }
            else
            {
                pPictureBox.Image = ImagenServices.CambiarTamañoImagen(SystemIcons.Exclamation.ToBitmap(), anchoComun, altoComun);
            }
        }
        #endregion

        #region Región: Pestaña Configuración Básica
        #region Eventos

        /// <summary>
        /// Evento que surge al ingresar entradas de teclas al Nombre
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void textBox_Nombre_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && !char.IsSeparator(e.KeyChar) && !char.IsSymbol(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Evento que surge al salir del textBox Nombre
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void textBox_Nombre_Leave(object sender, EventArgs e)
        {
            this.textBox_Nombre.Text = this.textBox_Nombre.Text.TrimStart(' ').TrimEnd(' ');
            bool resultado = Regex.IsMatch(this.textBox_Nombre.Text, @"^[a-zA-Záíéóú\s\p{P}]+$");
            this.CampoCompleto(this.pictureBox_ComprobacionNombre, resultado);
            if (!resultado)
            {
                this.textBox_Nombre.Text = "";
            }
            this.ActivarAceptar();

        }

        /// <summary>
        /// Evento que surge al checkear el radioButton Fuente RSS
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void radioButton_FuenteRSS_CheckedChanged(object sender, EventArgs e)
        {
            this.groupBox_TextoFijo.Enabled = !this.radioButton_FuenteRSS.Checked;
            this.groupBox_RSS.Enabled = this.radioButton_FuenteRSS.Checked;
            this.CampoCompleto(this.pictureBox_ComprobacionTipo, false);
            this.textBox_URL.Focus();
            this.MovimientoLabel("", this.panel_Prueba.Location.X + this.panel_Prueba.Size.Width);
        }

        /// <summary>
        /// Evento que surge al checkear el radioButton Fuente RSS
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void radioButton_TextoFijo_CheckedChanged(object sender, EventArgs e)
        {
            this.CampoCompleto(this.pictureBox_ComprobacionTipo, false);
            this.textBox_TextoFijo.Focus();
            this.MovimientoLabel("", this.panel_Prueba.Location.X + this.panel_Prueba.Size.Width);
        }

        /// <summary>
        /// Evento que surge al ingresar entradas de teclas al textoBox (RSS o textoFijo)
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Evento que surge al salir del textBox URL del Banner
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void textBox_URL_Leave(object sender, EventArgs e)
        {
            this.textBox_URL.Text = this.textBox_URL.Text.TrimStart(' ').TrimEnd(' ');
            bool resultado = WebServices.ComprobarURLVálidaRSS(this.textBox_URL.Text);
            if (!resultado)
            {
                this.textBox_URL.Text = "";
                MessageBox.Show("La URL especificada no es válida, ingrese nuevamente");
            }
            else
            {
                if (this.backgroundWorker_RSS.IsBusy)
                {
                    this.backgroundWorker_RSS.CancelAsync();
                }
                this.backgroundWorker_RSS.RunWorkerAsync(this.textBox_URL.Text);
            }
            this.CampoCompleto(this.pictureBox_ComprobacionTipo, resultado);
            this.ActivarAceptar();
        }

        /// <summary>
        /// Evento que surge al salir del textBox TextoFijo del Banner
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void textBox_TextoFijo_Leave(object sender, EventArgs e)
        {
            this.textBox_TextoFijo.Text = this.textBox_TextoFijo.Text.TrimStart(' ').TrimEnd(' ');
            this.CampoCompleto(this.pictureBox_ComprobacionTipo, this.textBox_TextoFijo.Text != "");
            this.ActivarAceptar();
        }

        /// <summary>
        /// Evento que surge cuando se sale del tableLayoutPanel del Tipo Banner
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void tableLayoutPanelTipo_Leave(object sender, EventArgs e)
        {
            if (this.radioButton_TextoFijo.Checked)
            {
                if (this.textBox_TextoFijo.Text != "")
                {
                    this.MovimientoLabel(this.textBox_TextoFijo.Text, this.panel_Prueba.Location.X + this.panel_Prueba.Size.Width);
                }
            }
        }

        /// <summary>
        /// Evento que surge cuando el timer hace un tick
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void timer_Prueba_Tick(object sender, EventArgs e)
        {
            this.label_ValorPrueba.Left -= 1;
            if (this.label_ValorPrueba.Left + this.label_ValorPrueba.Width + 5 < this.panel_Prueba.Left)
            {
                this.label_ValorPrueba.Left = this.panel_Prueba.Width + this.panel_Prueba.Location.X;
            }
        }

        /// <summary>
        /// Evento que surge cuando se cambia de pestaña
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.timer_Prueba.Enabled = (this.tabControl.SelectedIndex == 0) && (this.label_ValorPrueba.Text != "");
        }
        #endregion

        #region Métodos Extra
        /// <summary>
        /// Cambia el valor del label y su posición inicial y activa/desactiva el timer
        /// </summary>
        /// <param name="pTexto">Texto a mostrar en el label</param>
        /// <param name="pPosInicial">Posición Inicial del label</param>
        private void MovimientoLabel(string pTexto, int pPosInicial)
        {
            this.label_ValorPrueba.Left = pPosInicial;
            this.label_ValorPrueba.Text = pTexto;
            this.timer_Prueba.Enabled = pTexto != "";
        }
        #endregion

        #region Procesos Segundo Plano
        /// <summary>
        /// Evento que surge cuando el backgroundworker realiza las operaciones de RSS
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void backgroundWorker_RSS_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Comunicacion.OperacionesRSS((string)e.Argument);
        }

        /// <summary>
        /// Evento que surge cuando el backgroundworker terminar de realizar las operaciones de RSS
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void backgroundWorker_RSS_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                string mensaje = "Ha ocurrido el siguiente error durante el proceso:" + Environment.NewLine + e.Error.Message +
                                 Environment.NewLine + "¿Desea cargar el RSS nuevamente?";
                DialogResult result = MessageBox.Show(mensaje, "Confirmation", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    this.backgroundWorker_RSS.RunWorkerAsync();
                }
            }
            else if (!e.Cancelled)
            {
                this.MovimientoLabel((string)e.Result, this.panel_Prueba.Location.X + this.panel_Prueba.Size.Width);
            }
            /*
            else
            {
                if(this.textBox_URL.Text != "")
                {
                    this.backgroundWorker_RSS.RunWorkerAsync(this.textBox_URL.Text);
                }
            }
            */
        }
        #endregion
        #endregion

        #region Región: Pestaña Rango Horario
        #region Eventos
        /// <summary>
        /// Evento que surge al seleccionar un Rango Fecha
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void dataGridView_Fecha_SelectionChanged(object sender, EventArgs e)
        {
            if (this.iSCEActive)
            {
                this.ActualizarListaHorarios(this.RangoFechaSeleccionado());
            }            
        }

        /// <summary>
        /// Evento que surge al hacer clic sobre el botón de Agregar del groupBox Rango de Fechas
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void button_AgregarFecha_Click(object sender, EventArgs e)
        {
            DateTime auxFechaI = this.dateTimePicker_RangoFechaDesde.Value.Date;
            DateTime auxFechaF = this.dateTimePicker_RangoFechaHasta.Value.Date;
            int resultadoC = auxFechaI.CompareTo(auxFechaF);
            if ((resultadoC <= 0) && (this.VerificarRangoFecha(auxFechaI, auxFechaF)))
            {
                this.iSCEActive = true;
                this.iCantRangosFecha += 1;
                RangoFecha auxRangoFecha = new RangoFecha()
                {
                    FechaInicio = auxFechaI,
                    FechaFin = auxFechaF,
                    Codigo = this.iCantRangosFecha
                };
                this.iBanner.ListaRangosFecha.Add(auxRangoFecha);
                this.button_EliminarFecha.Enabled = true;
                this.ChangeEnableGroupBoxHorario(true);                
                this.ActualizarListaFechas();
                this.MarcarFilasIncompletas();
                this.CampoCompleto(this.pictureBox_ComprobacionRF, true);
                this.CampoCompleto(this.pictureBox_ComprobacionRH, this.iRangosFechaCompletos);
            }
            else
            {
                string mensaje;
                if(resultadoC > 0)
                {
                    mensaje = "La fecha de fin (Hasta) debe ser mayor o igual a la fecha de inicio (Desde)";
                }
                else
                {
                    mensaje = "El rango de fechas ya existe";
                }
                MessageBox.Show(mensaje);
            }
        }

        /// <summary>
        /// Evento que surge al hacer clic sobre el botón de Eliminar del groupBox Rango de Fechas
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void button_EliminarFecha_Click(object sender, EventArgs e)
        {
            RangoFecha auxRangoFecha = this.RangoFechaSeleccionado();
            this.iSCEActive = !(this.dataGridView_Fecha.CurrentRow.Index + 1 == this.dataGridView_Fecha.RowCount);
            this.iBanner.ListaRangosFecha.Remove(auxRangoFecha);
            this.ActualizarListaFechas();
            this.ActivarAceptar();
            this.button_AgregarFecha.Enabled = true;
            bool aux = this.iBanner.ListaRangosFecha.Count > 0;
            this.ChangeEnableGroupBoxHorario(aux);
            this.button_EliminarFecha.Enabled = aux;
            this.MarcarFilasIncompletas();
            //Sólo se ejecuta luego de que se elimina la última fila, luego de actualizar el DGV Fecha y así actualiza el Rango horario con el 
            //primero, sino tira error. Alternativa: sacar el EventHandler del SelectionChanged de DGV fecha cuando se elimina la última fila;
            if (!this.iSCEActive && (this.dataGridView_Fecha.RowCount > 0))
            {
                this.ActualizarListaHorarios(this.RangoFechaSeleccionado());
            }
            this.iSCEActive = true;
            this.CampoCompleto(this.pictureBox_ComprobacionRF, this.iBanner.ListaRangosFecha.Count > 0);
            this.CampoCompleto(this.pictureBox_ComprobacionRH, this.iRangosFechaCompletos);
        }

        /// <summary>
        /// Evento que surge al hacer clic sobre el botón de Agregar del groupBox Rango Horario
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void button_AgregarHora_Click(object sender, EventArgs e)
        {
            this.button_AgregarFecha.Enabled = true;
            Form_SeleccionRangoHorario seleccionHorarios = 
                                    new Form_SeleccionRangoHorario(this.RangoFechaSeleccionado(),this.ListaRangosHorariosRF(),true);
            seleccionHorarios.Owner = this;
            seleccionHorarios.ShowDialog();
        }

        /// <summary>
        /// Evento que surge al hacer clic sobre el botón de Eliminar del groupBox Rango Horario
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void button_EliminarHora_Click(object sender, EventArgs e)
        {
            RangoHorario rangoHorario = this.RangoHorarioSeleccionado();
            RangoFecha auxRangoFecha = this.RangoFechaSeleccionado();
            auxRangoFecha.ListaRangosHorario.Remove(rangoHorario);
            this.ActualizarListaHorarios(auxRangoFecha);
            int cantidadRangosHor = auxRangoFecha.ListaRangosHorario.Count;
            this.button_AgregarFecha.Enabled = cantidadRangosHor > 0;
            this.MarcarFilasIncompletas();
            this.ActivarAceptar();
            this.CampoCompleto(this.pictureBox_ComprobacionRH, this.iRangosFechaCompletos);
        }
        #endregion

        #region Métodos Extra
        /// <summary>
        /// Configura los DataGridView para que muestren las columnas correspondientes
        /// </summary>
        private void ConfiguracionInicialDataGridView()
        {
            //CONFIGURACIÓN DataGridView FECHA
            this.dataGridView_Fecha.AutoGenerateColumns = false;
            this.dataGridView_Fecha.AutoSize = false;
            // Ininicializa la columna de la 'Fecha Inicio'
            DataGridViewColumn column = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "FechaInicio",
                Name = "Fecha de inicio",
                ValueType = typeof(DateTime)
            };
            this.dataGridView_Fecha.Columns.Add(column);
            // Ininicializa la columna de la 'Fecha Fin'
            column = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "FechaFin",
                Name = "Fecha Fin",
                ValueType = typeof(DateTime)
            };
            this.dataGridView_Fecha.Columns.Add(column);
            // Ininicializa la columna del 'Codigo' (No Visible)
            column = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "Codigo",
                Name = "Codigo",
                Visible = false,
                ValueType = typeof(int),
            };
            this.dataGridView_Fecha.Columns.Add(column);

            //CONFIGURACIÓN DataGridView HORARIO
            this.dataGridView_Hora.AutoGenerateColumns = false;
            this.dataGridView_Hora.AutoSize = false;
            // Ininicializa la columna de la 'Hora Inicio'
            column = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "HoraInicio",
                Name = "Hora de inicio",
                ValueType = typeof(TimeSpan)
            };
            this.dataGridView_Hora.Columns.Add(column);
            // Ininicializa la columna de la 'Hora Fin'
            column = new DataGridViewTextBoxColumn()
            {
                DataPropertyName = "HoraFin",
                Name = "Hora Finalización",
                ValueType = typeof(TimeSpan)
            };
            this.dataGridView_Hora.Columns.Add(column);
        }

        /// <summary>
        /// Cambia la Habilitación todos los forms contenidos dentro de un groupBox 
        /// </summary>
        /// <param name="value">Valor a cambiar la habilitación</param>
        private void ChangeEnableGroupBoxHorario(bool value)
        {
            this.groupBox_RangoHorario.Enabled = value;
        }

        /// <summary>
        /// Devuleve el rango de fecha seleccionado
        /// </summary>
        /// <returns>Tipo de dato UI.Tipos.RangoFecha que representa el rango de Fecha que ha sido seleccionado del DataGridView </returns>
        private RangoFecha RangoFechaSeleccionado()
        {
            return(RangoFecha)this.dataGridView_Fecha.CurrentRow.DataBoundItem;
        }

        /// <summary>
        /// Devuleve el rango de horario seleccionado
        /// </summary>
        /// <returns>Tipo de dato UI.Tipos.RangoHorario que representa el rango horario que ha sido seleccionado del DataGridView</returns>
        private RangoHorario RangoHorarioSeleccionado()
        {
            return (RangoHorario)this.dataGridView_Hora.CurrentRow.DataBoundItem;
        }

        /// <summary>
        /// Actualiza el dataGridView de Fechas
        /// </summary>
        private void ActualizarListaFechas()
        {
            this.dataGridView_Fecha.DataSource = typeof(List<RangoFecha>);
            this.dataGridView_Fecha.DataSource = this.iBanner.ListaRangosFecha;
            (this.dataGridView_Fecha.BindingContext[this.dataGridView_Fecha.DataSource] as CurrencyManager).Refresh();
            this.dataGridView_Fecha.Update();
            this.dataGridView_Fecha.Refresh();
        }

        /// <summary>
        /// Actualiza la lista de Rangos Horarios
        /// </summary>
        /// <param name="pRangoFechaSeleccionado">Rango de Fecha seleccionado para ver sus Rangos Horarios</param>
        private void ActualizarListaHorarios(RangoFecha pRangoFechaSeleccionado)
        {
            this.dataGridView_Hora.DataSource = typeof(List<RangoHorario>);
            this.dataGridView_Hora.DataSource = pRangoFechaSeleccionado.ListaRangosHorario;
            (this.dataGridView_Hora.BindingContext[this.dataGridView_Hora.DataSource] as CurrencyManager).Refresh();
            this.dataGridView_Hora.Refresh();
            this.dataGridView_Hora.Update();
        }

        /// <summary>
        /// Cambia color de fondo de los rangos de fecha a los que les falta agregar Rangos Horarios
        /// </summary>
        private void MarcarFilasIncompletas()
        {
            this.iRangosFechaCompletos = this.dataGridView_Fecha.RowCount > 0;
            foreach (DataGridViewRow fila in this.dataGridView_Fecha.Rows)
            {
                RangoFecha rangoFecha = (RangoFecha)fila.DataBoundItem;
                if(rangoFecha.ListaRangosHorario.Count > 0)
                {
                    fila.DefaultCellStyle.BackColor = Color.White;
                }
                else
                {
                    fila.DefaultCellStyle.BackColor = Color.Red;
                    this.iRangosFechaCompletos = false;
                }
            }
        }

        /// <summary>
        /// Verifica que no haya dos rangos de fecha con igual Fecha de Inicio y Fecha de Fin
        /// </summary>
        /// <param name="pFechaInicio">Fecha de Inicio del nuevo Rango Fecha a crear</param>
        /// <param name="pFechaFin">Fecha de Fin del nuevo Rango Fecha a crear</param>
        /// <returns>Tipo de dato booleano que representa si se puede crear el RF o no</returns>
        private bool VerificarRangoFecha(DateTime pFechaInicio, DateTime pFechaFin)
        {
            int indice = this.iBanner.ListaRangosFecha.FindIndex(pRangoFecha =>
                    (pRangoFecha.FechaInicio.CompareTo(pFechaInicio)==0) && (pRangoFecha.FechaFin.CompareTo(pFechaFin) == 0));
            return (indice == -1);
        }

        /// <summary>
        /// Actualiza la lista de Rangos Horarios a partir de los Seleccionados)
        /// </summary>
        /// <param name="pListaRangoHorario">Lista de rangos Horarios con la cual actualizar</param>
        public void ActualizarHorarios(List<RangoHorario> pListaRangoHorario)
        {
            this.RangoFechaSeleccionado().ListaRangosHorario = pListaRangoHorario;
            this.ActualizarListaHorarios(this.RangoFechaSeleccionado());
            this.MarcarFilasIncompletas();
            this.ActivarAceptar();
            if (this.iRangosFechaCompletos)
            {
                this.CampoCompleto(this.pictureBox_ComprobacionRH, true);
            }
        }

        /// <summary>
        /// Devuelve los RangosHorarios
        /// </summary>
        /// <param name="listaRangosFecha"></param>
        /// <returns></returns>
        private List<RangoHorario> ListaRangosHorariosRF()
        {
            List<RangoHorario> listaRangoHorarios = new List<RangoHorario>();
            RangoFecha pRangoFecha = this.RangoFechaSeleccionado();
            List<RangoFecha> listaResutlado = this.iBanner.ListaRangosFecha.FindAll(x =>
                            ((x.FechaInicio <= pRangoFecha.FechaInicio) && (x.FechaFin >= pRangoFecha.FechaInicio)) ||
                            ((x.FechaInicio <= pRangoFecha.FechaFin) && (x.FechaFin >= pRangoFecha.FechaFin)) ||
                            ((x.FechaInicio >= pRangoFecha.FechaInicio) && (x.FechaFin <= pRangoFecha.FechaFin)));
            foreach (RangoFecha rangoFecha in listaResutlado)
            {
                listaRangoHorarios.AddRange(rangoFecha.ListaRangosHorario);
            }
            foreach (RangoHorario rangoHorario in pRangoFecha.ListaRangosHorario)
            {
                listaRangoHorarios.Remove(rangoHorario);
            }
            return listaRangoHorarios;
        }
        #endregion
        #endregion

        #region Región: Procesos segundo Plano
        /// <summary>
        /// Evento que surge cuando el Proceso en segundo plano empieza a guardar el banner
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void backgroundWorker_BotonAceptar_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            this.iFuncionVentana(this.iBanner);
        }

        /// <summary>
        /// Evento que surge cuando el backgroundworker realacionado al botón Aceptar ha finalizado su trabajo
        /// </summary>
        /// <param name="sender">Objeto que  envía el evento</param>
        /// <param name="e">Argumentos del evento</param>
        private void backgroundWorker_BotonAceptar_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.Owner.Hide();
                this.Show();
                MessageBox.Show("Se encontró el siguiente problema al guardar /n" + e.Error.Message);
            }
            else
            {
                this.iCerrarCodigo = true;
                ((Form_Banner)this.Owner).EnEspera(true);
                MessageBox.Show("Los datos se han guardado correctamente");
                ((Form_Banner)this.Owner).ActualizarDGV();
                this.Close();
            }
        }
        #endregion
    }
}
