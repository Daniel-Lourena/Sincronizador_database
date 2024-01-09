using Construtor.BancoDados;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Construtor
{
    internal partial class Principal : Form
    {
        private bool validaSegundaRotina = true;
        internal List<string> comandosGeral = new List<string>();

        internal Principal()
        {
            InitializeComponent();
            InicializaGrid();
        }

        private void InicializaGrid()
        {
            DataGridViewCheckBoxColumn CheckboxColumn = new DataGridViewCheckBoxColumn();
            CheckboxColumn.TrueValue = true;
            CheckboxColumn.FalseValue = false;
            dgvComandos.Columns.Add(CheckboxColumn);
        }

        private void btnVerificaColunas_Click(object sender, EventArgs e)
        {
            LimparGrid();
            BloquearComponentes();

            FormCarregar _formCarregar = new FormCarregar("Verificar");
            _formCarregar.ShowDialog();

            comandosGeral = _formCarregar._comandosGeral;
            CarregarGrid();
        }

        private void BloquearComponentes()
        {
            btnVerficaColunas.Enabled = false;
            btnRealizarComando.Enabled = false;
            btnMarcarTodos.Enabled = false;
        }

        private void LimparGrid()
        {
            comandosGeral.Clear();
            LimparLinhasGrid();
        }

        private void CarregarGrid()
        {
            foreach (string comando in comandosGeral)
            {
                dgvComandos.Rows.Add(comando);
            }

            dgvComandos.Refresh();
            btnVerficaColunas.Enabled = true;
            btnRealizarComando.Enabled = true;

            if (dgvComandos.RowCount > 0)
            {
                btnMarcarTodos.Enabled = true;
                btnRealizarComando.Enabled = true;
                validaSegundaRotina = true;
            }
            else
            {
                validaSegundaRotina = false;
                btnRealizarComando.Enabled = false;
            }
        }

        private void ValidaSegundaRotina()
        {
            if (dgvComandos.RowCount == 0)
            {
                if (validaSegundaRotina)
                {
                    LimparGrid();
                    FormCarregar _formCarregar = new FormCarregar("Verificar");
                    _formCarregar.ShowDialog();
                    CarregarGrid();
                }
            }
        }

        private void btnRealizarComando_Click(object sender, EventArgs e)
        {
            List<string> comandosExecutarTxt = new List<string>();
            StringBuilder stringBuilderErros = new StringBuilder();

            if (dgvComandos.RowCount > 0)
            {
                foreach (DataGridViewRow row in dgvComandos.Rows)
                {
                    DataGridViewCheckBoxCell checkBox = (DataGridViewCheckBoxCell)row.Cells[1];

                    if (checkBox.Value == checkBox.TrueValue)
                    {
                        comandosExecutarTxt.Add(row.Cells[0].Value.ToString());
                    }
                }
            }

            if (comandosExecutarTxt.Count > 0)
            {
                FormCarregar _formCarregar = new FormCarregar("Modificar", stringBuilderErros, comandosGeral,comandosExecutarTxt);
                _formCarregar.ShowDialog();
                (stringBuilderErros, comandosGeral) = (_formCarregar._stringBuilderErros,_formCarregar._comandosGeral);

                if (stringBuilderErros.Length > 0)
                    MessageBox.Show(stringBuilderErros.ToString());

                LimparLinhasGrid();
            }
            ValidaSegundaRotina();
        }

        private void LimparLinhasGrid()
        {
            dgvComandos.Rows.Clear();
            foreach (string comando in comandosGeral)
            {
                dgvComandos.Rows.Add(comando);
            }
            dgvComandos.Refresh();
        }

        private void btnMarcarTodos_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvComandos.Rows)
            {
                DataGridViewCheckBoxCell checkBox = (DataGridViewCheckBoxCell)row.Cells[1];
                checkBox.Value = checkBox.TrueValue;
            }
            dgvComandos.Refresh();
        }

    }
}