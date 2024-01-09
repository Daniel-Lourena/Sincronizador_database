using Construtor.BancoDados;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.IsisMtt;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Construtor
{
    internal partial class FormCarregar : Form
    {
        private List<string> comandoCriaTabelaBancoEmpresaDestino;
        private List<string> comandoCriaViewBancoEmpresaDestino;
        private List<string> comandoCriaColuna;
        private List<string> comandoAlteraColuna;
        private List<string> comandoCriaIndice;
        private List<string> tabelasCriar;

        private string acao = String.Empty;
        internal List<string> _comandosGeral = new List<string>();
        internal List<string> _realizarComandos = new List<string>();
        internal StringBuilder _stringBuilderErros;


        internal FormCarregar(string _acao)
        {
            comandoCriaTabelaBancoEmpresaDestino = new List<string>();
            comandoCriaViewBancoEmpresaDestino = new List<string>();
            comandoCriaColuna = new List<string>();
            comandoAlteraColuna = new List<string>();
            comandoCriaIndice = new List<string>();
            tabelasCriar = new List<string>();

            CarregarDados(_acao);
        }

        internal FormCarregar(string _acao,StringBuilder _pstringBuilderErros, List<string>_pcomandosGeral,List<string> _prealizarComandos)
        {
            _stringBuilderErros = _pstringBuilderErros;
            _comandosGeral = _pcomandosGeral;
            _realizarComandos = _prealizarComandos;
;
            CarregarDados(_acao, _realizarComandos.Count);
        }

        private void CarregarDados(string _acao,int tamanhoMaximoProgressBar = 100)
        {
            InitializeComponent();
            this.acao = _acao;

            this.progressBar1.Maximum = tamanhoMaximoProgressBar;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            switch(acao)
            {
                case "Verificar":
                    if (VerificarEstruturaCriacao().Result)
                        e.Result = true;
                        break;

                case "Modificar":
                    if (ModificarEstrutura().Result)
                        e.Result = true;
                        break;
            }

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            switch(acao)
            {
                case "Verificar":
                    _comandosGeral.AddRange(comandoCriaTabelaBancoEmpresaDestino);
                    _comandosGeral.AddRange(comandoCriaColuna);
                    _comandosGeral.AddRange(comandoAlteraColuna);
                    _comandosGeral.AddRange(comandoCriaIndice);
                    _comandosGeral.AddRange(comandoCriaViewBancoEmpresaDestino);
                    break;
            }

            this.Close();
        }


        private async Task<bool> VerificarEstruturaCriacao()
        {
            backgroundWorker1.ReportProgress(3,this.Text);

            //Mostra o nome da tabela e o tipo
            string mostraTabelas = "SHOW FULL TABLES;";


            //Conexão com as tabelas da EmpresaDestino
            MySqlConnection conexaoEmpresaDestino = new MySqlConnection(ConexoesEmpresas.STRCONEMPRESADESTINO);
            MySqlCommand comandoEmpresaDestino = new MySqlCommand(mostraTabelas, conexaoEmpresaDestino);


            //Conexão com as tabelas da EmpresaOrigem
            MySqlConnection conexaoEmpresaOrigem = new MySqlConnection(ConexoesEmpresas.STRCONEMPRESAORIGEM);
            MySqlCommand comandoEmpresaOrigem = new MySqlCommand(mostraTabelas, conexaoEmpresaOrigem);


            // Listagem das tabelas dos bancos
            List<string> tabelasBancoEmpresaDestino = new List<string>();
            List<string> tabelasBancoEmpresaOrigem = new List<string>();

            // Listagem das views dos bancos
            List<string> tabelasViewsEmpresaDestino = new List<string>();
            List<string> tabelasViewsEmpresaOrigem = new List<string>();


            // Listagem de indices das tabelas
            List<BancoDados.Index> indexesTabelaBancoEmpresaOrigem = new List<BancoDados.Index>();
            List<BancoDados.Index> indexesTabelaBancoEmpresaDestino = new List<BancoDados.Index>();
            List<BancoDados.Index> indexesNaoEncontradosBancoEmpresaDestino = new List<BancoDados.Index>();
            List<BancoDados.Index> indexesAjustarBancoEmpresaDestino = new List<BancoDados.Index>();
            List<BancoDados.Index> indexesNaoExistentesBancoEmpresaOrigem = new List<BancoDados.Index>();
            List<BancoDados.Index> primarysDiferentes = new List<BancoDados.Index>();


            // Listagem de Colunas das tabelas
            List<ColunaInfo> colunasBancoEmpresaDestino = new List<ColunaInfo>();
            List<ColunaInfo> colunasBancoEmpresaOrigem = new List<ColunaInfo>();


            // Variaveis para realizar a modificação na tabela existente
            bool alteraColuna = false;
            string tipo = String.Empty;
            string nulo = String.Empty;
            string padrao = String.Empty;
            string extra = String.Empty;


            // Inicialização dos objetos 
            string nomeTabelaEmpresaDestino = String.Empty;
            string nomeViewEmpresaDestino = String.Empty;
            string retornoComandoCriacaoTabela = String.Empty;
            string retornoComandoCriacaoView = String.Empty;


            ColunaInfo colunaBancoEmpresaDestino = null;
            int valorVarcharEmpresaDestino;
            int valorVarcharEmpresaOrigem;
            int valorIntEmpresaDestino;
            int valorIntEmpresaOrigem;


            using (conexaoEmpresaDestino)
            using (conexaoEmpresaOrigem)
            {
                conexaoEmpresaDestino.Open();
                conexaoEmpresaOrigem.Open();

                using (DbDataReader reader = await comandoEmpresaDestino.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        //Só adiciona tabelas simples na lista para verificação de colunas
                        if (!reader.IsDBNull(1))
                        {
                            switch (reader.GetFieldValue<string>(1))
                            {
                                case "VIEW": tabelasViewsEmpresaDestino.Add(reader.GetFieldValue<string>(0)); break;
                                case "BASE TABLE": tabelasBancoEmpresaDestino.Add(reader.GetFieldValue<string>(0)); break;
                            }
                        }
                    }
                }
                using (DbDataReader reader = await comandoEmpresaOrigem.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(1))
                        {
                            switch (reader.GetFieldValue<string>(1))
                            {
                                case "VIEW": tabelasViewsEmpresaOrigem.Add(reader.GetFieldValue<string>(0)); break;
                                case "BASE TABLE": tabelasBancoEmpresaOrigem.Add(reader.GetFieldValue<string>(0)); break;
                            }
                        }
                    }
                }

                backgroundWorker1.ReportProgress(10,this.Text);

                foreach (string nomeTabelaEmpresaOrigem in tabelasBancoEmpresaOrigem)
                {
                    //valida se a tabela existente no banco da EmpresaOrigem existe no banco da EmpresaDestino
                    //se não existir, irá ser criada
                    nomeTabelaEmpresaDestino = tabelasBancoEmpresaDestino.FirstOrDefault(nomeTabEmpresaDestino => nomeTabEmpresaDestino.Equals(nomeTabelaEmpresaOrigem));


                    if (String.IsNullOrEmpty(nomeTabelaEmpresaDestino))
                    {
                        comandoEmpresaOrigem.CommandText = $"SHOW CREATE TABLE `{nomeTabelaEmpresaOrigem}` ;";

                        using (MySqlDataReader reader = comandoEmpresaOrigem.ExecuteReader())
                        {
                            reader.Read();
                            retornoComandoCriacaoTabela = reader.GetValue(1).ToString();
                        }

                        //Retirados: ENGINE, AUTO_INCREMENT, CHARSET, COMMENT na criação da tabela
                        comandoCriaTabelaBancoEmpresaDestino.Add(retornoComandoCriacaoTabela.Substring(0, retornoComandoCriacaoTabela.IndexOf(" ENGINE=")));
                        tabelasCriar.Add(nomeTabelaEmpresaOrigem);
                    }
                    else
                    {

                        // Se a tabela que é existente na EmpresaOrigem for encontrada no banco da EmpresaDestino
                        comandoEmpresaOrigem.CommandText = $"SHOW COLUMNS FROM `{nomeTabelaEmpresaDestino}` ;";
                        using (DbDataReader reader = await comandoEmpresaOrigem.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                ColunaInfo coluna = new ColunaInfo();

                                int i = -1;
                                coluna.Nome = reader.GetValue(++i).ToString();
                                coluna.Tipo = reader.GetValue(++i).ToString();
                                coluna.Nulo = reader.GetValue(++i).ToString() == "YES" ? "NULL" : "NOT NULL";
                                coluna.Chave = reader.GetValue(++i).ToString();
                                coluna.Padrao = reader.GetValue(++i) == DBNull.Value ? null : reader.GetValue(i).ToString();
                                coluna.Extra = reader.GetValue(++i).ToString() == String.Empty ? String.Empty : reader.GetValue(i).ToString();

                                colunasBancoEmpresaOrigem.Add(coluna);
                            }
                        }

                        comandoEmpresaDestino.CommandText = $"SHOW COLUMNS FROM `{nomeTabelaEmpresaDestino}` ;";

                        using (DbDataReader reader = await comandoEmpresaDestino.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                ColunaInfo coluna = new ColunaInfo();

                                int i = -1;
                                coluna.Nome = reader.GetValue(++i).ToString();
                                coluna.Tipo = reader.GetValue(++i).ToString();
                                coluna.Nulo = reader.GetValue(++i).ToString() == "YES" ? "NULL" : "NOT NULL";
                                coluna.Chave = reader.GetValue(++i).ToString();
                                coluna.Padrao = reader.GetValue(++i) == DBNull.Value ? null : reader.GetValue(i).ToString();
                                coluna.Extra = reader.GetValue(++i).ToString() == String.Empty ? String.Empty : reader.GetValue(i).ToString(); ;

                                colunasBancoEmpresaDestino.Add(coluna);
                            }
                        }

                        foreach (ColunaInfo colunaEmpresaOrigem in colunasBancoEmpresaOrigem)
                        {
                            // Procura pelo nome da coluna na tabela da EmpresaDestino 
                            //Se não achar a coluna na tabela de acordo com a coluna no banco EmpresaOrigem
                            colunaBancoEmpresaDestino = colunasBancoEmpresaDestino.FirstOrDefault(colunaEmpresaDestino => colunaEmpresaDestino.Nome.ToLower().Equals(colunaEmpresaOrigem.Nome.ToLower()));

                            if (colunaBancoEmpresaDestino == null)
                            {
                                #region Coluna Não Encontrada

                                if (colunaEmpresaOrigem.Padrao == null)
                                    colunaEmpresaOrigem.Padrao = "NULL";

                                if (colunaEmpresaOrigem.Nulo.Equals("NULL") && (colunaEmpresaOrigem.Tipo.Contains("char") || colunaEmpresaOrigem.Tipo.Equals("text")))
                                {
                                    if (colunaEmpresaOrigem.Padrao.Equals("NULL"))
                                        comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` ADD COLUMN `{colunaEmpresaOrigem.Nome}` {colunaEmpresaOrigem.Tipo} {colunaEmpresaOrigem.Nulo} DEFAULT {colunaEmpresaOrigem.Padrao} ;";
                                    else
                                        comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` ADD COLUMN `{colunaEmpresaOrigem.Nome}` {colunaEmpresaOrigem.Tipo} {colunaEmpresaOrigem.Nulo} DEFAULT '{colunaEmpresaOrigem.Padrao}';";
                                }
                                else if (colunaEmpresaOrigem.Nulo.Equals("NULL"))
                                {
                                    comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` ADD COLUMN `{colunaEmpresaOrigem.Nome}` {colunaEmpresaOrigem.Tipo} {colunaEmpresaOrigem.Nulo} DEFAULT {colunaEmpresaOrigem.Padrao};";
                                }
                                else if (colunaEmpresaOrigem.Padrao == null && (colunaEmpresaOrigem.Tipo.Contains("char") || colunaEmpresaOrigem.Tipo.Equals("text")))
                                {
                                    comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` ADD COLUMN `{colunaEmpresaOrigem.Nome}` {colunaEmpresaOrigem.Tipo} {colunaEmpresaOrigem.Nulo} DEFAULT NULL ;";
                                }
                                else if (colunaEmpresaOrigem.Nulo.Equals("NOT NULL"))
                                {
                                    if (colunaEmpresaOrigem.Tipo.Contains("char") || colunaEmpresaOrigem.Tipo.Equals("text"))
                                    {
                                        if (colunaEmpresaOrigem.Padrao.Equals("NULL"))
                                            comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` ADD COLUMN `{colunaEmpresaOrigem.Nome}` {colunaEmpresaOrigem.Tipo} {colunaEmpresaOrigem.Nulo} ;";
                                        else
                                            comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` ADD COLUMN `{colunaEmpresaOrigem.Nome}` {colunaEmpresaOrigem.Tipo} {colunaEmpresaOrigem.Nulo} DEFAULT  '{colunaEmpresaOrigem.Padrao}' ;";
                                    }
                                    else if (colunaEmpresaOrigem.Nulo.Equals("NOT NULL") && colunaEmpresaOrigem.Padrao.Equals("NULL"))
                                    {
                                        comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` ADD COLUMN `{colunaEmpresaOrigem.Nome}` {colunaEmpresaOrigem.Tipo} {colunaEmpresaOrigem.Nulo} ;";
                                    }
                                    else
                                    {
                                        comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` ADD COLUMN `{colunaEmpresaOrigem.Nome}` {colunaEmpresaOrigem.Tipo} {colunaEmpresaOrigem.Nulo} DEFAULT {colunaEmpresaOrigem.Padrao} ;";
                                    }
                                }
                                else
                                {
                                    comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` ADD COLUMN `{colunaEmpresaOrigem.Nome}` {colunaEmpresaOrigem.Tipo} {colunaEmpresaOrigem.Nulo} ;";
                                }
                                comandoCriaColuna.Add(comandoEmpresaDestino.CommandText);
                                colunaBancoEmpresaDestino = colunaEmpresaOrigem;

                                #endregion
                            }

                            // Se encontrar a coluna no banco da EmpresaDestino
                            else
                            {
                                #region Verifica o Tipo de Dado e Tamanho

                                tipo = colunaBancoEmpresaDestino.Tipo;


                                if (!colunaBancoEmpresaDestino.Tipo.Equals(colunaEmpresaOrigem.Tipo))
                                {
                                    //verifica se o tamanho VARCHAR da EmpresaDestino é maior que o VARCHAR da EmpresaOrigem
                                    //se for, não irá alterar
                                    if (colunaEmpresaOrigem.Tipo.Contains("char"))
                                    {
                                        if (colunaBancoEmpresaDestino.Tipo.Contains("varchar"))
                                        {
                                            colunaBancoEmpresaDestino.Tipo = colunaBancoEmpresaDestino.Tipo.Replace("varchar(", String.Empty);
                                            valorVarcharEmpresaDestino = Convert.ToInt32(colunaBancoEmpresaDestino.Tipo.Remove(colunaBancoEmpresaDestino.Tipo.Length - 1, 1));

                                            colunaEmpresaOrigem.Tipo = colunaEmpresaOrigem.Tipo.Replace("varchar(", String.Empty);
                                            valorVarcharEmpresaOrigem = Convert.ToInt32(colunaEmpresaOrigem.Tipo.Remove(colunaEmpresaOrigem.Tipo.Length - 1, 1));

                                            if (valorVarcharEmpresaOrigem > valorVarcharEmpresaDestino)
                                            {
                                                tipo = $"varchar({valorVarcharEmpresaOrigem})";
                                                alteraColuna = true;
                                            }
                                        }
                                        else if (colunaBancoEmpresaDestino.Tipo.Equals("text") && colunaEmpresaOrigem.Tipo.Contains("varchar"))
                                        {
                                            tipo = colunaBancoEmpresaDestino.Tipo;
                                        }
                                        else if (colunaBancoEmpresaDestino.Tipo.Contains("varchar") && colunaEmpresaOrigem.Tipo.Contains("char") && !colunaEmpresaOrigem.Tipo.Contains("varchar"))
                                        {
                                            tipo = colunaBancoEmpresaDestino.Tipo;
                                        }
                                        else
                                        {
                                            tipo = colunaEmpresaOrigem.Tipo;
                                            alteraColuna = true;
                                        }
                                    }
                                    else if (colunaEmpresaOrigem.Tipo.Contains("int("))
                                    {
                                        if (colunaBancoEmpresaDestino.Tipo.Contains("int("))
                                        {
                                            colunaBancoEmpresaDestino.Tipo = colunaBancoEmpresaDestino.Tipo.Replace("int(", String.Empty);
                                            valorIntEmpresaDestino = Convert.ToInt32(colunaBancoEmpresaDestino.Tipo.Remove(colunaBancoEmpresaDestino.Tipo.Length - 1, 1));

                                            colunaEmpresaOrigem.Tipo = colunaEmpresaOrigem.Tipo.Replace("int(", String.Empty);
                                            valorIntEmpresaOrigem = Convert.ToInt32(colunaEmpresaOrigem.Tipo.Remove(colunaEmpresaOrigem.Tipo.Length - 1, 1));

                                            if (valorIntEmpresaOrigem > valorIntEmpresaDestino)
                                            {
                                                tipo = $"int({valorIntEmpresaOrigem})";
                                                alteraColuna = true;
                                            }
                                        }
                                        else if (!colunaBancoEmpresaDestino.Tipo.Contains("int"))
                                        {
                                            tipo = colunaEmpresaOrigem.Tipo;
                                            alteraColuna = true;
                                        }
                                    }
                                    else
                                    {
                                        tipo = colunaEmpresaOrigem.Tipo;
                                        alteraColuna = true;
                                    }
                                }
                                #endregion

                                #region Adiciona novos valores para a coluna
                                if (!colunaBancoEmpresaDestino.Nulo.Equals(colunaEmpresaOrigem.Nulo))
                                {
                                    nulo = colunaEmpresaOrigem.Nulo;
                                    alteraColuna = true;
                                }
                                else nulo = colunaBancoEmpresaDestino.Nulo;

                                if (!(colunaBancoEmpresaDestino.Padrao == colunaEmpresaOrigem.Padrao) && (tipo.Contains("char") || tipo.Contains("text") || tipo.Contains("time") || tipo.Contains("date")))
                                {
                                    if (colunaEmpresaOrigem.Padrao == null)
                                    {
                                        padrao = null;
                                    }
                                    else
                                    {
                                        padrao = $"'{colunaEmpresaOrigem.Padrao}'";
                                    }
                                    alteraColuna = true;
                                }
                                else if (!(colunaBancoEmpresaDestino.Padrao == colunaEmpresaOrigem.Padrao))
                                {
                                    padrao = colunaEmpresaOrigem.Padrao;
                                    alteraColuna = true;
                                }
                                else
                                {
                                    if (tipo.Contains("char") || tipo.Contains("text") || tipo.Contains("time") || tipo.Contains("date"))
                                    {
                                        padrao = $"'{colunaBancoEmpresaDestino.Padrao}'";
                                    }
                                    else
                                    {
                                        padrao = colunaBancoEmpresaDestino.Padrao;
                                    }
                                }


                                if (!colunaBancoEmpresaDestino.Extra.Equals(colunaEmpresaOrigem.Extra))
                                {
                                    extra = colunaEmpresaOrigem.Extra;
                                    alteraColuna = true;
                                }
                                else extra = colunaBancoEmpresaDestino.Extra;
                                #endregion

                            }


                            if (alteraColuna)
                            {
                                if (extra.Equals("auto_increment") && !colunaBancoEmpresaDestino.Chave.Equals("PRI"))
                                    extra += " PRIMARY KEY";

                                if (padrao == null)
                                    padrao = "NULL";

                                if ((tipo.Equals("text") || tipo.Equals("geometry") || tipo.Equals("blob") || tipo.Equals("json")))
                                    padrao = "NULL";

                                if (nulo.Equals("NULL"))
                                {
                                    comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` MODIFY COLUMN `{colunaBancoEmpresaDestino.Nome}` {tipo} {nulo} DEFAULT {padrao} {extra} ;";
                                }
                                else if (nulo.Equals("NOT NULL"))
                                {
                                    if (padrao == "NULL")
                                        comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` MODIFY COLUMN `{colunaBancoEmpresaDestino.Nome}` {tipo} {nulo} {extra} ;";

                                    else
                                        comandoEmpresaDestino.CommandText = $"ALTER TABLE `{nomeTabelaEmpresaDestino}` MODIFY COLUMN `{colunaBancoEmpresaDestino.Nome}` {tipo} {nulo} DEFAULT {padrao} {extra} ;";
                                }

                                comandoAlteraColuna.Add(comandoEmpresaDestino.CommandText);

                            }

                            tipo = String.Empty;
                            nulo = String.Empty;
                            padrao = String.Empty;
                            extra = String.Empty;
                            alteraColuna = false;
                        }

                        //Limpa as listas de colunas ao terminar de modificar cada tabela
                        colunasBancoEmpresaOrigem.Clear();
                        colunasBancoEmpresaDestino.Clear();
                    }
                }

                // Criação das views

                foreach (string viewNomeEmpresaOrigem in tabelasViewsEmpresaOrigem)
                {
                    //valida se a view existente no banco da EmpresaOrigem existe no banco da EmpresaDestino
                    //se não existir, irá ser criada
                    nomeViewEmpresaDestino = tabelasViewsEmpresaDestino.FirstOrDefault(nomeViewEmpresaDestino => nomeViewEmpresaDestino.Equals(viewNomeEmpresaOrigem));


                    if (String.IsNullOrEmpty(nomeViewEmpresaDestino))
                    {
                        comandoEmpresaOrigem.CommandText = $"SHOW CREATE VIEW `{viewNomeEmpresaOrigem}` ;";

                        using (MySqlDataReader reader = comandoEmpresaOrigem.ExecuteReader())
                        {
                            reader.Read();
                            retornoComandoCriacaoView = reader.GetValue(1).ToString();
                        }

                        comandoCriaViewBancoEmpresaDestino.Add(retornoComandoCriacaoView);
                    }
                }

                backgroundWorker1.ReportProgress(20, "Analisando as informações...");
                Thread.Sleep(1000);

                backgroundWorker1.ReportProgress(50, "Só mais um momento enquanto preparamos tudo...");

                foreach (string criacaoIndice in CriaIndices(comandoEmpresaOrigem, comandoEmpresaDestino, tabelasBancoEmpresaOrigem, indexesTabelaBancoEmpresaOrigem, indexesTabelaBancoEmpresaDestino, indexesNaoEncontradosBancoEmpresaDestino, indexesAjustarBancoEmpresaDestino, indexesNaoExistentesBancoEmpresaOrigem, primarysDiferentes))
                {
                    if (!criacaoIndice.Equals(String.Empty))
                    {
                        comandoCriaIndice.Add(criacaoIndice);
                    }
                }

                backgroundWorker1.ReportProgress(100, "Finalizando...");
                Thread.Sleep(1500);

                return true;
            }
        }

        private List<string> CriaIndices(MySqlCommand comandoEmpresaOrigem, MySqlCommand comandoEmpresaDestino, List<string> tabelasBancoEmpresaOrigem, List<BancoDados.Index> indexesTabelaBancoEmpresaOrigem, List<BancoDados.Index> indexesTabelaBancoEmpresaDestino, List<BancoDados.Index> indexesNaoEncontradosBancoEmpresaDestino, List<BancoDados.Index> indexesAjustarBancoEmpresaDestino, List<BancoDados.Index> indexesNaoExistentesBancoEmpresaOrigem, List<BancoDados.Index> primarysDiferentes)
        {
            StringBuilder alterTable = new StringBuilder();
            StringBuilder alterTablePrimary = new StringBuilder();
            StringBuilder dropIndex = new StringBuilder();
            StringBuilder indicesStringBuilderTabela = new StringBuilder();
            StringBuilder indicesStringBuilderPorNome = new StringBuilder();
            StringBuilder indiceColunasAgrupadas = new StringBuilder();
            StringBuilder indices = new StringBuilder();


            //Remove da verificação de indices existentes, todas as tabelas que ainda serão criadas no banco da EmpresaDestino
            foreach (string tabela in tabelasCriar)
            {
                tabelasBancoEmpresaOrigem.Remove(tabela);
            }

            foreach (string nometabelaEmpresaOrigem in tabelasBancoEmpresaOrigem)
            {
                //Procura pelos indices da tabela EmpresaOrigem
                comandoEmpresaOrigem.CommandText = $"SHOW INDEXES FROM `{nometabelaEmpresaOrigem}` ; ";

                using (MySqlDataReader reader = comandoEmpresaOrigem.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BancoDados.Index index = new BancoDados.Index();

                        if (!reader.IsDBNull(0)) index.Table = reader.GetValue(0).ToString();
                        if (!reader.IsDBNull(1)) index.NonUnique = reader.GetValue(1).ToString();
                        if (!reader.IsDBNull(2)) index.KeyName = reader.GetValue(2).ToString();
                        if (!reader.IsDBNull(3)) index.SeqInIndex = reader.GetValue(3).ToString();
                        if (!reader.IsDBNull(4)) index.CollumnName = reader.GetValue(4).ToString();
                        if (!reader.IsDBNull(5)) index.Collation = reader.GetValue(5).ToString() == "A" ? "ASC" : "DESC";

                        try
                        {
                            if (!reader.IsDBNull(13)) index.Visible = reader.GetValue(13).ToString();
                        }
                        catch
                        {
                            index.Visible = "YES";
                        }


                        indexesTabelaBancoEmpresaOrigem.Add(index);
                    }
                }


                //Procura pelos indices da tabela da EmpresaDestino
                comandoEmpresaDestino.CommandText = $"SHOW INDEXES FROM `{nometabelaEmpresaOrigem}` ; ";

                using (MySqlDataReader reader = comandoEmpresaDestino.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BancoDados.Index index = new BancoDados.Index();

                        if (!reader.IsDBNull(0)) index.Table = reader.GetValue(0).ToString();
                        if (!reader.IsDBNull(1)) index.NonUnique = reader.GetValue(1).ToString();
                        if (!reader.IsDBNull(2)) index.KeyName = reader.GetValue(2).ToString();
                        if (!reader.IsDBNull(3)) index.SeqInIndex = reader.GetValue(3).ToString();
                        if (!reader.IsDBNull(4)) index.CollumnName = reader.GetValue(4).ToString();
                        if (!reader.IsDBNull(5)) index.Collation = reader.GetValue(5).ToString() == "A" ? "ASC" : "DESC";

                        try
                        {
                            if (!reader.IsDBNull(13)) index.Visible = reader.GetValue(13).ToString();
                        }
                        catch
                        {
                            index.Visible = "YES";
                        }


                        indexesTabelaBancoEmpresaDestino.Add(index);
                    }
                }
            }

            //Verifica os indices que não existem na tabela da EmpresaDestino com relação a EmpresaOrigem
            //Agrupa os indices da EmpresaOrigem por tabela
            foreach (IGrouping<string, BancoDados.Index> idxAgrupadosPorTabelaEmpresaOrigem in indexesTabelaBancoEmpresaOrigem.GroupBy(nomeTabelaEmpresaOrigem => nomeTabelaEmpresaOrigem.Table))
            {
                //Vai trazer os indices agrupados da tabela EmpresaOrigem por nome
                foreach (IGrouping<string, BancoDados.Index> indicesAgrupadosPorNomeDistintoEmpresaOrigem in idxAgrupadosPorTabelaEmpresaOrigem.GroupBy(nomeIndiceEmpresaOrigem => nomeIndiceEmpresaOrigem.KeyName).Distinct())
                {
                    //Validação se o nome do indice é existente em algumas das tabelas da EmpresaDestino
                    //Se não existir, irá criar
                    if (indexesTabelaBancoEmpresaDestino.FirstOrDefault(nomeIndiceEmpresaDestino => nomeIndiceEmpresaDestino.KeyName.Equals(indicesAgrupadosPorNomeDistintoEmpresaOrigem.Key)) == null)
                    {
                        indexesNaoEncontradosBancoEmpresaDestino.Add(indicesAgrupadosPorNomeDistintoEmpresaOrigem.FirstOrDefault(nomeIndiceEmpresaOrigem => nomeIndiceEmpresaOrigem.KeyName.Equals(indicesAgrupadosPorNomeDistintoEmpresaOrigem.Key)));
                    }

                    //Caso encontre o índice em alguma das tabelas
                    else
                    {
                        //Agrupa os indices por tabela e depois por nome (EmpresaOrigem e EmpresaDestino)
                        List<BancoDados.Index>? indicesAgrupadosPorTabelaEmpresaDestino = indexesTabelaBancoEmpresaDestino.Where(nomeTabelaEmpresaDestino => nomeTabelaEmpresaDestino.Table.Equals(idxAgrupadosPorTabelaEmpresaOrigem.Key)).ToList();


                        //Verifica se todas as PK são iguais e ajusta caso necessário.
                        if (idxAgrupadosPorTabelaEmpresaOrigem.Count() == indexesTabelaBancoEmpresaDestino.Where(nomeTabelaEmpresaDestino => nomeTabelaEmpresaDestino.Table.Equals(idxAgrupadosPorTabelaEmpresaOrigem.Key)).Count())
                        {
                            foreach (var indicesAgrupadosPorPrimaryEmpresaOrigem in idxAgrupadosPorTabelaEmpresaOrigem.GroupBy(nomeIndiceEmpresaOrigem => nomeIndiceEmpresaOrigem.KeyName.Equals("PRIMARY")))
                            {
                                foreach (BancoDados.Index? indicePrimary in indicesAgrupadosPorPrimaryEmpresaOrigem)
                                {
                                    if (indicesAgrupadosPorTabelaEmpresaDestino.FirstOrDefault(nomeColuna => nomeColuna.CollumnName.Equals(indicePrimary.CollumnName)) == null)
                                    {
                                        primarysDiferentes.Add(indicePrimary);
                                    }
                                }
                            }
                        }

                        //Verifica se a quantidade de indices na tabela da EmpresaDestino é menor da quantidade dos indices EmpresaOrigem
                        //(AQUI PROCURA PELO TOTAL DE INDICES EM CADA TABELA)
                        if (idxAgrupadosPorTabelaEmpresaOrigem.Count() > indexesTabelaBancoEmpresaDestino.Where(nomeTabelaEmpresaDestino => nomeTabelaEmpresaDestino.Table.Equals(idxAgrupadosPorTabelaEmpresaOrigem.Key)).Count())
                        {
                            foreach (IGrouping<string, BancoDados.Index> indicesAgrupadosPorNomeEmpresaOrigem in idxAgrupadosPorTabelaEmpresaOrigem.GroupBy(nomeIndiceEmpresaOrigem => nomeIndiceEmpresaOrigem.KeyName))
                            {
                                //Verifica se a quantidade de indices na tabela da EmpresaDestino é diferente (AQUI VERIFICA A QUANTIDADE DE COLUNAS DE CADA INDICE)
                                //Baseando-se no nome do indice da tabela da EmpresaOrigem.
                                if (indicesAgrupadosPorNomeEmpresaOrigem.Count() != indicesAgrupadosPorTabelaEmpresaDestino.Where(nomeIndiceEmpresaDestino => nomeIndiceEmpresaDestino.KeyName.Equals(indicesAgrupadosPorNomeEmpresaOrigem.Key)).Count())
                                {
                                    //Adiciona o indice na lista para ser criado na tabela da EmpresaDestino
                                    foreach (BancoDados.Index? indiceAdicionar in indicesAgrupadosPorNomeEmpresaOrigem)
                                    {
                                        indexesAjustarBancoEmpresaDestino.Add(indiceAdicionar);
                                    }
                                }

                            }
                        }


                        //Irá validar se existem indices desnecessários existentes na tabela da EmpresaDestino.
                        //Se existir,a opção de exclui-los será informada.
                        if (idxAgrupadosPorTabelaEmpresaOrigem.Count() < indexesTabelaBancoEmpresaDestino.Where(nomeTabelaEmpresaDestino => nomeTabelaEmpresaDestino.Table.Equals(idxAgrupadosPorTabelaEmpresaOrigem.Key)).Count())
                        {
                            indexesNaoExistentesBancoEmpresaOrigem = new List<BancoDados.Index>(indicesAgrupadosPorTabelaEmpresaDestino);

                            List<BancoDados.Index>? indicesNaoExistentesBancoEmpresaOrigem = indicesAgrupadosPorTabelaEmpresaDestino.ToList();

                            foreach (IGrouping<string, BancoDados.Index> indicesAgrupadosPorNomeEmpresaOrigem in idxAgrupadosPorTabelaEmpresaOrigem.GroupBy(nomeIndiceEmpresaOrigem => nomeIndiceEmpresaOrigem.KeyName))
                            {
                                //Verifica se a quantidade de indices na tabela da EmpresaDestino é igual (AQUI VERIFICA A QUANTIDADE DE COLUNAS DE CADA INDICE)
                                if (indicesAgrupadosPorNomeEmpresaOrigem.Count() == indicesAgrupadosPorTabelaEmpresaDestino.Where(nomeIndiceEmpresaDestino => nomeIndiceEmpresaDestino.KeyName.Equals(indicesAgrupadosPorNomeEmpresaOrigem.Key)).Count())
                                {
                                    //Remove o indice da lista deixando apenas o que não for existente na tabela da EmpresaDestino
                                    //com relação a tabela da EmpresaOrigem
                                    foreach (BancoDados.Index? indiceRemover in indicesAgrupadosPorNomeEmpresaOrigem)
                                    {
                                        indexesNaoExistentesBancoEmpresaOrigem.Remove(indexesNaoExistentesBancoEmpresaOrigem.FirstOrDefault(x => x.KeyName.Equals(indiceRemover.KeyName)));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            indexesAjustarBancoEmpresaDestino.AddRange(indexesNaoEncontradosBancoEmpresaDestino);
            List<BancoDados.Index> nomesIndicesAgrupados = new List<BancoDados.Index>();
            string visible = String.Empty;


            #region Criação dos Índices Normais

            //Agrupar indices por tabela, por coluna e ordenar pela sequência de criação
            foreach (IGrouping<string, BancoDados.Index> indicesAgrupadosPorTabelaEmpresaDestino in indexesAjustarBancoEmpresaDestino.GroupBy(nomeTabelaEmpresaDestino => nomeTabelaEmpresaDestino.Table))
            {
                string nomeTabelaAtual = indicesAgrupadosPorTabelaEmpresaDestino.Key;

                //Agrupa todos os indices que NÃO são PRIMARY KEY
                foreach (IGrouping<string, BancoDados.Index> indicesAgrupadosPorNome in indicesAgrupadosPorTabelaEmpresaDestino.Where(nomeIndiceEmpresaDestino => !nomeIndiceEmpresaDestino.KeyName.Equals("PRIMARY")).GroupBy(nomeIndiceEmpresaDestino => nomeIndiceEmpresaDestino.KeyName).Distinct())
                {
                    //Adiciona as colunas referentes a cada indice
                    foreach (BancoDados.Index? idx in indicesAgrupadosPorNome.DistinctBy(nomeColunaEmpresaDestino => nomeColunaEmpresaDestino.CollumnName).OrderBy(ordemCriacaoIndice => ordemCriacaoIndice.SeqInIndex))
                    {
                        indices.Append($"`{idx.CollumnName}` {idx.Collation},");
                        visible = idx.Visible.Equals("YES") ? "VISIBLE" : "INVISIBLE";

                        nomesIndicesAgrupados.Add(idx);
                    }

                    //remove a virgula da adição de indices diferentes e adiciona a visibilidade
                    indiceColunasAgrupadas.Append($"( {indices.Remove(indices.Length - 1, 1)} ) {visible},");
                    indicesStringBuilderPorNome.Append($"ADD INDEX `{indicesAgrupadosPorNome.Key}` {indiceColunasAgrupadas}");

                    indices.Clear();
                    indiceColunasAgrupadas.Clear();
                }


                if (indicesStringBuilderPorNome.Length > 0)
                {
                    alterTable.Append($"ALTER TABLE `{nomeTabelaAtual}` {indicesStringBuilderPorNome.Remove(indicesStringBuilderPorNome.Length - 1, 1)} ;");
                }

                indicesStringBuilderPorNome.Clear();
                #endregion



                #region Criação dos Ìndices Primários

                //Agrupa todos os indices que são PRIMARY KEY
                if (indicesAgrupadosPorTabelaEmpresaDestino.Where(nomeIndiceEmpresaDestino => nomeIndiceEmpresaDestino.KeyName.Equals("PRIMARY")).Count() > 0)
                {
                    List<ColunaInfo> colunasBancoEmpresaDestino = new List<ColunaInfo>();
                    string indicesPrimarys = String.Empty;
                    string modifyColumn = String.Empty;
                    string dropPrimaryKey = String.Empty;

                    comandoEmpresaDestino.CommandText = $"SHOW COLUMNS FROM `{nomeTabelaAtual}` ;";

                    using (DbDataReader reader = comandoEmpresaDestino.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ColunaInfo coluna = new ColunaInfo();

                            coluna.Nome = reader.GetValue(0).ToString();
                            coluna.Tipo = reader.GetValue(1).ToString();
                            coluna.Chave = reader.GetValue(3).ToString();

                            colunasBancoEmpresaDestino.Add(coluna);
                        }
                    }

                    //Valida se existe uma chave primária para aquela tabela
                    //Se tiver, é necessário exclui-la para adicionar outra coluna como primária
                    if (colunasBancoEmpresaDestino.FirstOrDefault(keyIndice => keyIndice.Chave.Equals("PRI")) != null)
                        dropPrimaryKey = "DROP PRIMARY KEY,";

                    foreach (BancoDados.Index? indicePrimaryAdicionar in indicesAgrupadosPorTabelaEmpresaDestino.Where(nomeIndiceEmpresaDestino => nomeIndiceEmpresaDestino.KeyName.Equals("PRIMARY")).DistinctBy(colunaEmpresaDestino => colunaEmpresaDestino.CollumnName).OrderBy(ordemCriacaoIndice => ordemCriacaoIndice.SeqInIndex))
                    {
                        //Valida apenas as colunas que existem no banco da EmpresaDestino,
                        //para não tentar criar um índice de uma coluna inexistente.

                        BancoDados.ColunaInfo? colEmpresaDestinoObject = colunasBancoEmpresaDestino.FirstOrDefault(colunaEmpresaDestino => colunaEmpresaDestino.Nome.Equals(indicePrimaryAdicionar.CollumnName));
                        if (colEmpresaDestinoObject != null)
                        {
                            string dadoColunaAtualTipo = colEmpresaDestinoObject.Tipo;
                            modifyColumn += $"MODIFY COLUMN `{indicePrimaryAdicionar.CollumnName}` {dadoColunaAtualTipo} NOT NULL,";
                            indicesPrimarys += $"`{indicePrimaryAdicionar.CollumnName}`,";
                        }

                    }

                    if (!indicesPrimarys.Equals(String.Empty))
                        alterTablePrimary.Append($"ALTER TABLE `{nomeTabelaAtual}` {modifyColumn} {dropPrimaryKey} ADD PRIMARY KEY ( {indicesPrimarys.Remove(indicesPrimarys.Length - 1, 1)} );");
                }

                alterTable.Append(alterTablePrimary);

                alterTablePrimary.Clear();
            }


            //Exclui todos os índices normais para atualização da tabela
            foreach (BancoDados.Index? indice in nomesIndicesAgrupados.DistinctBy(nomeIndiceEmpresaDestino => nomeIndiceEmpresaDestino.KeyName))
            {
                if (indexesNaoEncontradosBancoEmpresaDestino.FirstOrDefault(indiceNaoEncontrado => indiceNaoEncontrado.Equals(indice)) == null)
                    dropIndex.Append($"ALTER TABLE `{indice.Table}` DROP INDEX `{indice.KeyName}`;");
            }


            //Remover índices desnecessários
            foreach (IGrouping<string, BancoDados.Index> indicesRemoverAgrupadosTabela in indexesNaoExistentesBancoEmpresaOrigem.GroupBy(tabela => tabela.Table))
            {
                foreach (BancoDados.Index? indexRemover in indicesRemoverAgrupadosTabela.DistinctBy(nome => nome.KeyName))
                {
                    if (indexRemover.KeyName == "PRIMARY")
                    {
                        dropIndex.Append($"ALTER TABLE `{indexRemover.Table}` DROP PRIMARY KEY;");
                    }
                    else
                    {
                        dropIndex.Append($"ALTER TABLE `{indexRemover.Table}` DROP INDEX `{indexRemover.KeyName}`;");
                    }
                }
            }

            //Ajusta as Primarys Keys que estiverem com colunas diferentes.
            foreach (BancoDados.Index? indexPrimary in primarysDiferentes.DistinctBy(tabela => tabela.Table))
            {
                dropIndex.Append($"ALTER TABLE `{indexPrimary.Table}` DROP PRIMARY KEY;");
            }

            dropIndex.Append(alterTable);
            #endregion


            return dropIndex.ToString().Split(";").ToList();
        }

        private (StringBuilder,List<string>) RealizarComandos()
        {
            return (_stringBuilderErros,_realizarComandos);
        }

        private async Task<bool> ModificarEstrutura()
        {
            using (MySqlConnection conexao = new MySqlConnection(ConexoesEmpresas.STRCONEMPRESADESTINO))
            {
                using (MySqlCommand comando = new MySqlCommand() { Connection = conexao })
                {
                    conexao.Open();

                    int progresso = 0;

                    foreach (string comandoExeTxt in _realizarComandos)
                    {
                        comando.CommandText = comandoExeTxt;

                        try
                        {
                            comando.ExecuteNonQuery();
                            _comandosGeral.Remove(comandoExeTxt);
                        }
                        catch (Exception ex)
                        {
                            _stringBuilderErros.AppendLine(ex.Message);
                        }

                        backgroundWorker1.ReportProgress(progresso++, "Modificando a estrutura, aguarde...");
                    }
                }
            }
            backgroundWorker1.ReportProgress(_realizarComandos.Count, "Finalizando...");

            Thread.Sleep(1500);

            return true;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
            this.Text = e.UserState.ToString();
        }
    }
}
