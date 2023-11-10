using Excel;
using Habitasorte.Business.Model;
using Habitasorte.Business.Model.Publicacao;
using Habitasorte.Business.Pdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Habitasorte.Business {

    public delegate void SorteioChangedEventHandler(Sorteio s);

    public class SorteioService {

        public event SorteioChangedEventHandler SorteioChanged;

        private Sorteio model;
        public Sorteio Model {
            get { return model; }
            set { model = value; SorteioChanged(model); }
        }

        public SorteioService() {
            Database.Initialize();
        }

        private void Execute(Action<Database> action) {
            using (SQLiteConnection connection = Database.CreateConnection()) {
                using (SQLiteTransaction tx = connection.BeginTransaction()) {
                    Database banco = new Database(connection, tx);
                    try {
                        action(banco);
                        tx.Commit();
                    } catch(Exception e) {
                        try     { tx.Rollback(); } catch { }
                        throw;
                    }
                    tx.Dispose();
                }
                connection.Dispose();
            }
        }

        private void AtualizarStatusSorteio(Database database, string status) {
            Model.StatusSorteio = status;
            database.AtualizarStatusSorteio(status);
        }

        /* Configuração */

        public void ExcluirBancoReiniciarAplicacao() {
            Database.ExcluirBanco();
            System.Windows.Application.Current.Shutdown();
        }

        /* Ações */

        public void CarregarSorteio() {
            Execute(d => {
                Model = d.CarregarSorteio();
            });
        }

        public void AtualizarSorteio() {
            Execute(d => {
                d.AtualizarSorteio(Model);
                AtualizarStatusSorteio(d, Status.IMPORTACAO);
            });
        }

        public void CarregarListas() {
            Execute(d => {
                Model.Listas = d.CarregarListas();
            });
            
        }

        public void CarregarProximaLista() {
            Execute(d => {
                Model.ProximaLista = d.CarregarProximaLista();
            });
        }

        public int ContagemCandidatos() {
            int contagemCandidatos = 0;
            Execute(d => {
                contagemCandidatos = d.ContagemCandidatos();
            });
            return contagemCandidatos;
        }

        public void AtualizarListas() {
            Execute(d => {
                d.AtualizarListas(Model.Listas);
                AtualizarStatusSorteio(d, Status.SORTEIO);
            });
        }

        public void CriarListasSorteioDeFaixas(string arquivoImportacao, string faixa, Action<string> updateStatus, Action<int> updateProgress, int listaAtual, int totalListas, int incremento, int rendaMinima, int rendaMaxima)
        {
            Execute(d =>
            {
                if (arquivoImportacao != null)
                {
                    using (Stream stream = File.OpenRead(arquivoImportacao))
                    {
                        using (IExcelDataReader excelReader = CreateExcelReader(arquivoImportacao, stream))
                        {
                            string linhaCabecalho = String.Join("", excelReader.AsDataSet().Tables[0].Rows[0].ItemArray);
                            string validacaoCabecalho = d.ValidarCabecalho(linhaCabecalho);
                            if (String.IsNullOrWhiteSpace(validacaoCabecalho))
                            {
                                excelReader.IsFirstRowAsColumnNames = true;
                            } else
                            {
                                throw new Exception(validacaoCabecalho);
                            }

                            System.Data.DataSet plan = excelReader.AsDataSet().Copy();
                            System.Data.DataTableReader planReader = new System.Data.DataTableReader(plan.Tables[0]);
                            d.CopiarCandidatosArquivo(faixa, planReader, updateStatus, updateProgress);
                            planReader.Close();
                            excelReader.Close();
                        }
                        stream.Dispose();
                        stream.Close();
                    }
                }
                d.CriarListasSorteioPorFaixa(faixa, updateStatus, updateProgress, listaAtual, totalListas, incremento, rendaMinima, rendaMaxima);
                AtualizarStatusSorteio(d, Status.QUANTIDADES);
            });
            //DateTime hrAtual = DateTime.Now;
            //DateTime tempoCommit = hrAtual.AddMinutes(1);
            //while (DateTime.Now < tempoCommit)
            //{

            //}
            //Execute(d => {
            //    d.CriarListasSorteioPorFaixa(faixa, updateStatus, updateProgress, listaAtual, totalListas, incremento, rendaMinima, rendaMaxima);
            //    AtualizarStatusSorteio(d, Status.QUANTIDADES);
            //});
        }

        private IExcelDataReader CreateExcelReader(string arquivoImportacao, Stream stream) {
            return (arquivoImportacao.ToLower().EndsWith(".xlsx") || arquivoImportacao.ToLower().EndsWith(".oldxls")) ?
                ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream);
        }

        public bool SortearProximaLista(Action<string> updateStatus, Action<int> updateProgress, Action<string, bool> logText, int? sementePersonalizada = null) {
            Lista listaSorteada = null;
            Lista listaAtual = new Lista { IdLista = model.ProximaLista.IdLista };
            String diretorioListas = "";
            Execute(d => {
                listaSorteada = d.SortearProximaLista(updateStatus, updateProgress, logText, sementePersonalizada);
                if (listaSorteada != null)
                {
                    if (Model.StatusSorteio == Status.SORTEIO)
                    {
                        AtualizarStatusSorteio(d, Status.SORTEIO_INICIADO);
                    }
                    if (d.CarregarProximaLista() == null)
                    {
                        AtualizarStatusSorteio(d, Status.FINALIZADO);
                    }
                }
            });
            
            if (listaSorteada != null)
            {
                diretorioListas = System.Configuration.ConfigurationManager.AppSettings.Get("PASTA_RESULTADO");
                if (String.IsNullOrWhiteSpace(diretorioListas))
                {
                    diretorioListas = "C:\\HabitaSorte_CasaNova2.2\\";
                }
                if (!Directory.Exists(diretorioListas))
                {
                    Directory.CreateDirectory(diretorioListas);
                }
                if (listaSorteada != null)
                {
                    SalvarLista(listaSorteada, (String.Concat(diretorioListas, listaSorteada.OrdemSorteio.ToString("00"), " - ", listaSorteada.Nome.Split('%')[0], ".pdf")));
                } else
                {
                    SalvarLista(model.ProximaLista, (String.Concat(diretorioListas, model.ProximaLista.OrdemSorteio.ToString("00"), " - ", model.ProximaLista.Nome.Split('%')[0], ".pdf")));
                }
            }
            return listaSorteada != null;
        }

        public string DiretorioExportacaoCSV => Database.DiretorioExportacaoCSV;
        public bool DiretorioExportacaoCSVExistente => Directory.Exists(Database.DiretorioExportacaoCSV);

        public void ExportarListas(Action<string> updateStatus, string caminhoPasta=null) {
            Execute(d => {
                d.ExportarListas(updateStatus, caminhoPasta);
            });
        }

        public void SalvarLista(Lista lista, string caminhoArquivo) {
            ListaPub listaPublicacao = null;
            Execute(d => { listaPublicacao = d.CarregarListaPublicacao(lista.IdLista); });
            PdfFileWriter.WriteToPdf(caminhoArquivo, Model, listaPublicacao);
            System.Diagnostics.Process.Start(caminhoArquivo);
        }

        public void SalvarSorteados(string caminhoArquivo)
        {
            ListaPub listaPublicacao = null;
            Execute(d => { listaPublicacao = d.CarregarListaSorteados(); });
            PdfFileWriter.WriteSorteadosToPdf(caminhoArquivo, Model, listaPublicacao);
            System.Diagnostics.Process.Start(caminhoArquivo);
        }
    }
}
