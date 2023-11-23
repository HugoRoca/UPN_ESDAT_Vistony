﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using UPN_ESDAT_FINAL.BusinessLogic;
using UPN_ESDAT_FINAL.Common;
using UPN_ESDAT_FINAL.Model;

namespace UPN_ESDAT_FINAL
{
    public partial class FrmCrearProcesos : Form
    {
        BLArea _blArea = new BLArea();
        BLProceso _blProceso = new BLProceso();
        Utils _utils = new Utils();

        Common.Enum.AccionBoton accion = Common.Enum.AccionBoton.Nuevo;
        List<AreaModel> areas = new List<AreaModel>();
        List<EstadoProcesoModel> estados = new List<EstadoProcesoModel>();
        List<string> _ocultarColumnas = new List<string> { "Id", "IdArea" };
        List<string> _ordenColumnas = new List<string> { "Estado", "DescripcionCorta", "DescripcionLarga", "Documentos", "Area" };
        Dictionary<string, int> _tamanioColumnas = new Dictionary<string, int> {
            { "DescripcionCorta", 200 },
            { "DescripcionLarga", 400 }
        };

        public FrmCrearProcesos()
        {
            InitializeComponent();
        }

        private void FrmCrearProcesos_Load(object sender, EventArgs e)
        {
            CargarGrid();
            CargarCombo();

            TextboxAccion(false);
        }

        private List<ProcesoModel> ModelDatos(List<ProcesoModel> procesos)
        {
            foreach (var item in procesos)
            {
                item.Area = areas.Find(x => x.Id == item.IdArea)?.Descripcion ?? "";
            }

            return procesos;
        }

        private void TextboxAccion(bool valor, bool limpiar = true)
        {
            if (limpiar)
            {
                txtIdProceso.Clear();
                txtDescripcionCorta.Clear();
                txtDescripcionLarga.Clear();
                cbArea.SelectedIndex = 0;
                cbEstado.SelectedIndex = 0;
            }

            txtDescripcionCorta.Enabled = valor;
            txtDescripcionLarga.Enabled = valor;
            cbEstado.Enabled = valor;
            cbArea.Enabled = valor;
            btnSubir.Enabled = valor;
            btnVerPdf.Enabled = valor;

            txtDescripcionCorta.Focus();
        }

        private void CargarCombo()
        {
            CargaEstados();

            List<AreaModel> areas = _blArea.ObtenerTodos();

            areas.Insert(0, new AreaModel { Descripcion = "Seleccione una opción", Id = -1 });

            // Asignar la lista de items al ComboBox
            cbEstado.DataSource = estados;
            cbArea.DataSource = areas;

            // Configurar las propiedades DisplayMember y ValueMember
            cbEstado.DisplayMember = "Descripcion";
            cbEstado.ValueMember = "Id";

            cbArea.DisplayMember = "Descripcion";
            cbArea.ValueMember = "Id";
        }

        private void CargarGrid()
        {
            List<ProcesoModel> procesos = _blProceso.ObtenerTodos();
            areas = _blArea.ObtenerTodos();

            procesos = ModelDatos(procesos);

            _utils.CargarDatosEnGridView(dgvProceso, procesos, _ocultarColumnas, false, _tamanioColumnas, _ordenColumnas);
        }

        private void CargaEstados()
        {
            // Agregar el elemento predeterminado al inicio de la lista
            estados.Insert(0, new EstadoProcesoModel { Descripcion = "Seleccione una opción", Id = -1 });
            estados.Add(new EstadoProcesoModel { Descripcion = Constantes.EstadoProceso.Activo, Id = 1 });
            estados.Add(new EstadoProcesoModel { Descripcion = Constantes.EstadoProceso.EnPausa, Id = 2 });
            estados.Add(new EstadoProcesoModel { Descripcion = Constantes.EstadoProceso.Inhabilitar, Id = 3 });
            estados.Add(new EstadoProcesoModel { Descripcion = Constantes.EstadoProceso.Finalizado, Id = 4 });
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            btnVerPdf.Visible = false;
            if (btnNuevo.Text == "Nuevo")
            {
                int nuevoId = _blProceso.ContarRegistros();

                TextboxAccion(true);

                txtIdProceso.Text = _utils.GenerarId(nuevoId).ToString();

                accion = _utils.Botones(btnNuevo, btnGuardar, btnEliminar, Common.Enum.AccionBoton.Nuevo);
            }
            else
            {
                accion = _utils.Botones(btnNuevo, btnGuardar, btnEliminar, Common.Enum.AccionBoton.Default);
                TextboxAccion(false);
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (!_utils.ValidarCamposGroupBox(gbDatosProceso))
            {
                _utils.MostrarMensaje("Debe de completar todos los campos!", Common.Enum.TipoMensaje.Error);
                return;
            }

            if (accion == Common.Enum.AccionBoton.EditarEliminar)
            {
                TextboxAccion(true, false);
                accion = _utils.Botones(btnNuevo, btnGuardar, btnEliminar, Common.Enum.AccionBoton.Editar);
                return;
            }

            ProcesoModel procesoModel = new ProcesoModel();
            procesoModel.Id = int.Parse(txtIdProceso.Text);
            procesoModel.DescripcionCorta = txtDescripcionCorta.Text;
            procesoModel.DescripcionLarga = txtDescripcionLarga.Text;
            procesoModel.IdArea = (int)cbArea.SelectedValue;

            int estadoId = (int)cbEstado.SelectedValue;

            procesoModel.Estado = estados.Find(x => x.Id == estadoId)?.Descripcion ?? "";

            // Documento
            procesoModel.Documentos = _utils.CopiarArchivo(txtDocumento.Text, Common.Enum.Extension.PDF, procesoModel.Id, "Proceso");

            switch (accion)
            {
                case Common.Enum.AccionBoton.Nuevo:
                    _blProceso.InsertarRegistro(procesoModel);
                    break;
                case Common.Enum.AccionBoton.Editar:
                    _blProceso.ActualizarRegistro(procesoModel);
                    break;
                case Common.Enum.AccionBoton.EditarEliminar:
                    TextboxAccion(true, false);
                    accion = _utils.Botones(btnNuevo, btnGuardar, btnEliminar, Common.Enum.AccionBoton.Editar);
                    return;
                case Common.Enum.AccionBoton.Default:
                    break;
                default:
                    break;
            }

            _utils.MostrarMensaje("Datos registrados correctamente!", Common.Enum.TipoMensaje.Informativo);

            CargarGrid();

            accion = _utils.Botones(btnNuevo, btnGuardar, btnEliminar, Common.Enum.AccionBoton.Default);

            TextboxAccion(false);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            // Verificar si hay un valor en el cuadro de texto txtIdProceso
            if (!string.IsNullOrEmpty(txtIdProceso.Text))
            {
                if (!_utils.MostrarMensaje("¿Está seguro que desea eliminar le registro?", Common.Enum.TipoMensaje.YesNoCancel)) return;
                // Obtener los valores de las celdas en la fila seleccionada
                int Id = int.Parse(txtIdProceso.Text);

                _blProceso.EliminarRegistros(Id);

                CargarGrid();

                _utils.MostrarMensaje("Registro eliminado correctamente!", Common.Enum.TipoMensaje.Informativo);
            }
            else
            {
                _utils.MostrarMensaje("Seleccione un valor de la lista!", Common.Enum.TipoMensaje.Informativo);
            }
        }

        private void dgvProceso_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && accion == Common.Enum.AccionBoton.Default)
            {
                DataGridViewRow filaSeleccionada = dgvProceso.Rows[e.RowIndex];

                // Obtener los valores de las celdas en la fila seleccionada
                // Mostrar los valores en TextBox
                txtIdProceso.Text = filaSeleccionada.Cells["Id"].Value.ToString();
                txtDescripcionCorta.Text = filaSeleccionada.Cells["DescripcionCorta"].Value.ToString();
                txtDescripcionLarga.Text = filaSeleccionada.Cells["DescripcionLarga"].Value.ToString();
                cbArea.SelectedIndex = int.Parse(filaSeleccionada.Cells["IdArea"].Value.ToString());
                txtDocumento.Text = filaSeleccionada.Cells["Documentos"].Value.ToString();

                int estadoId = estados.Find(x => x.Descripcion == filaSeleccionada.Cells["Estado"].Value.ToString())?.Id ?? 0;
                cbEstado.SelectedIndex = estadoId;

                accion = _utils.Botones(btnNuevo, btnGuardar, btnEliminar, Common.Enum.AccionBoton.EditarEliminar);

                if (!string.IsNullOrEmpty(txtDocumento.Text))
                {
                    txtDocumento.Text = _utils.ObtenerRutaArchivo("Proceso", int.Parse(txtIdProceso.Text), Common.Enum.Extension.PDF);
                    btnVerPdf.Visible = true;
                }
            }
        }

        private void btnSubir_Click(object sender, EventArgs e)
        {
            // Configurar el diálogo para buscar archivos PDF
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Archivos PDF|*.pdf";
            openFileDialog.Title = "Seleccionar archivo PDF";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Mostrar la ruta seleccionada en el cuadro de texto
                txtDocumento.Text = openFileDialog.FileName;
                btnVerPdf.Visible = true;
            }
        }

        private void btnVerPdf_Click(object sender, EventArgs e)
        {
            FrmVerDocumentoPDF frmVerDocumentoPDF = new FrmVerDocumentoPDF(txtDocumento.Text);
            frmVerDocumentoPDF.ShowDialog();
        }
    }
}
