namespace Construtor
{
    partial class Principal
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnVerficaColunas = new Button();
            dgvComandos = new DataGridView();
            comando = new DataGridViewTextBoxColumn();
            btnRealizarComando = new Button();
            btnMarcarTodos = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvComandos).BeginInit();
            SuspendLayout();
            // 
            // btnVerficaColunas
            // 
            btnVerficaColunas.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnVerficaColunas.Location = new Point(694, 429);
            btnVerficaColunas.Name = "btnVerficaColunas";
            btnVerficaColunas.Size = new Size(123, 38);
            btnVerficaColunas.TabIndex = 3;
            btnVerficaColunas.Text = "Verificar Estrutura";
            btnVerficaColunas.UseVisualStyleBackColor = true;
            btnVerficaColunas.Click += btnVerificaColunas_Click;
            // 
            // dgvComandos
            // 
            dgvComandos.AllowUserToAddRows = false;
            dgvComandos.AllowUserToDeleteRows = false;
            dgvComandos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvComandos.BackgroundColor = SystemColors.ControlLight;
            dgvComandos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvComandos.Columns.AddRange(new DataGridViewColumn[] { comando });
            dgvComandos.GridColor = SystemColors.Menu;
            dgvComandos.Location = new Point(12, 12);
            dgvComandos.Name = "dgvComandos";
            dgvComandos.RowTemplate.Height = 25;
            dgvComandos.Size = new Size(944, 396);
            dgvComandos.TabIndex = 4;
            // 
            // comando
            // 
            comando.DataPropertyName = "comando";
            comando.HeaderText = "Comando a ser realizado";
            comando.Name = "comando";
            comando.ReadOnly = true;
            comando.Resizable = DataGridViewTriState.True;
            comando.SortMode = DataGridViewColumnSortMode.NotSortable;
            comando.Width = 800;
            // 
            // btnRealizarComando
            // 
            btnRealizarComando.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnRealizarComando.Enabled = false;
            btnRealizarComando.Location = new Point(833, 429);
            btnRealizarComando.Name = "btnRealizarComando";
            btnRealizarComando.Size = new Size(123, 38);
            btnRealizarComando.TabIndex = 5;
            btnRealizarComando.Text = "Realizar Comandos";
            btnRealizarComando.UseVisualStyleBackColor = true;
            btnRealizarComando.Click += btnRealizarComando_Click;
            // 
            // btnMarcarTodos
            // 
            btnMarcarTodos.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnMarcarTodos.Enabled = false;
            btnMarcarTodos.Location = new Point(12, 429);
            btnMarcarTodos.Name = "btnMarcarTodos";
            btnMarcarTodos.Size = new Size(123, 38);
            btnMarcarTodos.TabIndex = 6;
            btnMarcarTodos.Text = "Marcar Todos";
            btnMarcarTodos.UseVisualStyleBackColor = true;
            btnMarcarTodos.Click += btnMarcarTodos_Click;
            // 
            // Principal
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.WindowFrame;
            ClientSize = new Size(971, 496);
            Controls.Add(btnMarcarTodos);
            Controls.Add(btnRealizarComando);
            Controls.Add(dgvComandos);
            Controls.Add(btnVerficaColunas);
            Name = "Principal";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Sincronizar Estrutura Banco de Dados";
            ((System.ComponentModel.ISupportInitialize)dgvComandos).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnVerficaColunas;
        private DataGridView dgvComandos;
        private Button btnRealizarComando;
        private Button btnMarcarTodos;
        private DataGridViewTextBoxColumn comando;
    }
}