using Habitasorte.Business;
using Habitasorte.Business.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Habitasorte {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private bool activated = false;
        private bool processing = false;

        private SorteioService Service { get; set; }
        private Sorteio Sorteio => Service.Model;
        private string StatusSorteio => Sorteio.StatusSorteio;

        public MainWindow() {

            InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            Title += $" v{versionInfo.FileVersion}";

            Service = new SorteioService();
            Service.SorteioChanged += (s) => { DataContext = s; };
            Service.CarregarSorteio();

            EtapaCadastro(false);
            EtapaImportacao(false);
            EtapaQuantidades(false);
            EtapaSorteio(false);
            EtapaFinalizado(false);
        }

        private void Window_Activated(object sender, EventArgs e) {
            RegistrarPastaResultado();
            if (!activated) {
                activated = true;
                switch (StatusSorteio) {
                    case Status.CADASTRO:
                        empreendimento2.Visibility = Visibility.Hidden;
                        empreendimento3.Visibility = Visibility.Hidden;
                        empreendimento4.Visibility = Visibility.Hidden;
                        empreendimento5.Visibility = Visibility.Hidden;
                        empreendimento6.Visibility = Visibility.Hidden;
                        chkBoxEmpreendimento2.Visibility = Visibility.Hidden;
                        chkBoxEmpreendimento3.Visibility = Visibility.Hidden;
                        chkBoxEmpreendimento4.Visibility = Visibility.Hidden;
                        chkBoxEmpreendimento5.Visibility = Visibility.Hidden;
                        chkBoxEmpreendimento6.Visibility = Visibility.Hidden;
                        EtapaCadastro(true);
                        break;
                    case Status.IMPORTACAO:
                        EtapaImportacao(true);
                        break;
                    case Status.QUANTIDADES:
                        EtapaQuantidades(true);
                        break;
                    case Status.SORTEIO:
                    case Status.SORTEIO_INICIADO:
                        EtapaSorteio(true);
                        break;
                    case Status.FINALIZADO:
                        EtapaFinalizado(true);
                        break;
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            e.Cancel = processing;
        }

        private bool VerificarStatus(params string[] statuses) {
            return statuses.Contains(StatusSorteio);
        }

        private void AlternarTab(TabItem tab, bool ativo) {
            if (ativo) {
                tab.Visibility = Visibility.Visible;
                tab.Focus();
                lblEtapaSorteio.Content = tab.Header;
            }
            (tab.Content as Grid).IsEnabled = ativo;
            tab.Visibility = Visibility.Collapsed;
        }

        private void ShowMessage(string message) {
            MessageBox.Show(
                message,
                "AVISO",
                MessageBoxButton.OK,
                MessageBoxImage.Asterisk
            );
        }

        private void ShowErrorMessage(string message) {
            MessageBox.Show(
                message,
                "ERRO",
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation
            );
        }

        /* Ativação das etapas do sorteio. */
        private void EtapaCadastro(bool ativo) {
            Service.CarregarSorteio();
            btnAvancarCadastro.IsEnabled = !VerificarStatus(Status.CADASTRO);
            grdFormCadastro.IsEnabled = VerificarStatus(Status.CADASTRO, Status.IMPORTACAO);
            AlternarTab(tabCadastro, ativo);
            if (ativo)
            {
                empreendimento1.IsReadOnly = false;
                empreendimento2.IsReadOnly = false;
                empreendimento3.IsReadOnly = false;
                empreendimento4.IsReadOnly = false;
                empreendimento5.IsReadOnly = false;
                empreendimento6.IsReadOnly = false;
            }
        }

        private void EtapaImportacao(bool ativo) {
            btnRecuarImportacao.IsEnabled = true;
            btnAvancarImportacao.IsEnabled = !VerificarStatus(Status.CADASTRO, Status.IMPORTACAO);
            grdArquivoImportacao.IsEnabled = VerificarStatus(Status.IMPORTACAO);
            grdConfiguracaoImportacao.IsEnabled = true;
            grdImportacaoEmAndamento.IsEnabled = false;
            if (!VerificarStatus(Status.IMPORTACAO)) {
                btnImportarArquivo.IsEnabled = false;
                if (ativo) {
                    lblStatusImportacao.Content = $"{Service.ContagemCandidatos()} candidatos importados.";
                }
            }
            AlternarTab(tabImportacao, ativo);
        }

        private void EtapaQuantidades(bool ativo) {
            Service.CarregarListas();
            lstQuantidades.IsEnabled = VerificarStatus(Status.QUANTIDADES, Status.SORTEIO);
            btnAtualizarQuantidades.IsEnabled = VerificarStatus(Status.QUANTIDADES, Status.SORTEIO);
            btnAvancarQuantidades.IsEnabled = !VerificarStatus(Status.CADASTRO, Status.IMPORTACAO, Status.QUANTIDADES);
            AlternarTab(tabQuantidades, ativo);
        }

        private void EtapaSorteio(bool ativo) {
            Service.CarregarListas();
            Service.CarregarProximaLista();
            txtSementePersonalizada.Text = "";
            btnRecuarSorteio.IsEnabled = true;
            btnAvancarSorteio.IsEnabled = VerificarStatus(Status.FINALIZADO);
            grdIniciarSorteio.IsEnabled = VerificarStatus(Status.SORTEIO, Status.SORTEIO_INICIADO);
            grdSorteioEmAndamento.IsEnabled = false;
            lstSorteioListasSorteio.IsEnabled = true;
            AlternarTab(tabSorteio, ativo);
        }

        private void EtapaFinalizado(bool ativo) {
            btnRecuarFinalizado.IsEnabled = true;
            btnExportarListas.IsEnabled = true;
            btnAbrirDiretorioExportacao.IsEnabled = ativo && Service.DiretorioExportacaoCSVExistente;
            AlternarTab(tabFinalizado, ativo);
        }

        /* Transição entre as etapas .*/

        private void btnAvancarConfiguracao_Click(object sender, RoutedEventArgs e) {
            EtapaCadastro(true);
        }

        private void buttonAvancarCadastro_Click(object sender, RoutedEventArgs e) {
            EtapaCadastro(false);
            EtapaImportacao(true);
        }

        private void buttonRecuarImportacao_Click(object sender, RoutedEventArgs e) {
            EtapaImportacao(false);
            EtapaCadastro(true);
        }

        private void buttonAvancarImportacao_Click(object sender, RoutedEventArgs e) {
            EtapaImportacao(false);
            EtapaQuantidades(true);
        }

        private void buttonRecuarQuantidades_Click(object sender, RoutedEventArgs e) {
            EtapaQuantidades(false);
            EtapaImportacao(true);
        }

        private void buttonAvancarQuantidades_Click(object sender, RoutedEventArgs e) {
            EtapaQuantidades(false);
            EtapaSorteio(true);
        }

        private void buttonRecuarSorteio_Click(object sender, RoutedEventArgs e) {
            EtapaSorteio(false);
            EtapaQuantidades(true);
        }

        private void buttonAvancarSorteio_Click(object sender, RoutedEventArgs e) {
            EtapaSorteio(false);
            EtapaFinalizado(true);
        }

        private void buttonRecuarFinalizado_Click(object sender, RoutedEventArgs e) {
            EtapaFinalizado(false);
            EtapaSorteio(true);
        }

        /* Etapa de Configuração */

        private void btnExcluirDados_Click(object sender, RoutedEventArgs e) {

            MessageBoxResult result = MessageBox.Show(
                $"Excluir dados e reiniciar aplicação?",
                "Excluir dados?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes) {
                MessageBoxResult confirmResult = MessageBox.Show(
                    $"Tem certeza? A exclusão dos dados é definitiva!",
                    "Excluir dados?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (confirmResult == MessageBoxResult.Yes) {
                    Service.ExcluirBancoReiniciarAplicacao();
                }
            }
        }

        /* Etapa de Cadastro. */

        private void buttonAtualizarCadastro_Click(object sender, RoutedEventArgs e) {
            if (String.IsNullOrWhiteSpace(empreendimento1.Text))
            {
                ShowMessage("Nome do Empreendimento 1 não preenchido");
                return;
            }
            if (Sorteio.IsValid) {
                Service.AtualizarSorteio();
                EtapaCadastro(true);
                ShowMessage("Sorteio Alterado!");
            }
        }

        /* Etapa de Importação. */

        private string GetDragEventFile(DragEventArgs e) {

            string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop, false);

            bool validFile = files != null
                && files.Count() == 1
                && (files.First().ToLower().EndsWith(".xls") || files.First().ToLower().EndsWith(".xlsx"));

            return (validFile) ? files.First() : null;
        }

        private void AtribuirArquivoImportacao(string file) {
            lblNomeArquivo.Content = file.Split(new char[] { '\\', '/' }).Last();
            lblCaminhoArquivo.Content = file;
            imgArquivoSelecionado.Visibility = Visibility.Visible;
            imgSemArquivo.Visibility = Visibility.Hidden;
            btnImportarArquivo.IsEnabled = true;
        }

        private void LimparArquivoImportacao() {
            lblNomeArquivo.Content = "";
            lblCaminhoArquivo.Content = "";
            imgArquivoSelecionado.Visibility = Visibility.Hidden;
            imgSemArquivo.Visibility = Visibility.Visible;
            btnImportarArquivo.IsEnabled = false;
        }
        
        private void gridImportacao_DragEnter(object sender, DragEventArgs e) {
            if (GetDragEventFile(e) != null) {
                imgCerto.Visibility = Visibility.Visible;
            } else {
                imgErrado.Visibility = Visibility.Visible;
            }
        }

        private void gridImportacao_DragLeave(object sender, DragEventArgs e) {
            imgErrado.Visibility = Visibility.Hidden;
            imgCerto.Visibility = Visibility.Hidden;
        }

        private void gridArquivoImportacao_Drop(object sender, DragEventArgs e) {
            imgErrado.Visibility = Visibility.Hidden;
            imgCerto.Visibility = Visibility.Hidden;
            string file = GetDragEventFile(e);
            if (GetDragEventFile(e) != null) {
                AtribuirArquivoImportacao(file);
            } else {
                LimparArquivoImportacao();
            }
        }

        private void gridArquivoImportacao_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.DefaultExt = ".xlsx";
            fileDialog.Filter = "Planilha Excel (*.xlsx)|*.xlsx|Planilha Excel 97-2003 (*.xls)|*.xls";

            bool? result = fileDialog.ShowDialog();
            if (result == true) {
                AtribuirArquivoImportacao(fileDialog.FileName);
            }
        }

        private void gridArquivoImportacao(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.DefaultExt = ".xlsx";
            fileDialog.Filter = "Planilha Excel (*.xlsx)|*.xlsx|Planilha Excel 97-2003 (*.xls)|*.xls";
            

            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                txtFaixaA.Text = fileDialog.FileName;
                btnImportarArquivo.IsEnabled = true;
                empreendimento1.Focus();
            }
        }

        private void buttonImportarArquivosFaixas_Click(object sender, RoutedEventArgs e)
        {

            btnRecuarImportacao.IsEnabled = false;
            btnAvancarImportacao.IsEnabled = false;
            grdConfiguracaoImportacao.IsEnabled = false;
            grdImportacaoEmAndamento.IsEnabled = true;

            string caminhoArquivoFaixaA = txtFaixaA.Text.Contains("\\") ? txtFaixaA.Text : null;
            string caminhoArquivoFaixaB = String.Empty;
            string caminhoArquivoFaixaC = String.Empty;
            string caminhoArquivoFaixaD = String.Empty;
            string caminhoArquivoFaixaE = String.Empty;

            string nomeEmpreendimento1 = empreendimento1.Text;
            string nomeEmpreendimento2 = empreendimento2.Text;
            string nomeEmpreendimento3 = empreendimento3.Text;
            string nomeEmpreendimento4 = empreendimento4.Text;
            string nomeEmpreendimento5 = empreendimento5.Text;
            string nomeEmpreendimento6 = empreendimento6.Text;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (wSender, wE) => {

                processing = true;

                Action<string> updateStatus = (value) => Dispatcher.Invoke(() => { lblStatusImportacao.Content = value; });
                Action<int> updateProgress = (value) => Dispatcher.Invoke(() => { pgrImportacao.Value = value; });

                try
                {
                    int listaAtual = 1;
                    int qtdEmpreendimentos = (String.IsNullOrWhiteSpace(nomeEmpreendimento1) ? 0 : 1) +
                                             (String.IsNullOrWhiteSpace(nomeEmpreendimento2) ? 0 : 1) +
                                             (String.IsNullOrWhiteSpace(nomeEmpreendimento3) ? 0 : 1) +
                                             (String.IsNullOrWhiteSpace(nomeEmpreendimento4) ? 0 : 1) +
                                             (String.IsNullOrWhiteSpace(nomeEmpreendimento5) ? 0 : 1) +
                                             (String.IsNullOrWhiteSpace(nomeEmpreendimento6) ? 0 : 1);

                    int qtdListas = 3 * 3 * 2 * qtdEmpreendimentos;

                    if (String.IsNullOrWhiteSpace(nomeEmpreendimento1))
                    {
                        throw new Exception ("Nome do Empreendimento 1 não preenchido");
                    }

                    Service.CriarListasSorteioDeFaixas(caminhoArquivoFaixaA, NomearLista(nomeEmpreendimento1, "A", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento2)) {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento2, "A", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento3))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento3, "A", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento4))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento4, "A", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento5))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento5, "A", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento6))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento6, "A", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    }

                    listaAtual = listaAtual + 3;
                    Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento1, "B", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento2))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento2, "B", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento3))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento3, "B", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento4))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento4, "B", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento5))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento5, "B", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento6))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento6, "B", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    }

                    listaAtual = listaAtual + 3;
                    Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento1, "C", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento2))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento2, "C", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento3))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento3, "C", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento4))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento4, "C", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento5))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento5, "C", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento6))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento6, "C", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    }

                    listaAtual = listaAtual + 3;
                    Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento1, "A RESERVA", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento2))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento2, "A RESERVA", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento3))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento3, "A RESERVA", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento4))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento4, "A RESERVA", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento5))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento5, "A RESERVA", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento6))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento6, "A RESERVA", "100", 0, 3), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 0, 3);
                    }

                    listaAtual = listaAtual + 3;
                    Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento1, "B RESERVA", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento2))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento2, "B RESERVA", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento3))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento3, "B RESERVA", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento4))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento4, "B RESERVA", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento5))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento5, "B RESERVA", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento6))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento6, "B RESERVA", "50", 2, 5), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 2, 5);
                    }

                    listaAtual = listaAtual + 3;
                    Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento1, "C RESERVA", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento2))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento2, "C RESERVA", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento3))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento3, "C RESERVA", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento4))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento4, "C RESERVA", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento5))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento5, "C RESERVA", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    }
                    if (!String.IsNullOrWhiteSpace(nomeEmpreendimento6))
                    {
                        listaAtual = listaAtual + 3;
                        Service.CriarListasSorteioDeFaixas(null, NomearLista(nomeEmpreendimento6, "C RESERVA", "25", 3, 7), updateStatus, updateProgress, listaAtual, qtdListas, qtdEmpreendimentos, 3, 7);
                    }
                    updateStatus("Importação finalizada.");
                }
                catch (Exception exception)
                {
                    ShowErrorMessage($"Erro na importação: {exception.Message}");
                    updateProgress(0);
                    updateStatus("Erro na importação.");
                }

                Dispatcher.Invoke(() => EtapaImportacao(true));
                processing = false;
            };
            worker.RunWorkerAsync();
        }

        /* Etapa de Quantidades. */

        private void buttonAtualizarQuantidades_Click(object sender, RoutedEventArgs e) {
            if (Sorteio.Listas.All(l => l.IsValid)) {
                Service.AtualizarListas();
                ShowMessage("Quantidades das listas atualizadas!");
                EtapaQuantidades(true);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) {
            (sender as TextBox).SelectAll();
        }

        /* Etapa de Sorteio. */

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            txtSementePersonalizada.IsEnabled = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            txtSementePersonalizada.Clear();
            txtSementePersonalizada.IsEnabled = false;
        }

        private void BloquearEtapaSorteio() {

            btnRecuarSorteio.IsEnabled = false;
            btnAvancarSorteio.IsEnabled = false;
            grdIniciarSorteio.IsEnabled = false;
            grdSorteioEmAndamento.IsEnabled = true;
            lstSorteioListasSorteio.IsEnabled = false;
        }

        private void buttonSortearProximaLista_Click(object sender, RoutedEventArgs e) {

            int? sementePersonalizada = null;
            if (chkSementePersonalizada.IsChecked == true) {
                int valorSemente;
                if (!int.TryParse(txtSementePersonalizada.Text.Trim(), out valorSemente)) {
                    ShowErrorMessage("O valor de semente informado é inválido.");
                    return;
                } else {
                    sementePersonalizada = valorSemente;
                }
            }

            BloquearEtapaSorteio();

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (wSender, wE) => {

                processing = true;

                Action<string> updateStatus = (value) => Dispatcher.Invoke(() => { lblStatusSorteio.Content = value; });
                Action<int> updateProgress = (value) => Dispatcher.Invoke(() => { pgrSorteio.Value = value; });
                Action<string, bool> logText = (value, substituir) => Dispatcher.Invoke(() => {
                    if (substituir)
                    {
                        lblNomeSorteado.Content = value;
                        if (value.Length >= 75)
                        {
                            lblNomeSorteado.FontSize = 28;
                        }
                        else
                        {
                            if (value.Length > 50)
                            {
                                lblNomeSorteado.FontSize = 34;
                            }
                            else
                            {
                                lblNomeSorteado.FontSize = 38;
                            }
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(txtLogSorteio.Text))
                        {
                            txtLogSorteio.AppendText(Environment.NewLine);
                        }
                        txtLogSorteio.AppendText(value);
                        txtLogSorteio.ScrollToEnd();
                    }
                });

                if (Service.SortearProximaLista(updateStatus, updateProgress, logText, sementePersonalizada))
                {
                    DateTime momento = DateTime.Now;
                    DateTime momentoFinal = DateTime.Now.AddMilliseconds(30);
                    while (momento < momentoFinal)
                    {
                        momento = DateTime.Now;
                    }

                    Dispatcher.Invoke(() => { lblNomeSorteado.Content = "";  txtLogSorteio.Clear();  txtSementePersonalizada.Text = ""; } );
                    processing = false;
                }
                Dispatcher.Invoke(() => EtapaSorteio(true));
            };
            worker.RunWorkerAsync();
        }

        private void btnSalvarLista_Click(object sender, RoutedEventArgs e) {

            Lista lista = ((sender as Button).Parent as Grid).DataContext as Lista;

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = lista.Nome;
            saveDialog.DefaultExt = ".pdf";
            saveDialog.Filter = "PDF files (.pdf)|*.pdf";

            if (saveDialog.ShowDialog() == true) {
                Service.SalvarLista(lista, saveDialog.FileName);
            }
        }

        /* Etapa de Sorteio Finalizado. */

        private void Button_Click(object sender, RoutedEventArgs e) {
            string pastaResultado = RegistrarPastaResultado();
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = "Sorteados_Titulares";
            saveDialog.DefaultExt = ".pdf";
            saveDialog.Filter = "PDF files (.pdf)|*.pdf";

            if (saveDialog.ShowDialog() == true)
            {
                Service.SalvarSorteados(saveDialog.FileName);
            }

            btnRecuarFinalizado.IsEnabled = false;
            btnExportarListas.IsEnabled = false;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (wSender, wE) => {

                processing = true;

                Action<string> updateStatus = (value) => Dispatcher.Invoke(() => { lblStatusExportacao.Content = value; });

                try {
                    Service.ExportarListas(updateStatus, pastaResultado);
                    Service.ExportarListas(updateStatus);
                    updateStatus("Exportação finalizada.");
                } catch (Exception exception) {
                    ShowErrorMessage($"Erro na exportação: {exception.Message}");
                    updateStatus("Erro na exportação.");
                }

                Dispatcher.Invoke(() => EtapaFinalizado(true));
                processing = false;
            };
            worker.RunWorkerAsync();
        }

        /* Publicação */

        private void btnAbrirDiretorioExportacao_Click(object sender, RoutedEventArgs e) {
            Process.Start(new ProcessStartInfo() {
                FileName = Service.DiretorioExportacaoCSV,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private String NomearLista(string empreendimento, string faixa, string percentual, int rendaMinima, int rendaMaxima)
        {
            return String.Concat(empreendimento, " - Faixa ", faixa, " - ", percentual, "% de subsídio - ", rendaMinima, " a ", rendaMaxima, " salários mínimos");
        }

        private string RegistrarPastaResultado()
        {
            string caminhoPastaResultado = System.Configuration.ConfigurationManager.AppSettings["PASTA_RESULTADO"];
            if (String.IsNullOrWhiteSpace(caminhoPastaResultado))
            {
                string[] divisorPasta = { "\\" };
                string[] caminhoArquivoEntrada = txtFaixaA.Text.Split(divisorPasta, StringSplitOptions.None);
                caminhoArquivoEntrada[caminhoArquivoEntrada.Count() - 1] = "";
                caminhoPastaResultado = String.Join(divisorPasta[0], caminhoArquivoEntrada);
                System.Configuration.ConfigurationManager.AppSettings.Set("PASTA_RESULTADO", caminhoPastaResultado);
            }
            return caminhoPastaResultado;
        }

        private void visualizarProximoEmpreendimento(object sender, RoutedEventArgs e)
        {
            int empreendimento = Int32.Parse(((TextBox)sender).Name.Split('o')[1]);

            switch (empreendimento)
            {
                case 1:
                    chkBoxEmpreendimento2.Visibility = Visibility.Visible;
                    break;
                case 2:
                    chkBoxEmpreendimento3.Visibility = Visibility.Visible;
                    break;
                case 3:
                    chkBoxEmpreendimento4.Visibility = Visibility.Visible;
                    break;
                case 4:
                    chkBoxEmpreendimento5.Visibility = Visibility.Visible;
                    break;
                case 5:
                    chkBoxEmpreendimento6.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void visualizarNomeEmpreendimento(object sender, RoutedEventArgs e)
        {
            int empreendimento = Int32.Parse(((CheckBox)sender).Name.Split('o')[2]);

            switch (empreendimento)
            {
                case 2:
                    empreendimento2.Visibility = Visibility.Visible;
                    break;
                case 3:
                    empreendimento3.Visibility = Visibility.Visible;
                    break;
                case 4:
                    empreendimento4.Visibility = Visibility.Visible;
                    break;
                case 5:
                    empreendimento5.Visibility = Visibility.Visible;
                    break;
                case 6:
                    empreendimento6.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
    }
}
