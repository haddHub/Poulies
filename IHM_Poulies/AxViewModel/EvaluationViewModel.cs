using GalaSoft.MvvmLight;
using System.Diagnostics;
using System.Windows.Threading;
using AxModel;
using AxAnalyse;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Timers;
using System;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using AxModel.Helpers;
using System.IO;
using System.Collections.Generic;
using AxModel.Message;
using System.Globalization;
using AxData;
using AxCommunication;
using AxExerciceGenerator;
using System.Linq;
using AxError;
using AxModelExercice;
using GalaSoft.MvvmLight.Ioc;
using Navegar;
using System.Xml.Linq;
using System.Threading;
namespace AxViewModel
{
    // enumeration pour la selectio d'un exo
    public enum Types
    {
        Cercle,
        Carre,
        Droite,
        Cibles,
        Mouv,
        Tonus
    }
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class EvaluationViewModel : BlankViewModelBase, IPageViewModel
    {
        #region Fields

        object locker = new object();

        public INavigation _nav;
        public IMessageBoxService _messageBoxService;

        private SingletonReeducation ValeurReeducation = SingletonReeducation.getInstance();
        private Singleton User = Singleton.getInstance();

        private List<ViewModelBase> _pagesInternes;
        private List<ViewModelBase> _pagesInternesThemes;

        private List<DataPosition> _tempsPositionXY;
        private List<DataPosition> _tempsPositionXY2;
        
        private double[] tabVitesseMain;
        private double[] tabForceRap;
        private double[] tabForceRap2;
        private List<double> listeVitesseInstant;
        private List<double> listeAngleInstant;
        private List<double> listeSatInstant;
        private List<double[]> listeForceRapXY;
        private List<double[]> listeForceRapXY2;
        private List<double> listeAnglePlateau;
        private List<ExerciceEvaluation> exoEvalList;
        private bool _enfant = false;
        private const string PatientValuePropertyName = "PatientValue";
        private ListePatientDataGrid _patientValue = null;

        private DispatcherTimer _timerDureeExercice;
        private DispatcherTimer _timerDureePause;
        private int _nbrSeconde;
        private int _nbrSecondePause;
        
        private string pathDossier;
        private string pathScreenshot;
        private System.Timers.Timer timerScreenshot;
        private ExercicePoulies _exoPoulies;
        /// <summary>
        /// Permet de savoir si il faut ou non clear les graphiques moyen.
        /// </summary>
        private bool _clearGraphMoyen = false;

        #endregion
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the EvaluationViewModel class.
        /// </summary>
        public EvaluationViewModel()
        {
            //_portSerieService = _pss;
            //_messageBoxService = _mbs;
            try
            {
                _nav = SimpleIoc.Default.GetInstance<INavigation>();
                _messageBoxService = SimpleIoc.Default.GetInstance<IMessageBoxService>();
                exoEvalList = new List<ExerciceEvaluation>(); // init exo
                //tableau des positions
                _tempsPositionXY = new List<DataPosition>();
                _tempsPositionXY2 = new List<DataPosition>();
                listeAnglePlateau = new List<double>();
                //timer pour la duree de lexercice
                _timerDureeExercice = new DispatcherTimer(DispatcherPriority.Render);
                _timerDureeExercice.Interval = TimeSpan.FromSeconds(1);
                _timerDureeExercice.Tick +=new EventHandler(_timerDureeExercice_Tick);
                //timer pour la duree de la pause
                _timerDureePause = new DispatcherTimer(DispatcherPriority.Render);
                _timerDureePause.Interval = TimeSpan.FromSeconds(1);
                _timerDureePause.Tick += new EventHandler(_timerDureePause_Tick);

                CreateCommands();       // création des commandes de la page
                CreateMessengers();     // création des messages
                InitNavigation();
                Debug.Print("EvaluationViewModel OK");
                Messenger.Default.Register<PatientMessage>(this, OnRegister);
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
            //_portSerieService.PositionDataReceived += new onPositionDataReceived(_portSerieModel_PositionDataReceived);
        }

        //ctor pour pouvoir faire des tests unitaires sur la navigation
        public EvaluationViewModel(int i)
        {

        }
        #endregion

        #region Properties
        private bool _typeCinematique = true;
        public bool TypeCinematique
        {
            get
            {
                return _typeCinematique;
            }
            set
            {
                _typeCinematique = value;
                RaisePropertyChanged("TypeCinematique");
            }
        }
        private bool _typeTest = false;
        public bool TypeTest
        {
            get
            {
                return _typeTest;
            }
            set
            {
                _typeTest = value;
                RaisePropertyChanged("TypeTest");
            }
        }

        private ViewModelBase _internView;
        public ViewModelBase InternView
        {
            get { return _internView; }
            set
            {
                if (_internView != value)
                {
                    _internView = value;
                    RaisePropertyChanged("InternView");
                }
            }
        }

        private ViewModelBase _internViewThemes;
        public ViewModelBase InternViewThemes
        {
            get { return _internViewThemes; }
            set
            {
                if (_internViewThemes != value)
                {
                    _internViewThemes = value;
                    RaisePropertyChanged("InternViewThemes");
                }
            }
        }

        public List<ViewModelBase> PagesInternes
        {
            get
            {
                if (_pagesInternes == null)
                    _pagesInternes = new List<ViewModelBase>();

                return _pagesInternes;
            }
        }

        public List<ViewModelBase> PagesInternesThemes
        {
            get
            {
                if (_pagesInternesThemes == null)
                    _pagesInternesThemes = new List<ViewModelBase>();

                return _pagesInternesThemes;
            }
        }

        public bool Enfant
        {
            get
            {
                return _enfant;
            }
            set
            {
                _enfant = value;
                RaisePropertyChanged("Enfant");
            }
        }

        public ListePatientDataGrid PatientValue
        {
            get { return _patientValue; }
            set
            {
                if (_patientValue == value)
                {
                    _patientValue = value;
                }

                RaisePropertyChanging(PatientValuePropertyName);
                _patientValue = value;
                RaisePropertyChanged(PatientValuePropertyName);

            }
        }

        private int _highScore;

        public int HighScore
        {
            get { return _highScore; }
            set 
            {
                _highScore = value;
                RaisePropertyChanged("HighScore");
            }
        }

        private int _score;

        public int Score
        {
            get { return _score; }
            set 
            {
                _score = value;
                RaisePropertyChanged("Score");
            }
        }


        #endregion

        #region MethodsNavigation

        private void InitNavigation()
        {
            PagesInternes.Add(new ExercicePouliesChoixViewModel());
            PagesInternesThemes.Add(new ThemesExercicePouliesViewModel());

            InternView = PagesInternes[0];
            InternViewThemes = PagesInternesThemes[0];
        }
        /// <summary>
        /// Méthode de création des commandes
        /// </summary>
        private void CreateCommands()
        {
            PreviousViewModelCommand = new RelayCommand(GoBack, CanGoBack);
            ChangeInternNavigation = new RelayCommand<string>((p) => changeInternView(p));
            NextViewModelCommand = new RelayCommand(NextViewModel);
            MainViewModelCommand = new RelayCommand(NavigateToHome);
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

        private bool CanGoBack()
        {
            return _nav.CanGoBack();
        }

        //Permet de revenir sur le premier ViewModel
        private void GoBack()
        {
            try
            {
                if (_nav.GetTypeViewModelToBack() == typeof(HomeViewModel))
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

        private void changeInternView(string p)
        {
            try
            {
                InternView = PagesInternes[Convert.ToInt32(p, 10)];
                InternViewThemes = PagesInternesThemes[Convert.ToInt32(p, 10)];
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        private void NextViewModel()
        {
            try
            {
                //Chercher si on doit demander des renseignement sur le ressort et la main dominanate.
                //Si oui afficher la page de choix sinon visualisation.
                Singleton singlePatient = Singleton.getInstance();
                InitEnregistrementDonneesBrute();
                string dossier = singlePatient.Patient.Nom + singlePatient.Patient.Prenom + singlePatient.Patient.DateDeNaissance.ToString().Replace("/", string.Empty);
                if (Directory.Exists("Files/Patients/" + dossier))
                {
                    _nav.NavigateTo<VisualisationViewModel>(this, null, false);
                    //Envoye false pour prevenir le viewModel que le choix du ressort avait déjà été fait
                    Messenger.Default.Send(false, "ConfigEvalCinematique");
                }
                else
                {
                    _nav.NavigateTo<ChoixRessortMainViewModel>(this, null, false);
                    //Envoye false pour prevenir le viewModel que le choix du ressort n'à pas encore été fait
                    Messenger.Default.Send(true, "ConfigEvalCinematique");

                    this.HighScore = 0;
                }
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }     
        }

        /// <summary>
        /// Initialisation des messages de la page
        /// </summary>
        private void CreateMessengers()
        {
            Messenger.Default.Register<bool>(this, "MainViewModel", StartExercice);
            Messenger.Default.Register<List<ExerciceGeneric>>(this, "RefreshListe", RemplirExoEvalList);
            Messenger.Default.Register<RobotErrorMessage>(this, "NewRobotError", OnRobotError); // effectue si il y a une erreur robot
            Messenger.Default.Register<List<ExerciceGeneric>>(this, "EvaluationViewModel", RemplirExoEvalList);
            Messenger.Default.Register<PositionDataModel>(this, "NewPosition", TraitementPosition);  // abonnement aux messages envoyé par MainViewModel pour afficher la position
            Messenger.Default.Register<Position2DataModel>(this, "NewPosition2", TraitementPosition2);
            Messenger.Default.Register<ForceRapDataModel>(this, "NewForceRap", TraitementFroceRap);
            Messenger.Default.Register<ForceRap2DataModel>(this, "NewForceRap2", TraitementFroceRap2);
            Messenger.Default.Register<AcosTDataModel>(this, "NewAcosT", TraitementAcosT);
            Messenger.Default.Register<bool>(this, "StopTimers", StopTimers);
            Messenger.Default.Register<bool>(this, "CleanDePause", CleanEcranDePause);
        }

        private void RemplirExoEvalList(List<ExerciceGeneric> listeGen)
        {
            exoEvalList.Clear();
            foreach (var exo in listeGen)
            {
                exoEvalList.Add((ExerciceEvaluation)exo);
            }
        }
        #endregion

        #region RelayCommand
        public RelayCommand PreviousViewModelCommand { get; set; }
        public RelayCommand<string> ChangeInternNavigation { get; set; }
        public RelayCommand NextViewModelCommand { get; set; }
        public RelayCommand MainViewModelCommand { get; set; }
        
        #endregion


        private void StartExercice(bool statu)
        {
            if (_clearGraphMoyen)
            {
                //Prend un screenshot de l'écran
                Messenger.Default.Send<string>(pathScreenshot, "ScrrenshotGraphiques");
            }
            _exoPoulies = this.exoEvalList[0] as ExercicePoulies;
            //Démarer le timer pour la durée de l'exercice
            if (statu)
            {
                _nbrSeconde = _exoPoulies.Duree;
                this.listeVitesseInstant = new List<double>();
                this.listeAngleInstant = new List<double>();
                this.listeSatInstant = new List<double>();
                this.listeForceRapXY = new List<double[]>();
                this.listeForceRapXY2 = new List<double[]>();

                //Clear les graphiques instant.
                Messenger.Default.Send<bool>(true, "ClearGraphiquesInstant");

                //si le type de l'exercice suivant n'est pas le meme, clean les graph et reset le high score.
                if (_clearGraphMoyen == true)
                {
                    Messenger.Default.Send<bool>(true, "ClearGraphiquesMoyen");
                    this.HighScore = 0;
                    _clearGraphMoyen = false;
                }

                this._timerDureeExercice.Start();
            }
            //Exercice terminé
            else
            {
                //Déroulement normale
                if (_nbrSeconde == 0)
                {
                    //Une pause et un start automatique
                    if (_exoPoulies.TempsDePause > 0)
                    {
                        Messenger.Default.Send<bool>(true, "PoulieEnPause");
                        _nbrSecondePause = _exoPoulies.TempsDePause;
                        int[] tabPause = new int[] { _nbrSecondePause, this.Score, this.HighScore };
                        //Envoie le nombre de seconde restant avant la fin de la pause au MainPViewModel
                        Messenger.Default.Send<int[]>(tabPause, "PoulieEnPauseGui");

                        this._timerDureePause.Start();
                        //Afficher l'exercice d'apres pendant la pause
                        NextExercice();
                    }
                    else
                    {
                        //passer a lexercice suivant sans le lancer.
                        //La pause est manuel. En attendant que le thérapeute lance demarre l'exercice suivant, l'ecran de pause apparait pour affichier le score
                        //du dernier exercice du bloc.
                        int[] tabPause = new int[] { -1, this.Score, this.HighScore };
                        Messenger.Default.Send<int[]>(tabPause, "PoulieEnPauseGui");
                        NextExercice();
                    }
                }
                //Le thérapeute a pousse sur stop en exercice
                else
                {
                    this._timerDureeExercice.Stop();
                    this._timerDureePause.Stop();
                }
                _tempsPositionXY.Clear();
                _tempsPositionXY2.Clear();
            }
        }

        //Durant l'exercice.
        private void _timerDureeExercice_Tick(object sender, EventArgs e)
        {
            //Décrement le timer général de l'exercice.
            _nbrSeconde--;

            //Envoye de la vitesse de monté du plateau instantannée et le temps en seconde au VisualisaionViewModel pour l'afficher dans le graph.
            double vitesseInstan = Math.Max(tabVitesseMain[0], tabVitesseMain[1]);
            double[] vitesseTemps = new double[2] { (this._exoPoulies.Duree - _nbrSeconde), vitesseInstan };
            Messenger.Default.Send<double[]>(vitesseTemps, "VitesseInstanPlateau");

            //Envoye de l'angle du plateau instantannée et le temps en seconde au VisualisaionViewModel pour l'afficher dans le graph.
            double[] angleTemps = new double[2] { (this._exoPoulies.Duree - _nbrSeconde), this._exoPoulies.Plats[1].Angle };
            Messenger.Default.Send<double[]>(angleTemps, "AngleInstanPlateau");

            //Envoye du Sat instantannée et le temps en seconde au VisualisaionViewModel pour l'afficher dans le graph.
            double[] satTemps = new double[2] { (this._exoPoulies.Duree - _nbrSeconde), CalculeDuSat(vitesseInstan) };
            Messenger.Default.Send<double[]>(satTemps, "SatInstanPlateau");

            //Exercice terminé
            if (_nbrSeconde <= 0)
            {
                this._timerDureeExercice.Stop();

                Messenger.Default.Send<string>("", "StopRobot");

                //Calcule de la vitesse moyenne pour l'exercice qui vient de se finir.
                double vitesseMoyenneExercice = 0;
                foreach (double vitesse in this.listeVitesseInstant)
                {
                    vitesseMoyenneExercice += vitesse;
                }
                //Envoye la vitesse moyenne et le numéro de l'exercice.
                double[] vitesseMoyenneTemps = new double[2] { this._exoPoulies.NumeroExercice, (vitesseMoyenneExercice / this.listeVitesseInstant.Count) };
                Messenger.Default.Send<double[]>(vitesseMoyenneTemps, "VitesseMoyennePlateau");

                //Calcule de l'angle moyenne pour l'exercice qui vient de se finir.
                double angleMoyenExercice = 0;
                foreach (double angle in listeAngleInstant)
                {
                    angleMoyenExercice += Math.Abs(angle);
                }
                //Envoye l'angle moyen et le numéro de l'exercice.
                double[] angleMoyenTemps = new double[2] { this._exoPoulies.NumeroExercice, (angleMoyenExercice / this.listeAngleInstant.Count) };
                Messenger.Default.Send<double[]>(angleMoyenTemps, "AngleMoyenPlateau");
                
                //Calcule du sat moyen pour l'exercice qui vient de se finir.
                double satMoyenExercice = 0;
                foreach (double sat in listeSatInstant)
                {
                    satMoyenExercice += sat;
                }
                //Le Sat moyen coresspond au score de l'exercice acutel.
                this.Score = (int)satMoyenExercice / this.listeSatInstant.Count;
                if (this.Score > this.HighScore)
                    this.HighScore = this.Score;
                //Envoye du Sat moyen et le numéro de l'exercice.
                double[] satMoyenTemps = new double[2] { this._exoPoulies.NumeroExercice, (satMoyenExercice / this.listeSatInstant.Count) };
                Messenger.Default.Send<double[]>(satMoyenTemps, "SatMoyenPlateau");

                //Enregistre l'ensemble des données dans les fichier text avant le reset des liste de données instantnées.
                EnregistrementDonneesBrute(this.listeVitesseInstant, this.listeAngleInstant, this.listeSatInstant, this.listeForceRapXY, this.listeForceRapXY2);

                //fait un screenshot pour le tout dernier exo sans devoir cliquer sur démarrer.
                if (this.exoEvalList.Count == 1)
                {
                    timerScreenshot = new System.Timers.Timer(3000);
                    timerScreenshot.Elapsed += new ElapsedEventHandler(timerScreenshot_Elapsed);
                    timerScreenshot.Start();
                }
            }
        }

        void timerScreenshot_Elapsed(object sender, ElapsedEventArgs e)
        {
            Messenger.Default.Send<string>(pathScreenshot, "ScrrenshotGraphiques");
            timerScreenshot.Stop();
        }

        private void _timerDureePause_Tick(object sender, EventArgs e)
        {
            _nbrSecondePause--;
            //Envoie le nombre de seconde restant avant la fin de la pause au MainPViewModel
            int[] tabPause = new int[] { _nbrSecondePause, this.Score, this.HighScore };
            Messenger.Default.Send<int[]>(tabPause, "PoulieEnPauseGui");
            //Pause terminée
            if (_nbrSecondePause <= 0)
            {
                this._timerDureePause.Stop();
                //Messenger.Default.Send<bool>(false, "PoulieEnPause");

                //Envoie le nombre de seconde restant avant la fin de la pause (0) le score et le highScore au MainPViewModel
                Messenger.Default.Send<int[]>(tabPause, "PoulieEnPauseGui");
                //Relancer le prochain exo.
                Messenger.Default.Send<bool>(true, "StartExerciceAutomatique");
            }
        }

        private void NextExercice()
        {
            if (this.exoEvalList.Count != 0)
            {
                if (this.exoEvalList.Count >= 2)
                {
                    //Adapter la config d'interaction de l'exercice suivant si Ressort et Entrainement
                    ExercicePoulies _exoPoulies = this.exoEvalList[0] as ExercicePoulies;
                    ExercicePoulies _exoPouliesSuivant = this.exoEvalList[1] as ExercicePoulies;
                    if (_exoPoulies.Ressort == TypeRessort.AvecRessort && _exoPoulies.TypeExercice == TypeExercicePoulies.Entrainement && _exoPouliesSuivant.TypeExercice == TypeExercicePoulies.Entrainement)
                    {
                        double newValue = (_exoPoulies.ConfigInteraction.KInteractR - (210 / 14));
                        if(newValue < 0.0)
                            newValue = 0.0;
                        byte valeurK = (byte)newValue;
                        if (valeurK < 0)
                            valeurK = 0;

                        byte valeurC = 50;

                        _exoPouliesSuivant.ActualiserConfig(valeurK, valeurC);
                    }

                    if (_exoPoulies.TypeExercice != _exoPouliesSuivant.TypeExercice)
                        _clearGraphMoyen = true;
                    else
                        _clearGraphMoyen = false;
                }

                this.exoEvalList.RemoveAt(0);
                if (this.exoEvalList.Count > 0)
                {
                    List<ExerciceGeneric> listeGen = new List<ExerciceGeneric>();
                    foreach (var item in exoEvalList)
                    {
                        listeGen.Add(item);
                    }
                   Messenger.Default.Send<List<ExerciceGeneric>>(listeGen, "NextExercice");
                }
                else
                {
                    //Fin de la séance.
                    //Bloque le demarage et fin de la séance.
                    Messenger.Default.Send<bool>(true, "BloquerStart");
                    //k pour dire qu'il n'y a plus d'exercice dans la liste.
                    Messenger.Default.Send<string>("k", "StopRobot");
                }
            }
        }

        //Stop les timers et clear les listes de positions.
        private void StopTimers(bool b)
        {
            this._timerDureeExercice.Stop();
            this._timerDureePause.Stop();
            this._tempsPositionXY.Clear();
            this._tempsPositionXY2.Clear();
        }

        private void OnRegister(PatientMessage obj)
        {
            try
            {
                PatientValue = obj.DataPatient;
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        private void OnRobotError(RobotErrorMessage e)
        {
            if (_nav.GetTypeViewModelToBack() == typeof(HomeViewModel))
                NavigateToHome();
        }

        public void TraitementPosition(PositionDataModel newPosition)   // TODO : à changer !
        {
            try
            {
                this._tempsPositionXY.Add(new DataPosition(newPosition.PositionX, newPosition.PositionY));
                if (_tempsPositionXY.Count > 0 && _tempsPositionXY2.Count > 0)
                {
                    if (_tempsPositionXY.Count >= 2 && _tempsPositionXY2.Count >= 2)
                        CalculVitesse();
                }
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        public void TraitementPosition2(Position2DataModel newPosition)   // TODO : à changer !
        {
            try
            {
                this._tempsPositionXY2.Add(new DataPosition(newPosition.PositionX, newPosition.PositionY));
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        public void TraitementFroceRap(ForceRapDataModel newForceRap)
        {
            try
            {
                this.tabForceRap = new double[2];
                this.tabForceRap[0] = newForceRap.ForceToNewtonX();
                this.tabForceRap[1] = newForceRap.ForceToNewtonY();
                //Ajout des couples de force de rapel dans une liste.
                this.listeForceRapXY.Add(this.tabForceRap);
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        public void TraitementFroceRap2(ForceRap2DataModel newForceRap2)
        {
            try
            {
                this.tabForceRap2 = new double[2];
                this.tabForceRap2[0] = newForceRap2.ForceToNewtonX();
                this.tabForceRap2[1] = newForceRap2.ForceToNewtonY();
                //Ajout des couples de force de rapel dans une liste.
                this.listeForceRapXY2.Add(this.tabForceRap2);
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        public void TraitementAcosT(AcosTDataModel newAcosT)
        {
            try
            {
                this.listeAnglePlateau.Add(Math.Round(newAcosT.AcosDegre(),2));
                //envoye l'angle au plateau tous les x points.
                if (this.listeAnglePlateau.Count >= 1)
                {
                    double angleMoyen = 0;
                    foreach (double item in listeAnglePlateau)
                    {
                        angleMoyen += item;
                    }
                    angleMoyen /= listeAnglePlateau.Count;
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                         Messenger.Default.Send(Math.Round(angleMoyen,2), "FaireTournerPlateau");
                    }), DispatcherPriority.Normal);
                    this.listeAnglePlateau.Clear();
                    listeAngleInstant.Add(this._exoPoulies.Plats[1].Angle);
                }
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        private void CalculVitesse()
        {
            tabVitesseMain = new double[2];
            double preakV = 0;
            if (_tempsPositionXY.Count != 0 && _tempsPositionXY2.Count != 0)
            {
                //calculer vitesse pour les deux mains en fonction de _tempsDatapositionXY et 2.
                tabVitesseMain[0] = Ax_Vitesse.VitesseMoy(_tempsPositionXY, ref preakV, 0.00625);
                tabVitesseMain[1] = Ax_Vitesse.VitesseMoy(_tempsPositionXY2, ref preakV, 0.00625);
                //transformation des vitesse centieme de m/s en cm/s.
                tabVitesseMain[0] /= 100.0;
                tabVitesseMain[1] /= 100.0;

                //Envoyer la vitesse au mainPViewModel pour faire bouger les objets.
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Messenger.Default.Send(tabVitesseMain, "FaireMonterPlateau");
                }), DispatcherPriority.Normal);
                this._tempsPositionXY.Clear();
                this._tempsPositionXY2.Clear();
                //40hertz
                listeVitesseInstant.Add(Math.Max(tabVitesseMain[0],tabVitesseMain[1]));
                //Ajout le SAT dans la liste avec la nouvelle vitesse instant
                listeSatInstant.Add(CalculeDuSat(this.listeVitesseInstant.Last()));
            }
        }

        private double CalculeDuSat(double vitesse)
        {
            double angle = Math.Abs(this._exoPoulies.Plats[1].Angle);
            if (angle < 1)
                angle = 1;

            double sat;
            if (this._exoPoulies.TypeExercice == TypeExercicePoulies.TestVitesseContrainte)
            {
                //Sat = (angle actuel / angle max)
                sat = (angle / 30.0);
                //renverse le score pour maintenir la logique.
                sat = 1 - sat;
                //*100 pour ramener en %
                sat *= 100;
            }
            else
            {
                //Sat = (vitesse actuel / vitesse max) / (angle actuel / angle max)
                sat = (vitesse / 35.0) / (angle / 30.0);
                //*100/30 pour ramener en %
                sat *= 100 / 30;
            }
            return sat;
        }

        private void CleanEcranDePause(bool b)
        {
            if (b)
            {
                //Clean l'ecran de pause dans le cas ou la pause n'est pas automatique.
                int[] tabPause = new int[] { 0, 0, 0 };
                Messenger.Default.Send<int[]>(tabPause, "PoulieEnPauseGui"); 
            }
        }

        private void InitEnregistrementDonneesBrute()
        {
            Singleton singlePatient = Singleton.getInstance();
            pathDossier = "Files/Patients/" + singlePatient.Patient.Nom + singlePatient.Patient.Prenom + singlePatient.Patient.DateDeNaissance.ToString().Replace("/", string.Empty);
        }

        private void EnregistrementDonneesBrute(List<double> vitesses, List<double> angles, List<double> sats, List<double[]> forceRap, List<double[]> forceRap2)
        {
            TextWriter tw;
            if (this._exoPoulies.TypeExercice == TypeExercicePoulies.TestVitesseContrainte || this._exoPoulies.TypeExercice == TypeExercicePoulies.TestVitesseLibre)
            {
                //Si le dossier reserver aux tests n'existe pas encore, on le crée.
                if (!Directory.Exists(pathDossier + "/Tests"))
                {
                    Directory.CreateDirectory(pathDossier + "/Tests");
                    File.Create(pathDossier + "/Tests/vitesseLibre.txt").Close();
                    File.Create(pathDossier + "/Tests/vitesseImposee.txt").Close();
                }

                if (this._exoPoulies.TypeExercice == TypeExercicePoulies.TestVitesseContrainte)
                {
                    //ouvre le fichier.
                    tw = new StreamWriter(pathDossier + "/Tests/vitesseImposee.txt", true);
                    pathScreenshot = pathDossier + "/Tests/vitesseImposeeGraphiques";
                    
                    //Precise la vitesse seulement quand elle change.
                    if (this._exoPoulies.NumeroExercice == 1 || this._exoPoulies.NumeroExercice == 6 || this._exoPoulies.NumeroExercice == 11)
                        tw.WriteLine("Vitesse Imposee : {0} cm/s", this._exoPoulies.VitesseContrainte);
                }
                else
                {
                    //ouvre le fichier.
                    tw = new StreamWriter(pathDossier + "/Tests/vitesseLibre.txt", true);
                    pathScreenshot = pathDossier + "/Tests/vitesseLibreGraphiques";
                }
            }
            else
            {
                //Si le dossier reserver aux tests n'existe pas encore, on le crée.
                if (!Directory.Exists(pathDossier + "/Entrainements"))
                {
                    Directory.CreateDirectory(pathDossier + "/Entrainements");
                    File.Create(pathDossier + "/Entrainements/entrainement.txt").Close();
                }

                tw = new StreamWriter(pathDossier + "/Entrainements/entrainement.txt", true);
                pathScreenshot = pathDossier + "/Entrainements/entrainementGraphiques";
            }

            if (File.Exists(pathScreenshot + ".jpg"))
            {
                pathScreenshot += "_2";
                if (File.Exists(pathScreenshot + ".jpg"))
                {
                    pathScreenshot = pathScreenshot.Remove(pathScreenshot.Length - 1, 1);
                    pathScreenshot += "3";
                    if (File.Exists(pathScreenshot + ".jpg"))
                    {
                        pathScreenshot = pathScreenshot.Remove(pathScreenshot.Length - 1, 1);
                        pathScreenshot += "4";
                    }
                }
            }

            tw.WriteLine("Exercice numero : {0}", this._exoPoulies.NumeroExercice);
            tw.WriteLine("---------------------------");
            tw.WriteLine("");

            tw.WriteLine("Vitesses : ");
            foreach (double vit in vitesses)
            {
                tw.Write(vit.ToString() + " ");
            }
            tw.WriteLine("");
            tw.WriteLine("Angles : ");
            foreach (double ang in angles)
            {
                tw.Write(Math.Abs(ang).ToString() + " ");
            }
            tw.WriteLine("");
            tw.WriteLine("SAT : ");
            foreach (double sat in sats)
            {
                tw.Write(sat.ToString() + " ");
            }
            tw.WriteLine("");
            //Ecrit les forces d'interactions et l'assistance seulement si possibilité d'en avoire une.
            if (this._exoPoulies.TypeExercice == TypeExercicePoulies.Entrainement)
            {
                if (this._exoPoulies.Ressort == TypeRessort.AvecRessort)
                {
                    tw.WriteLine("Interactions main gauche: ");

                    foreach (var forces in forceRap)
                    {
                        tw.WriteLine("X : {0}  Y : {1} ", forces[0], forces[1]);
                    }
                    tw.WriteLine("");
                    tw.WriteLine("Interactions main droite: ");

                    foreach (var forces in forceRap2)
                    {
                        tw.WriteLine("X : {0}  Y : {1} ", forces[0], forces[1]);
                    } 
                }
                tw.WriteLine("");
                tw.WriteLine("Assistance : {0} %", this._exoPoulies.PourcentageAssistance);
            }
            tw.WriteLine("");
            tw.WriteLine("");
            tw.Close();
            //clear des listes de données
            this.listeSatInstant.Clear();
            this.listeForceRapXY.Clear();
            this.listeForceRapXY2.Clear();
            this.listeAngleInstant.Clear();
            this.listeVitesseInstant.Clear();
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
        }
    }
}