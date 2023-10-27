﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Habitasorte.Business.Model {
    public class Sorteio : IDataErrorInfo, INotifyPropertyChanged {

        private string nome;
        private string statusSorteio;
        private string faixaA;
        private bool faixaAAtivo;
        private string faixaB;
        private bool faixaBAtivo;
        private string faixaC;
        private bool faixaCAtivo;
        private string faixaD;
        private bool faixaDAtivo;
        private string empreendimento1;
        private bool empreendimento1Ativo;
        private string empreendimento2;
        private bool empreendimento2Ativo;
        private string empreendimento3;
        private bool empreendimento3Ativo;
        private string empreendimento4;
        private bool empreendimento4Ativo;
        private string empreendimento5;
        private bool empreendimento5Ativo;
        private string empreendimento6;
        private bool empreendimento6Ativo;
        private ICollection<Lista> listas;
        private Lista proximaLista;

        public Sorteio() {
            listas = new List<Lista>();
        }

        public string Nome {
            get { return nome; }
            set { SetField(ref nome, value); }
        }

        public string StatusSorteio {
            get { return statusSorteio; }
            set { SetField(ref statusSorteio, value); }
        }

        public string FaixaA {
            get { return faixaA; }
            set { SetField(ref faixaA, value); }
        }

        public bool FaixaAAtivo {
            get { return faixaAAtivo; }
            set {
                SetField(ref faixaAAtivo, value);
                if (!value) {
                    faixaA = null;
                }
                NotifyPropertyChanged("FaixaA");
            }
        }

        public string FaixaB {
            get { return faixaB; }
            set { SetField(ref faixaB, value); }
        }

        public bool FaixaBAtivo {
            get { return faixaBAtivo; }
            set {
                SetField(ref faixaBAtivo, value);
                if (!value) {
                    faixaB = null;
                }
                NotifyPropertyChanged("FaixaB");
            }
        }

        public string FaixaC {
            get { return faixaC; }
            set { SetField(ref faixaC, value); }
        }

        public bool FaixaCAtivo {
            get { return faixaCAtivo; }
            set {
                SetField(ref faixaCAtivo, value);
                if (!value) {
                    faixaC = null;
                }
                NotifyPropertyChanged("FaixaC");
            }
        }

        public string FaixaD {
            get { return faixaD; }
            set { SetField(ref faixaD, value); }
        }

        public bool FaixaDAtivo
        {
            get { return faixaDAtivo; }
            set
            {
                SetField(ref faixaDAtivo, value);
                if (!value)
                {
                    faixaD = null;
                }
                NotifyPropertyChanged("FaixaD");
            }
        }

        public string Empreendimento1
        {
            get { return empreendimento1; }
            set { SetField(ref empreendimento1, value); }
        }

        public bool Empreendimento1Ativo
        {
            get { return empreendimento1Ativo; }
            set
            {
                SetField(ref empreendimento1Ativo, value);
                if (!value)
                {
                    empreendimento1 = null;
                }
                NotifyPropertyChanged("Empreendimento1");
            }
        }

        public string Empreendimento2
        {
            get { return empreendimento2; }
            set { SetField(ref empreendimento2, value); }
        }

        public bool Empreendimento2Ativo
        {
            get { return empreendimento2Ativo; }
            set
            {
                SetField(ref empreendimento2Ativo, value);
                if (!value)
                {
                    empreendimento2 = null;
                }
                NotifyPropertyChanged("Empreendimento2");
            }
        }

        public string Empreendimento3
        {
            get { return empreendimento3; }
            set { SetField(ref empreendimento3, value); }
        }

        public bool Empreendimento3Ativo
        {
            get { return empreendimento3Ativo; }
            set
            {
                SetField(ref empreendimento3Ativo, value);
                if (!value)
                {
                    empreendimento3 = null;
                }
                NotifyPropertyChanged("Empreendimento3");
            }
        }

        public string Empreendimento4
        {
            get { return empreendimento4; }
            set { SetField(ref empreendimento4, value); }
        }

        public bool Empreendimento4Ativo
        {
            get { return empreendimento4Ativo; }
            set
            {
                SetField(ref empreendimento4Ativo, value);
                if (!value)
                {
                    empreendimento4 = null;
                }
                NotifyPropertyChanged("Empreendimento4");
            }
        }

        public string Empreendimento5
        {
            get { return empreendimento5; }
            set { SetField(ref empreendimento5, value); }
        }

        public bool Empreendimento5Ativo
        {
            get { return empreendimento5Ativo; }
            set
            {
                SetField(ref empreendimento5Ativo, value);
                if (!value)
                {
                    empreendimento5 = null;
                }
                NotifyPropertyChanged("Empreendimento5");
            }
        }

        public string Empreendimento6
        {
            get { return empreendimento6; }
            set { SetField(ref empreendimento6, value); }
        }

        public bool Empreendimento6Ativo
        {
            get { return empreendimento6Ativo; }
            set
            {
                SetField(ref empreendimento6Ativo, value);
                if (!value)
                {
                    empreendimento6 = null;
                }
                NotifyPropertyChanged("Empreendimento6");
            }
        }

        public ICollection<Lista> Listas {
            get { return listas; }
            set {
                value.ToList().ForEach(l => l.Sorteio = this);
                SetField(ref listas, value);
                NotifyPropertyChanged("TotalVagasTitulares");
                NotifyPropertyChanged("TotalVagasReserva");
                NotifyPropertyChanged("TotalVagas");
            }
        }

         public Lista ProximaLista {
            get { return proximaLista; }
            set {
                SetField(ref proximaLista, value);
            }
        }

        public int? TotalVagasTitulares => listas.Where(l => !l.Nome.ToUpper().Contains("RESERVA")).Sum(l => l.Quantidade);
        public int? TotalVagasReserva => listas.Where(l => l.Nome.ToUpper().Contains("RESERVA")).Sum(l => l.Quantidade);
        public int? TotalVagas => listas.Sum(l => l.Quantidade);

        /* INotifyPropertyChanged */

        #region INotifyPropertyChanged

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            }
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        public void NotifyPropertyChanged(string property) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        /* IDataErrorInfo */

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error { get { return null; } }

        string IDataErrorInfo.this[string columnName] { get {
            if (columnName == "Nome" && string.IsNullOrWhiteSpace(Nome)) return "Nome inválido!";
            if (columnName == "FaixaA" && string.IsNullOrWhiteSpace(FaixaA)) return "Faixa A inválida!";
            if (columnName == "FaixaB" && FaixaAAtivo && string.IsNullOrWhiteSpace(FaixaB)) return "Faixa B inválida!";
            if (columnName == "FaixaC" && FaixaBAtivo && string.IsNullOrWhiteSpace(FaixaC)) return "Faixa C inválida!";
            if (columnName == "FaixaD" && FaixaCAtivo && string.IsNullOrWhiteSpace(FaixaD)) return "Faixa D inválida!";
            return null;
        }}

        public bool IsValid { get {
            IDataErrorInfo errorInfo = this as IDataErrorInfo;
            return errorInfo["Nome"] == null
                && errorInfo["FaixaA"] == null
                && errorInfo["FaixaB"] == null
                && errorInfo["FaixaC"] == null
                && errorInfo["FaixaD"] == null;
        }}

        #endregion
    }
}
