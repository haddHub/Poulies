using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using AxModelExercice;
using Navegar;
using GalaSoft.MvvmLight.Ioc;
using AxError;
using GalaSoft.MvvmLight.Command;
using AxModel;
using System.IO;
using System.Xml.Linq;

namespace AxViewModel
{
    public class ChoixRessortMainViewModel : ViewModelBase
    {
        public bool AvecRessort { get; set; }
        public bool SansRessort { get; set; }
        public bool MainDominanteGauche { get; set; }
        public bool MainDominanteDroite { get; set; }
        public bool MainParetiqueGauche { get; set; }
        public bool MainParetiqueDroite { get; set; }

        public RelayCommand PreviousViewModelCommand { get; set; }
        public RelayCommand NextViewModelCommand { get; set; }
        public RelayCommand MainViewModelCommand { get; set; }

        private List<ExerciceGeneric> _listeGen;
        public INavigation _nav;
        public IMessageBoxService _messageBoxService;

        public ChoixRessortMainViewModel()
        {
            _nav = SimpleIoc.Default.GetInstance<INavigation>();
            _messageBoxService = SimpleIoc.Default.GetInstance<IMessageBoxService>();

            PreviousViewModelCommand = new RelayCommand(GoBack, CanGoBack);
            NextViewModelCommand = new RelayCommand(NextViewModel,NextViewModel_CanExecute);
            MainViewModelCommand = new RelayCommand(NavigateToHome);

            Messenger.Default.Register<List<ExerciceGeneric>>(this, "ChoixRessortMainViewModel", Config);
        }

        private void Config(List<ExerciceGeneric> liste)
        {
            _listeGen = new List<ExerciceGeneric>(liste);
        }

        #region Navigation

        private void GoBack()
        {
            try
            {
                if (_nav.GetTypeViewModelToBack() == typeof(EvaluationViewModel))
                {
                    _nav.GoBack("SetIsRetour", new object[] { true });
                }
                else
                {
                    _nav.GoBack();
                }
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        private bool CanGoBack()
        {
            return _nav.CanGoBack();
        }

        public void NavigateToHome()
        {
            try
            {
                if (_messageBoxService.ShowYesNo(AxLanguage.Languages.REAplan_Accueil_Confirmation, CustomDialogIcons.Question) == CustomDialogResults.Yes)
                {
                    _nav.NavigateTo<HomeViewModel>(false);
                }
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        private void NextViewModel()
        {
            EnregisterLesChoix();
            ModifierListe();
            _nav.NavigateTo<VisualisationViewModel>(this, null, false);
            Messenger.Default.Send(_listeGen, "EvaluationViewModel");
        }

        private bool NextViewModel_CanExecute()
        {
            if ((AvecRessort == false && SansRessort == false) || (MainDominanteGauche == false && MainDominanteDroite == false) || (MainParetiqueGauche == false && MainParetiqueDroite == false))
                return false;
            else
                return true;
        }

        #endregion

        private void EnregisterLesChoix()
        {
            Singleton singlePatient = Singleton.getInstance();
            string dossier = singlePatient.Patient.Nom + singlePatient.Patient.Prenom + singlePatient.Patient.DateDeNaissance.ToString().Replace("/", string.Empty);
            Directory.CreateDirectory("Files/Patients/" + dossier);

            XDocument doc = new XDocument(
                new XDeclaration("1.0", "UTF-16", null),
                new XElement("Info",
                    new XElement("Ressort",AvecRessort),
                    new XElement("MainDominanteGauche",MainDominanteGauche),
                    new XElement("MainParetiqueGauche", MainDominanteGauche)));
            doc.Save("Files/Patients/" + dossier + "/infoPatient.xml");
        }

        private void ModifierListe()
        {
            foreach (var ex in _listeGen)
            {
                ExercicePoulies exoRessort = ex as ExercicePoulies;
                if (AvecRessort)
                    exoRessort.Ressort = TypeRessort.AvecRessort;
                else
                    exoRessort.Ressort = TypeRessort.SansRessort;
            }
        }
    }
}
