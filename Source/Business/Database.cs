using Habitasorte.Business.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Habitasorte.Business.Model.Publicacao;
using System.Data.SQLite;

namespace Habitasorte.Business
{
    public class Database
    {

        private static string ConnectionString { get; set; }

        private static SQLiteConnection Connection { get; set; }
        private static SQLiteTransaction Transaction { get; set; }

        string[] listaSimNao = { "SIM", "NÃO", "NAO" };
        public static void Initialize()
        {

            string dbFile = ConfigurationManager.AppSettings["ARQUIVO_BANCO"];
            string dbDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = $"{dbDirectory}{dbFile}";
            //Provider = Microsoft.ACE.OLEDB.12.0; Data Source = ExcelFilePath; Extended Properties = 'Excel 12.0;HRD=YES; IMEX=1;
            //ConnectionString = $"DataSource=\"{dbPath}\";Max Database Size=4091;Case Sensitive=False;Locale Identifier=1046;";

            SQLiteConnectionStringBuilder stringConexao = new SQLiteConnectionStringBuilder();
            stringConexao.DataSource = dbPath;
            stringConexao.Version = 3;
            stringConexao.DefaultTimeout = 120;

            ConnectionString = stringConexao.ConnectionString;

            SQLiteConnection engine = new SQLiteConnection(ConnectionString);
            Connection = engine.OpenAndReturn();
            string scriptFile = ConfigurationManager.AppSettings["ARQUIVO_SCRIPT"];
            string scriptPath = $"{dbDirectory}{scriptFile}";
            string scriptText;
            using (StreamReader streamReader = new StreamReader(scriptPath, Encoding.UTF8))
            {
                scriptText = streamReader.ReadToEnd();
            }
            using (SQLiteConnection connection = Connection)
            {
                foreach (string commandText in scriptText.Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(commandText))
                    {
                        using (SQLiteCommand command = new SQLiteCommand())
                        {
                            command.Connection = connection;
                            command.CommandType = CommandType.Text;
                            command.CommandText = commandText;
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
            }

            //if (!File.Exists(dbPath)) {
            //    using (SqlCeEngine engine = new SqlCeEngine(ConnectionString)) {
            //        engine.CreateDatabase();
            //    }
            //    string scriptFile = ConfigurationManager.AppSettings["ARQUIVO_SCRIPT"];
            //    string scriptPath = $"{dbDirectory}{scriptFile}";
            //    string scriptText;
            //    using (StreamReader streamReader = new StreamReader(scriptPath, Encoding.UTF8)) {
            //        scriptText = streamReader.ReadToEnd();
            //    }
            //    using (SqlCeConnection connection = CreateConnection()) {
            //        foreach (string commandText in scriptText.Split(';')) {
            //            if (!string.IsNullOrWhiteSpace(commandText)) {
            //                using (SqlCeCommand command = new SqlCeCommand()) {
            //                    command.Connection = connection;
            //                    command.CommandType = CommandType.Text;
            //                    command.CommandText = commandText;
            //                    command.ExecuteNonQuery();
            //                }
            //            }
            //        }
            //    }
            //}
        }

        public static void ExcluirBanco()
        {
            string dbFile = ConfigurationManager.AppSettings["ARQUIVO_BANCO"];
            string dbDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = $"{dbDirectory}{dbFile}";
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }

        public static SQLiteConnection CreateConnection()
        {
            SQLiteConnection connection = new SQLiteConnection(ConnectionString);
            return connection.OpenAndReturn();
        }

        public Database(SQLiteConnection connection, SQLiteTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        private SQLiteCommand CreateCommand(string commandText, params SQLiteParameter[] parameters)
        {
            SQLiteCommand command = new SQLiteCommand
            {
                Connection = Connection,
                Transaction = Transaction,
                CommandType = CommandType.Text,
                CommandText = commandText
            };
            command.Parameters.AddRange(parameters);
            return command;
        }

        private void ExecuteNonQuery(string commandText, params SQLiteParameter[] parameters)
        {
            using (SQLiteCommand command = CreateCommand(commandText, parameters))
            {
                command.ExecuteNonQuery();
            }
        }

        private int ExecuteScalar<T>(string commandText, params SQLiteParameter[] parameters)
        {
            using (SQLiteCommand command = CreateCommand(commandText, parameters))
            {
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private string CarregarParametro(string nome)
        {
            using (SQLiteCommand command = CreateCommand("SELECT VALOR FROM PARAMETRO WHERE NOME = @NOME"))
            {
                command.Parameters.AddWithValue("@NOME", nome);
                return command.ExecuteScalar() as string;
            }
        }

        private void AtualizarParametro(string nome, string valor)
        {
            ExecuteNonQuery(
                "UPDATE PARAMETRO SET VALOR = @VALOR WHERE NOME = @NOME",
                new SQLiteParameter("NOME", nome),
                new SQLiteParameter("VALOR", (object)valor ?? DBNull.Value)
            );
        }

        /* Ações */

        public Sorteio CarregarSorteio()
        {

            string faixaB = CarregarParametro("FAIXA_B");
            string faixaC = CarregarParametro("FAIXA_C");
            string faixaD = CarregarParametro("FAIXA_D");
            string empreendimento1 = CarregarParametro("EMPREENDIMENTO_1");
            string empreendimento2 = CarregarParametro("EMPREENDIMENTO_2");
            string empreendimento3 = CarregarParametro("EMPREENDIMENTO_3");
            string empreendimento4 = CarregarParametro("EMPREENDIMENTO_4");
            string empreendimento5 = CarregarParametro("EMPREENDIMENTO_5");
            string empreendimento6 = CarregarParametro("EMPREENDIMENTO_6");

            return new Sorteio
            {
                Nome = CarregarParametro("NOME_SORTEIO"),
                StatusSorteio = CarregarParametro("STATUS_SORTEIO"),
                FaixaA = CarregarParametro("FAIXA_A"),
                FaixaAAtivo = true,
                FaixaB = faixaB,
                FaixaBAtivo = !String.IsNullOrWhiteSpace(faixaB),
                FaixaC = faixaC,
                FaixaCAtivo = !String.IsNullOrWhiteSpace(faixaC),
                FaixaD = faixaD,
                FaixaDAtivo = !String.IsNullOrWhiteSpace(faixaD),
                Empreendimento1 = empreendimento1,
                Empreendimento1Ativo = !String.IsNullOrWhiteSpace(empreendimento1),
                Empreendimento2 = empreendimento2,
                Empreendimento2Ativo = !String.IsNullOrWhiteSpace(empreendimento2),
                Empreendimento3 = empreendimento3,
                Empreendimento3Ativo = !String.IsNullOrWhiteSpace(empreendimento3),
                Empreendimento4 = empreendimento4,
                Empreendimento4Ativo = !String.IsNullOrWhiteSpace(empreendimento4),
                Empreendimento5 = empreendimento5,
                Empreendimento5Ativo = !String.IsNullOrWhiteSpace(empreendimento5),
                Empreendimento6 = empreendimento6,
                Empreendimento6Ativo = !String.IsNullOrWhiteSpace(empreendimento6)
            };
        }

        public void AtualizarSorteio(Sorteio sorteio)
        {
            string nomeSorteio = String.Concat("Programa Habitacional Casa Nova - SORTEIO ", DateTime.Now.ToString("MMMM/yyyy"));
            AtualizarParametro("NOME_SORTEIO", nomeSorteio);
            AtualizarParametro("FAIXA_A", sorteio.FaixaA);
            AtualizarParametro("FAIXA_B", sorteio.FaixaAAtivo ? sorteio.FaixaB : null);
            AtualizarParametro("FAIXA_C", sorteio.FaixaBAtivo ? sorteio.FaixaC : null);
            AtualizarParametro("FAIXA_D", sorteio.FaixaCAtivo ? sorteio.FaixaD : null);
            AtualizarParametro("EMPREENDIMENTO_1", sorteio.Empreendimento1);
            AtualizarParametro("EMPREENDIMENTO_2", sorteio.Empreendimento2Ativo ? sorteio.Empreendimento2 : null);
            AtualizarParametro("EMPREENDIMENTO_3", sorteio.Empreendimento3Ativo ? sorteio.Empreendimento3 : null);
            AtualizarParametro("EMPREENDIMENTO_4", sorteio.Empreendimento4Ativo ? sorteio.Empreendimento4 : null);
            AtualizarParametro("EMPREENDIMENTO_5", sorteio.Empreendimento5Ativo ? sorteio.Empreendimento5 : null);
            AtualizarParametro("EMPREENDIMENTO_6", sorteio.Empreendimento6Ativo ? sorteio.Empreendimento6 : null);
        }

        public void AtualizarStatusSorteio(string status)
        {
            AtualizarParametro("STATUS_SORTEIO", status);
        }

        public ICollection<Lista> CarregarListas()
        {
            List<Lista> listas = new List<Lista>();
            using (SQLiteCommand command = CreateCommand($"SELECT * FROM LISTA ORDER BY ORDEM_SORTEIO"))
            {
                using (SQLiteDataReader resultSet = command.ExecuteReader())
                {
                    
                    while (resultSet.Read())
                    {
                        listas.Add(new Lista
                        {
                            IdLista = Convert.ToInt32(resultSet[resultSet.GetOrdinal("ID_LISTA")]),
                            OrdemSorteio = Convert.ToInt32(resultSet[resultSet.GetOrdinal("ORDEM_SORTEIO")]),
                            Nome = resultSet.GetString(resultSet.GetOrdinal("NOME")),
                            Quantidade = Convert.ToInt32(resultSet[resultSet.GetOrdinal("QUANTIDADE")]),
                            Sorteada = Convert.ToBoolean(resultSet[resultSet.GetOrdinal("SORTEADA")]),
                            Publicada = Convert.ToBoolean(resultSet[resultSet.GetOrdinal("PUBLICADA")])
                        });
                    }
                }
            }
            return listas;
        }

        public Lista CarregarProximaLista()
        {
            using (SQLiteCommand command = CreateCommand("SELECT * FROM LISTA WHERE SORTEADA = 0 ORDER BY ORDEM_SORTEIO"))
            {
                using (SQLiteDataReader resultSet = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (resultSet.Read())
                    {
                        int idLista = Convert.ToInt32(resultSet[resultSet.GetOrdinal("ID_LISTA")]);
                        return new Lista
                        {
                            IdLista = idLista,
                            OrdemSorteio = Convert.ToInt32(resultSet[resultSet.GetOrdinal("ORDEM_SORTEIO")]),
                            Nome = resultSet.GetString(resultSet.GetOrdinal("NOME")),
                            Quantidade = Convert.ToInt32(resultSet[resultSet.GetOrdinal("QUANTIDADE")]),
                            CandidatosDisponiveis = CandidatosDisponiveisLista(idLista)
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public int ContagemCandidatos()
        {
            return ExecuteScalar<int>("SELECT COUNT(*) FROM CANDIDATO");
        }

        private int CandidatosDisponiveisLista(int idLista)
        {
            return ExecuteScalar<int>($"SELECT COUNT(*) FROM CANDIDATO_LISTA INNER JOIN CANDIDATO ON CANDIDATO_LISTA.ID_CANDIDATO = CANDIDATO.ID_CANDIDATO WHERE CANDIDATO.CONTEMPLADO = 0 AND CANDIDATO_LISTA.ID_LISTA = {idLista}");
        }

        private string AnonimizarInicioCpf(object valor)
        {
            string[] parte = valor.ToString().Split('.');

            int tamanhoTrecho = parte[0].Length > 3 ? 3 : parte[0].Length;
            bool inicioAnonimo = true;

            for (int ix = 0; ix < tamanhoTrecho; ix++)
            {
                if (parte[0][ix] >= '0' && parte[0][ix] <= '9')
                {
                    inicioAnonimo = false;
                    break;
                }
            }

            if (!inicioAnonimo)
            {
                if (parte.Length == 1)
                {
                    parte[0] = String.Concat("XXX", parte[0].Substring(3));
                }
                else
                {
                    parte[0] = "XXX";
                }
            }

            return String.Join(".", parte);
        }

        public void AtualizarListas(ICollection<Lista> listas)
        {
            foreach (Lista lista in listas)
            {
                ExecuteNonQuery(
                    "UPDATE LISTA SET QUANTIDADE = @QUANTIDADE WHERE ID_LISTA = @ID_LISTA",
                    new SQLiteParameter("ID_LISTA", lista.IdLista),
                    new SQLiteParameter("QUANTIDADE", lista.Quantidade)
                );
            }
        }

        private bool ConverterBooleano(object valor)
        {
            if (listaSimNao.Contains(valor.ToString().ToUpper()))
            {
                return valor.ToString().ToUpper() == "SIM";
            }
            else
            {
                return Convert.ToBoolean(valor);
            }
        }

        public void CopiarCandidatosArquivo(string faixa, IDataReader dataReader, Action<string> updateStatus, Action<int> updateProgress)
        {
            string inicio = "Iniciando importação ";
            updateStatus(inicio + faixa);

            /* Copia os candidatos da lista de importação. */

            updateStatus("Importando candidatos " + faixa);

            SQLiteCommand command = CreateCommand("SELECT ID_CANDIDATO FROM CANDIDATO ORDER BY ID_CANDIDATO DESC");

            SQLiteDataReader resultSet = command.ExecuteReader(CommandBehavior.SingleRow);

            int idCandidato = 0;

            if (resultSet.Read())
            {
                idCandidato = Convert.ToInt32(resultSet[0]);
            }

            while (dataReader.Read())
            {
                idCandidato++;

                string cpf = AnonimizarInicioCpf(dataReader[dataReader.GetOrdinal("CPF DO RESPONSÁVEL")]);
                string nome = dataReader.GetString(dataReader.GetOrdinal("NOME DO RESPONSÁVEL"));
                int qtdCriterios = Convert.ToInt32(dataReader[dataReader.GetOrdinal("TOTAL DE CRITÉRIOS")]);
                bool deficiente = ConverterBooleano(dataReader[dataReader.GetOrdinal("DEFICIENTE")]);
                bool idoso = ConverterBooleano(dataReader[dataReader.GetOrdinal("RESPONSÁVEL E/OU  CÔNJUGE IDOSO (60+)")]);
                bool superIdoso = ConverterBooleano(dataReader[dataReader.GetOrdinal("RESPONSÁVEL E OU CÔNJUGE COM 80 ANOS OU MAIS")]);
                int inscricao = Convert.ToInt32(dataReader[dataReader.GetOrdinal("NÚMERO DA INSCRIÇÃO")]);
                int rendaBruta = Convert.ToInt32(dataReader[dataReader.GetOrdinal("FAIXA SALARIAL")]);

                ExecuteNonQuery($"INSERT INTO CANDIDATO (ID_CANDIDATO, CPF, NOME, LISTA_DEFICIENTES, LISTA_IDOSOS, LISTA_SUPER_IDOSOS, INSCRICAO, RENDA_BRUTA, QUANTIDADE_CRITERIOS) VALUES (@ID_CANDIDATO, @CPF, @NOME, @DEFICIENTE, @IDOSO, @SUPER_IDOSO, @INSCRICAO, @RENDA, @CRITERIOS)",
                    new SQLiteParameter("ID_CANDIDATO", idCandidato) { DbType = DbType.Int32 },
                    new SQLiteParameter("CPF", cpf) { DbType = DbType.String },
                    new SQLiteParameter("NOME", nome) { DbType = DbType.String },
                    new SQLiteParameter("DEFICIENTE", deficiente) { DbType = DbType.Boolean },
                    new SQLiteParameter("IDOSO", idoso) { DbType = DbType.Boolean },
                    new SQLiteParameter("SUPER_IDOSO", superIdoso) { DbType = DbType.Boolean },
                    new SQLiteParameter("INSCRICAO", inscricao) { DbType = DbType.Int32 },
                    new SQLiteParameter("RENDA", rendaBruta) { DbType = DbType.Int32 },
                    new SQLiteParameter("CRITERIOS", qtdCriterios) { DbType = DbType.Int32 }
                );
            }

            resultSet.Close();
            command.Dispose();

            //SqlCeBulkCopy bulkCopy = new SqlCeBulkCopy(Connection, Transaction);
            //bulkCopy.DestinationTableName = "CANDIDATO";
            //bulkCopy.ColumnMappings.Add(new SqlCeBulkCopyColumnMapping(0, "CPF"));
            //bulkCopy.ColumnMappings.Add(new SqlCeBulkCopyColumnMapping(1, "NOME"));
            //bulkCopy.ColumnMappings.Add(new SqlCeBulkCopyColumnMapping(2, "QUANTIDADE_CRITERIOS"));
            //bulkCopy.ColumnMappings.Add(new SqlCeBulkCopyColumnMapping(3, "LISTA_DEFICIENTES"));
            //bulkCopy.ColumnMappings.Add(new SqlCeBulkCopyColumnMapping(4, "LISTA_IDOSOS"));
            //bulkCopy.ColumnMappings.Add(new SqlCeBulkCopyColumnMapping(5, "LISTA_SUPER_IDOSOS"));
            //bulkCopy.ColumnMappings.Add(new SqlCeBulkCopyColumnMapping(6, "INSCRICAO"));
            //bulkCopy.ColumnMappings.Add(new SqlCeBulkCopyColumnMapping(7, "RENDA_BRUTA"));
            //bulkCopy.WriteToServer(dataReader);
        }

        public void CriarListasSorteioPorFaixa(string faixa, Action<string> updateStatus, Action<int> updateProgress, int listaAtual, int totalListas, int incrementoOrdem, int rendaMinima, int rendaMaxima)
        {

            /* Gera as listas de sorteio por grupo e faixa de renda. */
            //int incrementoOrdem = listaAtual;
            int idPrimeiraLista = listaAtual;
            int idUltimaLista;

            updateStatus($"Gerando lista {listaAtual} de {totalListas}.");
            updateProgress((int)((++listaAtual / (double)totalListas) * 100));

            idUltimaLista = CriarListaSorteioPorGrupoFaixa(faixa, "Idosos", listaAtual, idPrimeiraLista);
            ExecuteNonQuery($"INSERT INTO CANDIDATO_LISTA(ID_LISTA, ID_CANDIDATO) SELECT {idUltimaLista}, ID_CANDIDATO FROM CANDIDATO WHERE LISTA_IDOSOS = 1 AND RENDA_BRUTA >= {rendaMinima} AND RENDA_BRUTA <= {rendaMaxima}");
            ClassificarListaSorteioIdosos(idUltimaLista);

            updateStatus($"Gerando lista {++listaAtual} de {totalListas}.");
            updateProgress((int)((++listaAtual / (double)totalListas) * 100));

            idUltimaLista = CriarListaSorteioPorGrupoFaixa(faixa, "Deficientes", listaAtual, idPrimeiraLista + incrementoOrdem);
            ExecuteNonQuery($"INSERT INTO CANDIDATO_LISTA(ID_LISTA, ID_CANDIDATO) SELECT {idUltimaLista}, ID_CANDIDATO FROM CANDIDATO WHERE LISTA_DEFICIENTES = 1 AND RENDA_BRUTA >= {rendaMinima} AND RENDA_BRUTA <= {rendaMaxima}");
            ClassificarListaSorteioSimples(idUltimaLista);

            updateStatus($"Gerando lista {++listaAtual} de {totalListas}.");
            updateProgress((int)((++listaAtual / (double)totalListas) * 100));

            idUltimaLista = CriarListaSorteioPorGrupoFaixa(faixa, "Geral", listaAtual, idPrimeiraLista + (2 * incrementoOrdem));
            ExecuteNonQuery($"INSERT INTO CANDIDATO_LISTA(ID_LISTA, ID_CANDIDATO) SELECT {idUltimaLista}, ID_CANDIDATO FROM CANDIDATO WHERE RENDA_BRUTA >= {rendaMinima} AND RENDA_BRUTA <= {rendaMaxima}");
            ClassificarListaSorteioSimples(idUltimaLista);
        }

        private int CriarListaSorteioPorGrupoFaixa(string grupoFaixa, string nomeLista, int id, int incremento)
        {
            ExecuteNonQuery(
                $"INSERT INTO LISTA(ID_LISTA, NOME, ORDEM_SORTEIO, QUANTIDADE, SORTEADA, PUBLICADA) VALUES(@IDLISTA, @GRUPOFAIXA, @INCREMENTO_ORDEM, 1, 0, 0);",
                new SQLiteParameter("IDLISTA", id) { DbType = DbType.Int32 },
                new SQLiteParameter("GRUPOFAIXA", String.Concat(nomeLista, " - ", grupoFaixa)) { DbType = DbType.String },
                new SQLiteParameter("INCREMENTO_ORDEM", incremento) { DbType = DbType.Int32 }
            );
            return incremento;
        }

        private int CriarListaSorteio(string empreendimento, string nomeLista, int fatorLista, int incremento, int qtdEmpreendimentos)
        {
            ExecuteNonQuery(
                $"INSERT INTO LISTA(NOME, ORDEM_SORTEIO, QUANTIDADE, SORTEADA, PUBLICADA) VALUES(@EMPREENDIMENTO + ' - {nomeLista}', {fatorLista} * @QTD_EMPREENDIMENTOS + @INCREMENTO_ORDEM, 1, 0, 0);",
                new SQLiteParameter("EMPREENDIMENTO", empreendimento) { DbType = DbType.String },
                new SQLiteParameter("QTD_EMPREENDIMENTOS", qtdEmpreendimentos),
                new SQLiteParameter("INCREMENTO_ORDEM", incremento)
            );
            return (int)ExecuteScalar<decimal>("SELECT @@IDENTITY");
        }

        private void ClassificarListaSorteioIdosos(int idUltimaLista)
        {
            ClassificarListaSorteio(idUltimaLista, "IDOSOS");
        }

        private void ClassificarListaSorteioSimples(int idUltimaLista)
        {
            ClassificarListaSorteio(idUltimaLista, "SIMPLES");
        }

        private void ClassificarListaSorteioComposto(int idUltimaLista)
        {
            ClassificarListaSorteio(idUltimaLista, "COMPOSTO");
        }

        private void ClassificarListaSorteioConstante(int idUltimaLista)
        {
            ClassificarListaSorteio(idUltimaLista, "CONSTANTE");
        }

        private void ClassificarListaSorteio(int idUltimaLista, string tipoOrdenacao)
        {

            List<CandidatoGrupo> candidatosLista = new List<CandidatoGrupo>();

            using (SQLiteCommand command = CreateCommand("SELECT * FROM CANDIDATO_LISTA INNER JOIN CANDIDATO ON CANDIDATO_LISTA.ID_CANDIDATO = CANDIDATO.ID_CANDIDATO WHERE CANDIDATO_LISTA.ID_LISTA = @ID_LISTA"))
            {
                command.Parameters.AddWithValue("ID_LISTA", idUltimaLista);
                using (SQLiteDataReader resultSet = command.ExecuteReader())
                {
                    while (resultSet.Read())
                    {
                        candidatosLista.Add(new CandidatoGrupo
                        {
                            IdCandidato = Convert.ToInt32(resultSet[resultSet.GetOrdinal("ID_CANDIDATO")]),
                            Cpf = resultSet.GetString(resultSet.GetOrdinal("CPF")),
                            Nome = resultSet.GetString(resultSet.GetOrdinal("NOME")).ToUpper().TrimEnd(),
                            QuantidadeCriterios = Convert.ToInt32(resultSet[resultSet.GetOrdinal("QUANTIDADE_CRITERIOS")]),
                            SuperIdoso = Convert.ToBoolean(resultSet[resultSet.GetOrdinal("LISTA_SUPER_IDOSOS")])
                        });
                    }
                }
            }

            CandidatoGrupo[] candidatosOrdenados;

            if (tipoOrdenacao == "IDOSOS")
            {
                candidatosOrdenados = candidatosLista
                    .OrderByDescending(c => c.QuantidadeCriterios)
                    .ThenByDescending(c => c.SuperIdoso)
                    .ThenBy(c => c.Nome)
                    .ThenByDescending(c => c.IdInscricao)
                    .ToArray();
            }

            else if (tipoOrdenacao == "SIMPLES")
            {
                candidatosOrdenados = candidatosLista
                    .OrderByDescending(c => c.QuantidadeCriterios)
                    .ThenBy(c => c.Nome)
                    .ThenByDescending(c => c.IdInscricao)
                    .ToArray();
            }

            else if (tipoOrdenacao == "COMPOSTO")
            {
                candidatosOrdenados = candidatosLista
                    .OrderByDescending(c => c.QuantidadeCriteriosComposta)
                    .ThenBy(c => c.Nome)
                    .ThenByDescending(c => c.IdInscricao)
                    .ToArray();
            }

            else
            {
                candidatosOrdenados = candidatosLista
                    .OrderBy(c => c.Nome)
                    .ThenByDescending(c => c.IdInscricao)
                    .ToArray();
            }

            CandidatoGrupo candidatoAnterior = null;
            int sequencia = 1;
            int classificacao = 1;

            SQLiteCommand updateCommand = CreateCommand(
                "UPDATE CANDIDATO_LISTA SET SEQUENCIA = @SEQUENCIA, CLASSIFICACAO = @CLASSIFICACAO WHERE ID_LISTA = @ID_LISTA AND ID_CANDIDATO = @ID_CANDIDATO",
                new SQLiteParameter("SEQUENCIA", -1) { DbType = DbType.Int32 },
                new SQLiteParameter("CLASSIFICACAO", -1) { DbType = DbType.Int32 },
                new SQLiteParameter("ID_LISTA", idUltimaLista) { DbType = DbType.Int32 },
                new SQLiteParameter("ID_CANDIDATO", -1) { DbType = DbType.Int32 }
            );
            updateCommand.Prepare();

            foreach (CandidatoGrupo candidato in candidatosOrdenados)
            {

                if (candidatoAnterior != null)
                {
                    if (tipoOrdenacao == "IDOSOS")
                    {
                        if (candidato.SuperIdoso != candidatoAnterior.SuperIdoso)
                        {
                            classificacao++;
                        }
                        if (candidato.QuantidadeCriterios != candidatoAnterior.QuantidadeCriterios)
                        {
                            classificacao++;
                        }
                    }
                    else
                    if (tipoOrdenacao == "SIMPLES" && candidato.QuantidadeCriterios != candidatoAnterior.QuantidadeCriterios)
                    {
                        classificacao++;
                    }
                    else if (tipoOrdenacao == "COMPOSTO" && candidato.QuantidadeCriteriosComposta != candidatoAnterior.QuantidadeCriteriosComposta)
                    {
                        classificacao++;
                    }
                }

                updateCommand.Parameters["SEQUENCIA"].Value = sequencia;
                updateCommand.Parameters["CLASSIFICACAO"].Value = classificacao;
                updateCommand.Parameters["ID_CANDIDATO"].Value = candidato.IdCandidato;
                updateCommand.ExecuteNonQuery();

                sequencia++;
                candidatoAnterior = candidato;
            }
        }

        public void SortearCandidatos(Action<string> updateStatus, Action<int> updateProgress, Action<string, bool> logText, int? sementePersonalizada = null)
        {

            //updateStatus("Iniciando sorteio...");

            Lista proximaLista = CarregarProximaLista();
            if (proximaLista == null)
            {
                throw new Exception("Não existem listas disponíveis para sorteio.");
            }
            bool titular = !proximaLista.Nome.Contains("RESERVA");
            double quantidadeAtual = 0;
            double quantidadeTotal = Math.Min(proximaLista.Quantidade, (int)proximaLista.CandidatosDisponiveis);

            string fonteSemente = "PERSONALIZADA";
            int semente = (sementePersonalizada == null) ? ObterSemente(ref fonteSemente) : (int)sementePersonalizada;
            ExecuteNonQuery(
                "UPDATE LISTA SET SORTEADA = 1, SEMENTE_SORTEIO = @SEMENTE_SORTEIO, FONTE_SEMENTE = @FONTE_SEMENTE WHERE ID_LISTA = @ID_LISTA",
                new SQLiteParameter("SEMENTE_SORTEIO", semente) { DbType = DbType.Int32 },
                new SQLiteParameter("FONTE_SEMENTE", fonteSemente) { DbType = DbType.String },
                new SQLiteParameter("ID_LISTA", proximaLista.IdLista) { DbType = DbType.Int32 }
            );
            Random random = new Random(semente);

            string queryGrupoSorteio = @"
                SELECT CANDIDATO_LISTA.CLASSIFICACAO AS CLASSIFICACAO, COUNT(*) AS QUANTIDADE
                FROM CANDIDATO_LISTA INNER JOIN CANDIDATO ON CANDIDATO_LISTA.ID_CANDIDATO = CANDIDATO.ID_CANDIDATO
                WHERE CANDIDATO_LISTA.ID_LISTA = @ID_LISTA AND CANDIDATO_LISTA.DATA_CONTEMPLACAO IS NULL AND CANDIDATO.CONTEMPLADO = 0
                GROUP BY CANDIDATO_LISTA.CLASSIFICACAO
                ORDER BY CANDIDATO_LISTA.CLASSIFICACAO
            ";
            SQLiteCommand commandGrupoSorteio = CreateCommand(queryGrupoSorteio);
            commandGrupoSorteio.Parameters.AddWithValue("ID_LISTA", proximaLista.IdLista);
            commandGrupoSorteio.Prepare();

            string queryCandidatosGrupo = @"
                SELECT CANDIDATO_LISTA.SEQUENCIA, CANDIDATO.ID_CANDIDATO, CANDIDATO.CPF, CANDIDATO.NOME, CANDIDATO.INSCRICAO
                FROM CANDIDATO_LISTA INNER JOIN CANDIDATO ON CANDIDATO_LISTA.ID_CANDIDATO = CANDIDATO.ID_CANDIDATO
                WHERE CANDIDATO_LISTA.ID_LISTA = @ID_LISTA AND CANDIDATO_LISTA.DATA_CONTEMPLACAO IS NULL AND CANDIDATO.CONTEMPLADO = 0 AND CANDIDATO_LISTA.CLASSIFICACAO = @CLASSIFICACAO
                ORDER BY CANDIDATO_LISTA.SEQUENCIA
            ";
            SQLiteCommand commandCandidatosGrupo = CreateCommand(queryCandidatosGrupo);
            commandCandidatosGrupo.Parameters.AddWithValue("ID_LISTA", proximaLista.IdLista);
            commandCandidatosGrupo.Parameters.AddWithValue("CLASSIFICACAO", -1);
            commandCandidatosGrupo.Prepare();

            GrupoSorteio grupoSorteio = null;
            StringBuilder lista = new StringBuilder();
            for (int i = 1; i <= proximaLista.Quantidade; i++)
            {

                if (grupoSorteio == null || grupoSorteio.Quantidade < 1)
                {
                    //updateStatus("Carregando próximo grupo de sorteio.");
                    using (SQLiteDataReader resultSet = commandGrupoSorteio.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (resultSet.Read())
                        {
                            grupoSorteio = new GrupoSorteio
                            {
                                Classificacao = Convert.ToInt32(resultSet[resultSet.GetOrdinal("CLASSIFICACAO")]),
                                Quantidade = Convert.ToInt32(resultSet[resultSet.GetOrdinal("QUANTIDADE")])
                            };
                        }
                        else
                        {
                            grupoSorteio = null;
                        }
                    }
                    if (grupoSorteio != null)
                    {
                        commandCandidatosGrupo.Parameters["CLASSIFICACAO"].Value = grupoSorteio.Classificacao;
                        using (SQLiteDataReader resultSet = commandCandidatosGrupo.ExecuteReader())
                        {
                            while (resultSet.Read())
                            {
                                CandidatoGrupo candidato = new CandidatoGrupo
                                {
                                    Sequencia = Convert.ToInt32(resultSet[resultSet.GetOrdinal("SEQUENCIA")]),
                                    IdCandidato = Convert.ToInt32(resultSet[resultSet.GetOrdinal("ID_CANDIDATO")]),
                                    Cpf = resultSet.GetString(resultSet.GetOrdinal("CPF")),
                                    Nome = resultSet.GetString(resultSet.GetOrdinal("NOME")),
                                    IdInscricao = Convert.ToInt32(resultSet[resultSet.GetOrdinal("INSCRICAO")])
                                };
                                grupoSorteio.Candidatos.Add(candidato.Sequencia, candidato);
                            }
                        }
                    }
                }

                if (grupoSorteio == null)
                {
                    break;
                }
                else
                {
                    //updateStatus($"Sorteando entre o grupo de classificação \"{grupoSorteio.Classificacao}\": {quantidadeTotal - quantidadeAtual} vagas restantes.");
                }

                int indiceSorteado = (grupoSorteio.Quantidade == 1) ? 0 : random.Next(0, grupoSorteio.Quantidade);
                CandidatoGrupo candidatoSorteado = grupoSorteio.Candidatos.Skip(indiceSorteado).Take(1).First().Value;
                candidatoSorteado.Nome = candidatoSorteado.Nome.ToUpper();
                grupoSorteio.Candidatos.Remove(candidatoSorteado.Sequencia);

                ExecuteNonQuery(
                    "UPDATE CANDIDATO SET CONTEMPLADO = 1 WHERE ID_CANDIDATO = @ID_CANDIDATO",
                    new SQLiteParameter("ID_CANDIDATO", candidatoSorteado.IdCandidato) { DbType = DbType.Int32 }
                );

                ExecuteNonQuery(
                    @"
                        UPDATE CANDIDATO_LISTA
                        SET SEQUENCIA_CONTEMPLACAO = @SEQUENCIA_CONTEMPLACAO, DATA_CONTEMPLACAO = @DATA_CONTEMPLACAO
                        WHERE ID_CANDIDATO = @ID_CANDIDATO AND ID_LISTA = @ID_LISTA
                    ",
                    new SQLiteParameter("SEQUENCIA_CONTEMPLACAO", i) { DbType = DbType.Int32 },
                    new SQLiteParameter("DATA_CONTEMPLACAO", DateTime.Now) { DbType = DbType.DateTime },
                    new SQLiteParameter("ID_CANDIDATO", candidatoSorteado.IdCandidato) { DbType = DbType.Int32 },
                    new SQLiteParameter("ID_LISTA", proximaLista.IdLista) { DbType = DbType.Int32 }
                );

                grupoSorteio.Quantidade--;
                quantidadeAtual++;

                updateProgress((int)((quantidadeAtual / quantidadeTotal) * 100));

            }

        }

        public Lista SortearProximaLista(Action<string> updateStatus, Action<int> updateProgress, Action<string, bool> logText, int? sementePersonalizada = null)
        {
            Lista lista = null;
            string queryCandidatosNaoExibidos = @"
                SELECT CANDIDATO_LISTA.SEQUENCIA_CONTEMPLACAO, CANDIDATO.ID_CANDIDATO, CANDIDATO.CPF, CANDIDATO.NOME, CANDIDATO.INSCRICAO, CANDIDATO_LISTA.ID_LISTA
                FROM CANDIDATO_LISTA INNER JOIN CANDIDATO ON CANDIDATO_LISTA.ID_CANDIDATO = CANDIDATO.ID_CANDIDATO
                WHERE CANDIDATO_LISTA.SEQUENCIA_CONTEMPLACAO IS NOT NULL AND CANDIDATO.CONTEMPLADO = 1 AND CANDIDATO_LISTA.EXIBIDO = 0
                ORDER BY CANDIDATO_LISTA.SEQUENCIA_CONTEMPLACAO
            ";
            SQLiteCommand commandCandidatosNaoExibidos = CreateCommand(queryCandidatosNaoExibidos);
            commandCandidatosNaoExibidos.Prepare();

            using (SQLiteDataReader resultSet = commandCandidatosNaoExibidos.ExecuteReader())
            {
                if (resultSet.HasRows)
                {
                    resultSet.Read();
                    int idLista = Convert.ToInt32(resultSet[resultSet.GetOrdinal("ID_LISTA")]);
                    string queryNomeLista = @"
                        SELECT NOME
                        FROM LISTA
                        WHERE ID_LISTA = @ID_LISTA
                    ";
                    SQLiteCommand commandNomeLista = CreateCommand(queryNomeLista);
                    commandNomeLista.Parameters.AddWithValue("ID_LISTA", idLista);
                    commandNomeLista.Prepare();
                    SQLiteDataReader resultNomeLista = commandNomeLista.ExecuteReader();
                    resultNomeLista.Read();
                    string nomeLista = resultNomeLista.GetString(resultNomeLista.GetOrdinal("NOME"));
                    int qtdCandidatos = CandidatosDisponiveisLista(idLista);
                    updateStatus("Sorteando lista " + nomeLista + " - " + qtdCandidatos + " candidatos");
                    CandidatoGrupo candidatoSorteado = new CandidatoGrupo
                    {
                        Sequencia = Convert.ToInt32(resultSet[resultSet.GetOrdinal("SEQUENCIA_CONTEMPLACAO")]),
                        IdCandidato = Convert.ToInt32(resultSet[resultSet.GetOrdinal("ID_CANDIDATO")]),
                        Cpf = resultSet.GetString(resultSet.GetOrdinal("CPF")),
                        Nome = resultSet.GetString(resultSet.GetOrdinal("NOME")),
                        IdInscricao = Convert.ToInt32(resultSet[resultSet.GetOrdinal("INSCRICAO")])
                    };
                    ExibirSorteado(candidatoSorteado, idLista, logText);
                    if (nomeLista.Contains("RESERVA") || sementePersonalizada != null)
                    {
                        while (resultSet.Read())
                        {
                            candidatoSorteado = new CandidatoGrupo
                            {
                                Sequencia = Convert.ToInt32(resultSet[resultSet.GetOrdinal("SEQUENCIA_CONTEMPLACAO")]),
                                IdCandidato = Convert.ToInt32(resultSet[resultSet.GetOrdinal("ID_CANDIDATO")]),
                                Cpf = resultSet.GetString(resultSet.GetOrdinal("CPF")),
                                Nome = resultSet.GetString(resultSet.GetOrdinal("NOME")),
                                IdInscricao = Convert.ToInt32(resultSet[resultSet.GetOrdinal("INSCRICAO")])
                            };
                            ExibirSorteado(candidatoSorteado, idLista, logText);
                        }
                        updateStatus("Sorteio Lista " + nomeLista + " finalizado!");
                        lista = new Lista { IdLista = idLista, Nome = nomeLista };
                    }
                    else
                    {
                        if (resultSet.Read() == false)
                        {
                            updateStatus("Sorteio Lista " + nomeLista + " finalizado!");
                            lista = new Lista { IdLista = idLista, Nome = nomeLista };
                        }
                    }
                    resultNomeLista.Close();
                }
                else
                {
                    SortearCandidatos(updateStatus, updateProgress, logText, sementePersonalizada);
                    lista = SortearProximaLista(updateStatus, updateProgress, logText, sementePersonalizada); //VER
                    if (lista != null)
                    {
                        int qtdCandidatos = CandidatosDisponiveisLista(lista.IdLista);
                        updateStatus("Sorteio Lista " + lista.Nome + " - " + qtdCandidatos + " candidatos");
                    }
                }
                resultSet.Close();
            }
            return lista;
        }

        public void ExibirSorteado(CandidatoGrupo candidatoSorteado, int idLista, Action<string, bool> logText)
        {
            string nomeSorteado = string.Format("{0:0000} - ***.{1}-** - {2} (Inscrição {3})", candidatoSorteado.Sequencia, candidatoSorteado.Cpf.Substring(4, 7), candidatoSorteado.Nome.ToUpper(), candidatoSorteado.IdInscricao.ToString());
            int indice = 0;
            int pos = nomeSorteado.Length;
            DateTime momento = DateTime.Now;
            DateTime momentoFinal = DateTime.Now.AddMilliseconds(5000);

            int posNome = candidatoSorteado.Nome.Length;
            string nomeDecifrar = candidatoSorteado.Nome;

            bool concluido = false;
            string complemento = "zmylxkwjviuhtgsfreqdpcobna";
            nomeDecifrar = nomeDecifrar.ToLower() + complemento;
            posNome = nomeDecifrar.Length;
            while (!concluido)
            {
                indice = 0;
                int trecho = 0;
                while (indice < posNome)
                {
                    trecho = posNome - indice - 1;
                    if (nomeDecifrar[indice] != ' ')
                    {
                        nomeDecifrar = nomeDecifrar.Substring(0, indice) + (char)((int)nomeDecifrar[indice] - 1) + nomeDecifrar.Substring(indice + 1, trecho);
                    }
                    indice++;
                }

                logText(nomeDecifrar, true);

                if (String.IsNullOrWhiteSpace(nomeDecifrar))
                {
                    nomeDecifrar = candidatoSorteado.Nome;
                }

                if (nomeDecifrar.TrimEnd() == candidatoSorteado.Nome)
                {
                    concluido = true;
                }

                momento = DateTime.Now;
                momentoFinal = DateTime.Now.AddMilliseconds(30);
                while (momento < momentoFinal)
                {
                    momento = DateTime.Now;
                }
                indice++;
            }

            logText(string.Format("***.{0:000'.'000}-** - {1} - {2}", String.Format("{0:00000000000}", candidatoSorteado.Cpf.Substring(4, 7)), candidatoSorteado.Nome.ToUpper(), candidatoSorteado.IdInscricao.ToString()), true);

            momento = DateTime.Now;
            momentoFinal = DateTime.Now.AddMilliseconds(3000);
            while (momento < momentoFinal)
            {
                momento = DateTime.Now;
            }

            logText(CompletarNome(nomeSorteado), false);
            ExecuteNonQuery(
                @"
                        UPDATE CANDIDATO_LISTA
                        SET EXIBIDO = 1
                        WHERE ID_CANDIDATO = @ID_CANDIDATO AND ID_LISTA = @ID_LISTA
                    ",
                new SQLiteParameter("ID_CANDIDATO", candidatoSorteado.IdCandidato) { DbType = DbType.Int32 },
                new SQLiteParameter("ID_LISTA", idLista) { DbType = DbType.Int32 }
            );
        }

        private int ObterSemente(ref string fonteSemente)
        {

            int? semente = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(@"https://www.random.org/cgi-bin/randbyte?nbytes=4&format=h").Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        string content = response.Content.ReadAsStringAsync().Result;
                        semente = Convert.ToInt32(content.Replace(" ", ""), 16);
                        fonteSemente = "RANDOM.ORG";
                    }
                }
            }
            catch { }

            if (semente == null)
            {
                fonteSemente = "SISTEMA";
                return (int)DateTime.Now.Ticks;
            }
            else
            {
                return (int)semente;
            }
        }

        private string CompletarNome(string nome)
        {
            int res = 0;
            Int32.TryParse(nome.Substring(0, 4), out res);
            if (res % 2 > 0)
            {
                if (nome.Count() <= 94)
                {
                    string espacos = "                                                                                              ";
                    return String.Concat(nome, espacos.Substring(0, 94 - nome.Count()));
                }
            }
            else
            {
                return String.Concat("|", nome);
            }
            return nome;
        }

        public static string DiretorioExportacaoCSV => $"{AppDomain.CurrentDomain.BaseDirectory}CSV";

        public void ExportarListas(Action<string> updateStatus, string caminhoPasta=null)
        {

            updateStatus("Iniciando exportação...");
            string directoryPath;
            if (String.IsNullOrWhiteSpace(caminhoPasta))
            {
                directoryPath = DiretorioExportacaoCSV;
                if (Directory.Exists(directoryPath))
                {
                    updateStatus("Excluindo arquivos anteriores.");
                    Directory.Delete(directoryPath, true);
                }
                Directory.CreateDirectory(directoryPath);
            } else
            {
                directoryPath = caminhoPasta;
            }

            string[] tabelas = new string[] { "CANDIDATO", "LISTA", "CANDIDATO_LISTA" };
            foreach (string tabela in tabelas)
            {
                WriteTable(directoryPath, tabela, updateStatus);
            }

            updateStatus("Finalizando exportação...");
        }

        private void WriteTable(string directoryPath, string tableName, Action<string> updateStatus)
        {

            int count = 0;
            int total = ExecuteScalar<int>($"SELECT COUNT(*) FROM {tableName}");

            using (StreamWriter writter = new StreamWriter($"{directoryPath}/{tableName}.CSV"))
            {
                using (SQLiteCommand command = CreateCommand($"SELECT * FROM {tableName}"))
                {
                    using (SQLiteDataReader dataReader = command.ExecuteReader())
                    {
                        IEnumerable<int> fieldRange = Enumerable.Range(0, dataReader.FieldCount);
                        CsvWriter.WriteRow(writter, fieldRange.Select(i => dataReader.GetName(i).ToLower()).ToArray());
                        while (dataReader.Read())
                        {
                            updateStatus($"Exportando tabela \"{tableName}\" - linha {++count} de {total}.");
                            CsvWriter.WriteRow(
                                writter,
                                fieldRange.Select(i => dataReader.GetValue(i))
                                    .Select(i => {
                                        if (i is bool)
                                        {
                                            return ((bool)i) ? "1" : "0";
                                        }
                                        else
                                        {
                                            return i.ToString();
                                        }
                                    })
                                    .ToArray()
                            );
                        }
                    }
                };
            }
        }

        public ListaPub CarregarListaPublicacao(int idLista)
        {

            ListaPub lista;

            using (SQLiteCommand command = CreateCommand("SELECT * FROM LISTA WHERE ID_LISTA = @ID_LISTA"))
            {
                command.Parameters.AddWithValue("ID_LISTA", idLista);
                using (SQLiteDataReader resultSet = command.ExecuteReader())
                {
                    resultSet.Read();
                    lista = new ListaPub()
                    {
                        IdLista = Convert.ToInt32(resultSet[resultSet.GetOrdinal("ORDEM_SORTEIO")]),
                        Nome = resultSet.GetString(resultSet.GetOrdinal("NOME")),
                        FonteSementeSorteio = resultSet.GetString(resultSet.GetOrdinal("FONTE_SEMENTE")),
                        SementeSorteio = Convert.ToInt32(resultSet[resultSet.GetOrdinal("SEMENTE_SORTEIO")]),
                        Candidatos = new List<CandidatoPub>()
                    };
                }
            }

            string queryCandidatos = @"
                SELECT
                    CANDIDATO_LISTA.SEQUENCIA_CONTEMPLACAO, CANDIDATO.CPF, CANDIDATO.NOME, QUANTIDADE_CRITERIOS, CANDIDATO.INSCRICAO
                FROM
                    CANDIDATO_LISTA
                    INNER JOIN LISTA ON CANDIDATO_LISTA.ID_LISTA = LISTA.ID_LISTA
                    INNER JOIN CANDIDATO ON CANDIDATO_LISTA.ID_CANDIDATO = CANDIDATO.ID_CANDIDATO
                WHERE LISTA.ID_LISTA = @ID_LISTA AND CANDIDATO_LISTA.SEQUENCIA_CONTEMPLACAO IS NOT NULL
                ORDER BY CANDIDATO_LISTA.SEQUENCIA_CONTEMPLACAO
            ";

            using (SQLiteCommand command = CreateCommand(queryCandidatos))
            {
                command.Parameters.AddWithValue("ID_LISTA", idLista);
                using (SQLiteDataReader resultSet = command.ExecuteReader())
                {
                    while (resultSet.Read())
                    {
                        lista.Candidatos.Add(new CandidatoPub
                        {
                            IdCandidato = Convert.ToInt32(resultSet[resultSet.GetOrdinal("SEQUENCIA_CONTEMPLACAO")]),
                            Cpf = resultSet.GetString(resultSet.GetOrdinal("CPF")),
                            Nome = resultSet.GetString(resultSet.GetOrdinal("NOME")),
                            QuantidadeCriterios = Convert.ToInt32(resultSet[resultSet.GetOrdinal("QUANTIDADE_CRITERIOS")]),
                            IdInscricao = Convert.ToInt32(resultSet[resultSet.GetOrdinal("INSCRICAO")])
                        });
                    }
                }
            }

            return lista;
        }

        public ListaPub CarregarListaSorteados()
        {

            ListaPub lista;
            List<int> idsListasReservas = new List<int>();

            using (SQLiteCommand command = CreateCommand("SELECT ID_LISTA FROM LISTA WHERE NOME LIKE '% RESERVA %'"))
            {
                using (SQLiteDataReader resultSet = command.ExecuteReader())
                {
                    while (resultSet.Read())
                    {
                        idsListasReservas.Add(Convert.ToInt32(resultSet[resultSet.GetOrdinal("ID_LISTA")]));
                    }
                }
            }

            string listasReservas = String.Join(",", idsListasReservas);

            lista = new ListaPub()
            {
                IdLista = 0,
                Nome = "",
                FonteSementeSorteio = "",
                SementeSorteio = 0,
                Candidatos = new List<CandidatoPub>()
            };

            string queryCandidatos = @"
                SELECT
                    CANDIDATO_LISTA.SEQUENCIA_CONTEMPLACAO, CANDIDATO.CPF, CANDIDATO.NOME, QUANTIDADE_CRITERIOS, CANDIDATO.INSCRICAO
                FROM
                    CANDIDATO_LISTA
                    INNER JOIN CANDIDATO ON CANDIDATO_LISTA.ID_CANDIDATO = CANDIDATO.ID_CANDIDATO
                WHERE CANDIDATO_LISTA.SEQUENCIA_CONTEMPLACAO IS NOT NULL
                AND CANDIDATO_LISTA.ID_LISTA NOT IN (" + listasReservas + @")
                ORDER BY CANDIDATO.NOME
            ";

            using (SQLiteCommand command = CreateCommand(queryCandidatos))
            {
                using (SQLiteDataReader resultSet = command.ExecuteReader())
                {
                    while (resultSet.Read())
                    {
                        lista.Candidatos.Add(new CandidatoPub
                        {
                            IdCandidato = Convert.ToInt32(resultSet[resultSet.GetOrdinal("SEQUENCIA_CONTEMPLACAO")]),
                            Cpf = resultSet.GetString(resultSet.GetOrdinal("CPF")),
                            Nome = resultSet.GetString(resultSet.GetOrdinal("NOME")),
                            QuantidadeCriterios = Convert.ToInt32(resultSet[resultSet.GetOrdinal("QUANTIDADE_CRITERIOS")]),
                            IdInscricao = Convert.ToInt32(resultSet[resultSet.GetOrdinal("INSCRICAO")])
                        });
                    }
                }
            }

            return lista;
        }

        public void PublicarLista(int idLista)
        {
            ExecuteNonQuery(
                "UPDATE LISTA SET PUBLICADA = 1 WHERE ID_LISTA = @ID_LISTA",
                new SQLiteParameter("ID_LISTA", idLista) { DbType = DbType.Int32 }
            );
        }

        public string ValidarCabecalho(string cabecalho)
        {
            string[] termosCabecalho = { "CPF DO RESPONSÁVEL", "NOME DO RESPONSÁVEL", "TOTAL DE CRITÉRIOS", "DEFICIENTE", "RESPONSÁVEL E/OU  CÔNJUGE IDOSO (60+)", "RESPONSÁVEL E OU CÔNJUGE COM 80 ANOS OU MAIS", "NÚMERO DA INSCRIÇÃO", "FAIXA SALARIAL" };
            StringBuilder listaTermosNaoEncontrados = new StringBuilder();

            for(int ix=0; ix < termosCabecalho.Count(); ix++)
            {
                if (!cabecalho.Contains(termosCabecalho[ix]))
                {
                    listaTermosNaoEncontrados.AppendLine(String.Concat(" - ", termosCabecalho[ix]));
                }
            }

            if (listaTermosNaoEncontrados.Length > 0)
            {
                listaTermosNaoEncontrados.Insert(0, "Relação de colunas não encontradas no arquivo de candidatos inscritos: \n\n");
            }

            return listaTermosNaoEncontrados.ToString();
        }
    }
}
