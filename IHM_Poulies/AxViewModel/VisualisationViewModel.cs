using GalaSoft.MvvmLight;
using System.Diagnostics;
using AxModel;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Media;
using System.Windows;
using System.Collections.Concurrent;
using AxAnalyse;
using System.Timers;
using System;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.IO;
using AxAction;
using AxCommunication;
using AxError;
using AxModelExercice;
using AxModel.Message;
using GalaSoft.MvvmLight.Ioc;
using Navegar;
using AxData;
using AxModel.Helpers;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
namespace AxViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class VisualisationViewModel : BlankViewModelBase, IPageViewModel
    {
        #region Fields

        object locker = new object();

        
        private ObservableCollection<ExerciceEvaluation> _exoEvalList = new ObservableCollection<ExerciceEvaluation>();
        private ObservableCollection<ExerciceReeducation> _exoReedList = new ObservableCollection<ExerciceReeducation>();


        public ObservableCollection<ExerciceEvaluation> ExoEvalList
        {
            get { return _exoEvalList; }
            set {
                if (_exoEvalList != value)
                {
                    _exoEvalList = value;
                    RaisePropertyChanged("ExoEvalList");
                }

            }
        }
        public ObservableCollection<ExerciceReeducation> ExoReedList
        {
            get { return _exoReedList; }
            set
            {
                if (_exoReedList != value)
                {
                    _exoReedList = value;
                    RaisePropertyChanged("ExoReedList");
                }

            }
        }
        
        private SingletonReeducation ValeurReeducation = SingletonReeducation.getInstance();

        private bool _acqHz;
        private static System.Timers.Timer _acq100Hz;   // Timer acquisition à 100hz
        private DispatcherTimer timerSave;              // le timer Enregistrement

        private static int _nbrsPassage = 0;
        private double[] tabDistRelle = new double[3];      // Résultat distance réelle pour 3
        private double[] tabVitesseMoy = new double[3];     // Résultat vitesse moyenne pour 3
        private double[] tabDebutTraj = new double[2];      // debut et fin de traj
        private double[] tabFinTraj = new double[2];
        private bool _initPosition = false;
        private bool _initPositionEnd = false;
        private bool _ack_end_ok = false;
        private bool _ack_end = false;              // fin de traj
        private bool _calc_ack_end = false;         // calc fin de traj
        private bool _detectStart = false;          // detection demarrage
        private bool _initdetectstart = false;
        private bool _initdetectblock = false;
        private bool _detectionDem = false;         // activer / désactiver la détection de start
        private bool _initActivated = false;
        private bool _save = true;                  // Enregistrement
        private bool _activOF = false;              // Enregistrement
        private bool test = false;                  // Enregistrement
        private bool posforce = false;
        private bool _pause = true;                 // enable button
        private bool _detectionBlockA = false;
        private bool actif = false;
        private bool pause = false;

        private string _filename;                   // Chemin enregistrement
        private string text;                        // Chemin enregistrement

        private double _initPatient;
        private int _initp;
        private int _tempsInitMouvement;            // temps passé dans l'init au debut d'un mouvement
        private double _currentForceX, _currentForceY;
        private double _currentForce2X, _currentForce2Y;
        private PositionDataModel _currentPosition;
        private DataPosition tempsPositionXY;
        private PositionDataModel initdetextstart;
        private PositionDataModel initdetextblock;

        private double _resultDist;             // Résultat distance réelle
        private double _resultStra;             // Résultat Straightness
        private double _resultVit;              // Résultat Vitesse Réel
        private double _resultVitP;             // Résultat Vitesse Peak
        private double _resultSmoth;            // Résultat Smoothness
        private double _detectStartAmp = 2.0;
        private double _detectBlockAmp = 0.15;  // 0.15
        private double tempsStra = 0.0;
        private DataPosition[] debut = new DataPosition[3];
        private DataPosition[] fin = new DataPosition[3];

        private ExerciceBaseConfigViewModel _currentConfig;
        private ExerciceBaseConfigViewModel _TempsCurrentConfig;
        private ExerciceBaseConfigViewModel _blockCurrentConfig;
        private ExerciceBaseConfigViewModel _currentUcConfig;

        private Collection<PositionDataModel> _detectionStart;
        private Collection<PositionDataModel> _detectionBlock;
        private BlockingCollection<DataPosition> PositionXY;
        private List<DataPosition> _tabVitesse;
        public List<ViewModelBase> PagesInternes { get; set; }
        public bool canPrecedent = true;
        private int _countCycle;
        public INavigation _nav;
        public IMessageBoxService _msbs;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the VisualisationViewModel class.
        /// </summary>
        public VisualisationViewModel()
        {

            try
            {

                PagesInternes = new List<ViewModelBase>();
                _nav = SimpleIoc.Default.GetInstance<INavigation>();
                _msbs = SimpleIoc.Default.GetInstance<IMessageBoxService>();
                InitVisualisation();
                InitGraphiquePlot();
                CreateCommands();       // création des commandes de la page
                CreateMessengers();     // création des messages
                Debug.Print("VisualisationViewModel OK");
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        #endregion

        #region Properties

    private ViewModelBase _internViewParametres;
        public ViewModelBase InternViewParametres
        {
            get { return _internViewParametres; }
            set
            {
                if (_internViewParametres != value)
                {
                    _internViewParametres = value;
                    RaisePropertyChanged("InternViewParametres");
                }
            }
        }

        private ViewModelBase _internViewBoutons;
        public ViewModelBase InternViewBoutons
        {
            get { return _internViewBoutons; }
            set
            {
                if (_internViewBoutons != value)
                {
                    _internViewBoutons = value;
                    RaisePropertyChanged("InternViewBoutons");
                }
            }
        }

        private ViewModelBase _internViewList;
        public ViewModelBase InternViewList
        {
            get { return _internViewList; }
            set
            {
                if (_internViewList != value)
                {
                    _internViewList = value;
                    RaisePropertyChanged("InternViewList");
                }
            }
        }

        public bool Pause
        {
            get
            {
                return _pause;
            }
            set
            {
                _pause = value;
                RaisePropertyChanged("Pause");
            }
        }

        /// <summary>
        /// Méthode de changement de nom  du fichier
        /// </summary>
        public string FileName
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                RaisePropertyChanged("FileName");
            }
        }

        /// <summary>
        /// Méthode qui active ou pas la progressBar
        /// </summary>
        public bool ActivOF
        {
            get
            {
                return _activOF;
            }
        }

        public bool InitActivated
        {
            get
            {
                return _initActivated;
            }
            set
            {
                _initActivated = value;
                RaisePropertyChanged("InitActivated");
            }
        }

        /// <summary>
        /// Méthode d'activation ou non de BrowseOF
        /// </summary>
        public bool Save
        {
            get
            {
                return _save;
            }
            set
            {
                _save = value;
                RaisePropertyChanged("Save");
            }
        }

        public ExerciceBaseConfigViewModel CurrentConfig
        {
            get
            {
                return _currentConfig;
            }
            set
            {
                _currentConfig = value;
                RaisePropertyChanged("CurrentConfig");                
            }
        }
       
        public ExerciceBaseConfigViewModel CurrentUcConfig
        {
            get
            {
                return _currentUcConfig;
            }
            set
            {
                _currentUcConfig = value;
                RaisePropertyChanged("CurrentUcConfig");
            }
        }

        public ExerciceBaseConfigViewModel BlockCurrentConfig
        {
            get
            {
                return _blockCurrentConfig;
            }
            set
            {
                _blockCurrentConfig = value;
                RaisePropertyChanged("BlockCurrentConfig");
            }
        }

        public double CurrentForceX
        {
            get
            {
                return this._currentForceX;
            }
            set
            {
                this._currentForceX = value;
                RaisePropertyChanged("CurrentForceX");
            }
        }

        public double CurrentForceY
        {
            get
            {
                return this._currentForceY;
            }
            set
            {
                this._currentForceY = value;
                RaisePropertyChanged("CurrentForceY");
            }
        }

        public double CurrentForce2X
        {
            get
            {
                return this._currentForce2X;
            }
            set
            {
                this._currentForce2X = value;
                RaisePropertyChanged("CurrentForce2X");
            }
        }

        public double CurrentForce2Y
        {
            get
            {
                return this._currentForce2Y;
            }
            set
            {
                this._currentForce2Y = value;
                RaisePropertyChanged("CurrentForce2Y");
            }
        }

        private RegulateurConfig _currentRegulateurConfig;

        public RegulateurConfig CurrentRegulateurConfig
        {
            get { return _currentRegulateurConfig; }
            set 
            {
                _currentRegulateurConfig = value;
                RaisePropertyChanged("CurrentRegulateurConfig");
            }
        }

        private InteractionConfig _currentInteractionConfig;

        public InteractionConfig CurrentInteractionConfig
        {
            get { return _currentInteractionConfig; }
            set 
            {
                _currentInteractionConfig = value;
                RaisePropertyChanged("CurrentInteractionConfig");
            }
        }
        
        public PositionDataModel CurrentPosition
        {
            get
            {
                return this._currentPosition;
            }
            set
            {
                this._currentPosition = value;
                RaisePropertyChanged("CurrentPosition");
            }
        }

        public double ResultDist
        {
            get
            {
                return _resultDist;
            }
            set
            {
                _resultDist = value;
                RaisePropertyChanged("ResultDist");
            }
        }

        public double ResultStra
        {
            get
            {
                return _resultStra;
            }
            set
            {
                if (value > 1.0)
                {
                    _resultStra = 1.0;
                }
                else
                {
                    _resultStra = value;
                }
                RaisePropertyChanged("ResultStra");
            }
        }

        public double ResultVit
        {
            get
            {
                return _resultVit;
            }
            set
            {
                _resultVit = value;
                RaisePropertyChanged("ResultVit");
            }
        }

        public double ResultVitP
        {
            get
            {
                return _resultVitP;
            }
            set
            {
                _resultVitP = value;
                RaisePropertyChanged("ResultVitP");
            }
        }

        public double ResultSmoth
        {
            get
            {
                return _resultSmoth;
            }
            set
            {
                _resultSmoth = value;
                RaisePropertyChanged("ResultSmoth");
            }
        }

        public int CountCycle
        {
            get
            {
                return _countCycle;
            }
            set
            {
                _countCycle = value;
                RaisePropertyChanged("CountCycle");
            }
        }
        private int _scoreMax;

        public int ScoreMax
        {
            get { return _scoreMax; }
            set 
            { 
                _scoreMax = value;
                RaisePropertyChanged("ScoreMax");
            }
        }

        public string Name
        {
            get { return "Visualisation"; }
        }

        private ExercicePoulies _exoPoulies;

        public ExercicePoulies ExoPoulies
        {
            get { return _exoPoulies; }
            set 
            {
                _exoPoulies = value;
                RaisePropertyChanged("ExoPoulies");
            }
        }

        private PlotModel _plotModelVitesseInstant;

        public PlotModel PlotModelVitesseInstant
        {
            get { return _plotModelVitesseInstant; }
            set 
            {
                _plotModelVitesseInstant = value;
                RaisePropertyChanged("PlotModelVitesseInstant");
            }
        }

        private PlotModel _plotVitesseMoyenne;

        public PlotModel PlotModelVitesseMoyenne
        {
            get { return _plotVitesseMoyenne; }
            set 
            {
                _plotVitesseMoyenne = value;
                RaisePropertyChanged("PlotModelVitesseMoyenne");
            }
        }

        private PlotModel _plotModelAngleInstant;

        public PlotModel PlotModelAngleInstant
        {
            get { return _plotModelAngleInstant; }
            set 
            {
                _plotModelAngleInstant = value;
                RaisePropertyChanged("PlotModelAngleInstant");
            }
        }

        private PlotModel _plotModelAngleMoyen;

        public PlotModel PlotModelAngleMoyen
        {
            get { return _plotModelAngleMoyen; }
            set 
            {
                _plotModelAngleMoyen = value;
                RaisePropertyChanged("PlotModelAngleMoyen");
            }
        }

        private PlotModel _plotModelSatInstant;

        public PlotModel PlotModelSatInstant
        {
            get { return _plotModelSatInstant; }
            set 
            {
                _plotModelSatInstant = value;
                RaisePropertyChanged("PlotModelSatInstant");
            }
        }

        private PlotModel _plotModelSatMoyen;

        public PlotModel PlotModelSatMoyen
        {
            get { return _plotModelSatMoyen; }
            set
            {
                _plotModelSatMoyen = value;
                RaisePropertyChanged("PlotModelSatMoyen");
            }
        }


        private bool _symetrieVS;

        public bool SymetrieVS
        {
            get { return _symetrieVS; }
            set 
            { 
                _symetrieVS = value;
                this.CurrentInteractionConfig.Jss = !value;
                this.CurrentInteractionConfig.Vs = value;
                ChangementDeSymetrie();
            }
        }

        private bool _symetrieJSS;

        public bool SymetrieJSS
        {
            get { return _symetrieJSS; }
            set 
            { 
                _symetrieJSS = value;
                this.CurrentInteractionConfig.Jss = value;
                this.CurrentInteractionConfig.Vs = !value;
            }
        }

        private bool _dephasage;

        public bool Dephasage
        {
            get { return _dephasage; }
            set
            {
                _dephasage = value;
            }
        }

        #endregion

        #region Methods
        public void LoadReed(List<ExerciceGeneric> liteExReed)
        {
            try
            {
                if (liteExReed[0].TypeExercice == ExerciceTypes.Jeu)
                {
                    _exoReedList = new ObservableCollection<ExerciceReeducation>();
                    foreach (var exo in liteExReed)
                    {
                        ExoReedList.Add((ExerciceReeducation)exo);
                    }
                    RaisePropertyChanged("ExoReedList");
                    //permet de recuperer le score de base
                    List<ExerciceJeu> listeJeu = new List<ExerciceJeu>();
                    listeJeu.Add(ExoReedList[0] as ExerciceJeu);
                    ScoreMax = listeJeu[0].Score * listeJeu[0].Niveau;
                }
                if (PagesInternes.Count == 0)
                {
                    PagesInternes.Add(new ListeVisualisationReeducation());
                    PagesInternes.Add(new BoutonsVisualisationReeducationViewModel());
                    PagesInternes.Add(new ParametresVisualisationReeducationViewModel());
                }

                InternViewList = PagesInternes[0];
                InternViewBoutons = PagesInternes[1];
                InternViewParametres = PagesInternes[2];
                PagesInternes.Clear();
                canPrecedent = true;
                //reset le nombre de cycle pour l'exercice suivant
                CountCycle = 0;
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        private void LoadEvaluation(List<ExerciceGeneric> listeExEval)
        {
            try
            {
                if (listeExEval[0].TypeExercice == ExerciceTypes.Evaluation)
                {
                    _exoEvalList = new ObservableCollection<ExerciceEvaluation>();
                    foreach (var exo in listeExEval)
                    {
                        ExoEvalList.Add((ExerciceEvaluation)exo);
                    }
                    RaisePropertyChanged("ExoEvalList");
                    if (ExoEvalList[0] is ExercicePoulies)
                    {
                        this.ExoPoulies = ExoEvalList[0] as ExercicePoulies;
                        //reset des tableau de positions

                        if (this.ExoPoulies.TypeSymetrie == TypeSymetriePoulies.JssHorloge || this.ExoPoulies.TypeSymetrie == TypeSymetriePoulies.JssAntiHorloge)
                            this.SymetrieJSS = true;
                        else
                            this.SymetrieVS = true;

                        if (this.ExoPoulies.Decalage == 0)
                            this.Dephasage = false;
                        else
                            this.Dephasage = true;

                        InitConfigAEnvoyer();
                        EnvoyerConfigPort();
                        EnvoyerConfigRegulateur();
                        EnvoyerConfigInteraction();
                        EnvoyerConfigEx();
                    }
                }

                if (PagesInternes.Count == 0)
                {
                    PagesInternes.Add(new ListeVisualisationEvaluationViewModel());
                    PagesInternes.Add(new BoutonsVisualisationEvaluationViewModel());
                    PagesInternes.Add(new ParametresVisualisationEvaluationViewModel());
                }

                InternViewList = PagesInternes[0];
                InternViewBoutons = PagesInternes[1];
                InternViewParametres = PagesInternes[2];
                PagesInternes.Clear();
                canPrecedent = true;
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }
 
        private void InitVisualisation()
        {

            _countCycle = 0;
            _tabVitesse = new List<DataPosition>();
            initdetextstart = new PositionDataModel(3750, 5000);
            _detectionStart = new Collection<PositionDataModel>();
            _detectionBlock = new Collection<PositionDataModel>();
            initdetextblock = new PositionDataModel();

            _currentConfig = new ExerciceBaseConfigViewModel(new ExerciceBaseConfig());
            _TempsCurrentConfig = new ExerciceBaseConfigViewModel(new ExerciceBaseConfig());
            _blockCurrentConfig = new ExerciceBaseConfigViewModel(new ExerciceBaseConfig());
            _currentUcConfig = new ExerciceBaseConfigViewModel(new ExerciceBaseConfig());

            _currentRegulateurConfig = new RegulateurConfig();
            _currentInteractionConfig = new InteractionConfig();

            _currentPosition = new PositionDataModel(100, 100);

            _acq100Hz = new System.Timers.Timer(100);    // Create a timer with a ten millisecond interval.
            _acq100Hz.Elapsed += new ElapsedEventHandler(Onacq100HzSequenceEvent);    // Hook up the Elapsed event for the timer.
            _acq100Hz.Enabled = false;
            
            timerSave = new DispatcherTimer();
            timerSave.Interval = TimeSpan.FromMilliseconds(100);
            timerSave.Tick += new EventHandler(timer_TickSave);     // appelé tout les 100 ms  

            tempsPositionXY = new DataPosition();
            //tempsPositionXY.X = 35.45;
            //tempsPositionXY.Y = 45.20;
            PositionXY = new BlockingCollection<DataPosition>();    // pour les calc
            PositionXY.Add(tempsPositionXY);
        }

        private void InitGraphiquePlot()
        {
            //Vitesse
            this.PlotModelVitesseInstant = new PlotModel() { Title = "Vitesse" };
            var axeVitesse = new LinearAxis() { Title = "Vitesse (cm/s)", Position = AxisPosition.Left, Maximum = 45, Minimum = -1, MajorStep = 10 };
            var axeTemps = new LinearAxis() { Title = "Temps (s)", Position = AxisPosition.Bottom, Maximum = 60, Minimum = -1 };
            this.PlotModelVitesseInstant.Axes.Add(axeVitesse);
            this.PlotModelVitesseInstant.Axes.Add(axeTemps);
            var lineSeriesVitesseInstant = new LineSeries() { MarkerType = MarkerType.Circle };
            this.PlotModelVitesseInstant.Series.Add(lineSeriesVitesseInstant);
            
            this.PlotModelVitesseMoyenne = new PlotModel() { Title = "Vitesse Moyenne"};
            var axeVitesseMoy = new LinearAxis() { Title = "Vitesse (cm/s)", Position = AxisPosition.Left, Maximum = 45, Minimum = -1, MajorStep = 10 };
            var axeTempsMoy = new LinearAxis() { Title = "Exercice", Position = AxisPosition.Bottom, Maximum = 15, Minimum = 0, MajorStep = 1, MinorStep = 1};
            this.PlotModelVitesseMoyenne.Axes.Add(axeVitesseMoy);
            this.PlotModelVitesseMoyenne.Axes.Add(axeTempsMoy);
            var lineSeriesVitesseMoyenne = new LineSeries() { MarkerType = MarkerType.Circle };
            this.PlotModelVitesseMoyenne.Series.Add(lineSeriesVitesseMoyenne);
            
            //Angle
            this.PlotModelAngleInstant = new PlotModel() { Title = "Angle" };
            var axeAngle = new LinearAxis() { Title = "Angle (°)", Position = AxisPosition.Left, Maximum = 31, Minimum = -31, MinorStep = 10, MajorStep = 10};
            var axeTempsAngle = new LinearAxis() { Title = "Temps (s)", Position = AxisPosition.Bottom, Maximum = 60, Minimum = 0 };
            this.PlotModelAngleInstant.Axes.Add(axeAngle);
            this.PlotModelAngleInstant.Axes.Add(axeTempsAngle);
            var lineSeriesAngleInstant = new LineSeries() { MarkerType = MarkerType.Circle };
            this.PlotModelAngleInstant.Series.Add(lineSeriesAngleInstant);
          
            this.PlotModelAngleMoyen = new PlotModel() { Title = "Angle Moyen" };
            var axeAngleMoyen = new LinearAxis() { Title = "Angle (°)", Position = AxisPosition.Left, Maximum = 31, Minimum = -1, MinorStep = 10, MajorStep = 10 };
            var axeTempsAngleMoyen = new LinearAxis() { Title = "Exercice", Position = AxisPosition.Bottom, Maximum = 15, Minimum = 0, MajorStep = 1, MinorStep = 1};
            this.PlotModelAngleMoyen.Axes.Add(axeAngleMoyen);
            this.PlotModelAngleMoyen.Axes.Add(axeTempsAngleMoyen);
            var lineSeriesAngleMoyen = new LineSeries() { MarkerType = MarkerType.Circle };
            this.PlotModelAngleMoyen.Series.Add(lineSeriesAngleMoyen);

            //Sat
            this.PlotModelSatInstant = new PlotModel() { Title = "SAT" };
            var axeSat = new LinearAxis() { Title = "SAT", Position = AxisPosition.Left, Maximum = 100, Minimum = -1, MinorStep = 10, MajorStep = 10 };
            var axeTempsSat = new LinearAxis() { Title = "Temps (s)", Position = AxisPosition.Bottom, Maximum = 60, Minimum = 0 };
            this.PlotModelSatInstant.Axes.Add(axeSat);
            this.PlotModelSatInstant.Axes.Add(axeTempsSat);
            var lineSeriesSatInstant = new LineSeries() { MarkerType = MarkerType.Circle };
            this.PlotModelSatInstant.Series.Add(lineSeriesSatInstant);

            this.PlotModelSatMoyen = new PlotModel() { Title = "SAT Moyen" };
            var axeSatMoyen = new LinearAxis() { Title = "SAT", Position = AxisPosition.Left, Maximum = 100, Minimum = -1, MinorStep = 10, MajorStep = 10 };
            var axeTempsSatMoyen = new LinearAxis() { Title = "Exercice", Position = AxisPosition.Bottom, Maximum = 15, Minimum = 0, MajorStep = 1, MinorStep = 1 };
            this.PlotModelSatMoyen.Axes.Add(axeSatMoyen);
            this.PlotModelSatMoyen.Axes.Add(axeTempsSatMoyen);
            var lineSeriesSatMoyen = new LineSeries() { MarkerType = MarkerType.Circle };
            this.PlotModelSatMoyen.Series.Add(lineSeriesSatMoyen);
        }

        private void CreateCommands()
        {
            EnvoyerConfigCommand = new RelayCommand(() => EnvoyerConfigPort(),CanExecuteConfigCommand);
            EnvoyerConfigInteractionCommand = new RelayCommand(EnvoyerConfigInteraction);
            EnvoyerConfigRegulateurCommand = new RelayCommand(EnvoyerConfigRegulateur);
            PreviousViewModelCommand = new RelayCommand(GoBack, CanGoBack);
            MainViewModelCommand = new RelayCommand(NavigateToHome);
        }

        private void CreateMessengers()
        {
            Messenger.Default.Register<List<ExerciceGeneric>>(this, "NextExercice", TraitementExercice);    // abonnement aux messages envoyé par MainViewModel pour envoyer au µc
            Messenger.Default.Register<List<ExerciceGeneric>>(this, "ReeducationKidWizardViewModel", TraitementExercice);    // abonnement aux messages envoyé par ReeducationKidWizard pour envoyer au µc
            Messenger.Default.Register<List<ExerciceGeneric>>(this, "EvaluationViewModel", TraitementExercice);              // abonnement aux messages envoyé par Evaluation pour envoyer au µc
            Messenger.Default.Register<ForceDataModel>(this, "NewForce",TraitementForce);
            Messenger.Default.Register<Force2DataModel>(this, "NewForce2", TraitementForce2); // abonnement aux messages envoyé par MainViewModel pour afficher la force
            Messenger.Default.Register<PositionDataModel>(this, "NewPosition",TraitementPosition);  // abonnement aux messages envoyé par MainViewModel pour afficher la position
            Messenger.Default.Register<Position2DataModel>(this, "NewPosition2", TraitementPosition2);
            Messenger.Default.Register<FrameConfigDataModel>(this, "NewUcConfig", ConfigUcUpdateMessage);               // abonnement aux messages envoyé par MainViewModel pour afficher la configuration du µc
            //Messenger.Default.Register<bool>(this, "Acq", Acquisition);                     // abonnement aux messages envoyé par MainViewModel à la réception d'un ACK aquisition
            Messenger.Default.Register<FrameExerciceDataModel>(this, "MainPViewModel", Acquisition);
            Messenger.Default.Register<int>(this, "CountCycle", CCycle);
            Messenger.Default.Register<RobotErrorMessage>(this, "NewRobotError", OnRobotError); // effectuer si il y a une erreur robot
            Messenger.Default.Register<bool>(this, "VisualisationCanPrecedent", ChangeCanPrecedent);
            Messenger.Default.Register<double[]>(this, "VitesseInstanPlateau", VitessteInstantannePlateau);
            Messenger.Default.Register<double[]>(this, "VitesseMoyennePlateau", VitessteMoyennePlateau);
            Messenger.Default.Register<double[]>(this, "AngleInstanPlateau", AngleInstantannePlateau);
            Messenger.Default.Register<double[]>(this, "AngleMoyenPlateau", AngleMoyenPlateau);
            Messenger.Default.Register<double[]>(this, "SatInstanPlateau", SatInstantannePlateau);
            Messenger.Default.Register<double[]>(this, "SatMoyenPlateau", SatMoyenPlateau);
            Messenger.Default.Register<bool>(this, "ClearGraphiquesInstant", ClearGraphiquesInstant);
            Messenger.Default.Register<bool>(this, "ClearGraphiquesMoyen", ClearGraphiquesMoyen);
        }

        public void NavigateToHome()
        {
            try
            {
                if((_msbs.ShowYesNo(AxLanguage.Languages.REAplan_Accueil_Confirmation,CustomDialogIcons.Question) == CustomDialogResults.Yes))
                {
                    Messenger.Default.Send<bool>(true, "InitAnalyseEval");
                    //stop le robot et arret l'aquisition de données.
                    Messenger.Default.Send("n", "StopRobot");
                    //Clean l'interface patient.
                    Messenger.Default.Send(true, "ClearGui");
                    //stoper les timer des exercices
                    Messenger.Default.Send(true, "StopTimers");
                    Messenger.Default.Send("", "ResetCurentListExercice");
                    Messenger.Default.Send("", "ResetCanExecuteMainVM");
                    Messenger.Default.Send(true, "CleanDePause");
                    Messenger.Default.Send(false, "GamePause");//annule l'effet de pause sur le mainP quand on revient a l'accueil.
                    CountCycle = 0;

                    this.ClearGraphiquesInstant(true);
                    this.ClearGraphiquesMoyen(true);
                    _nav.NavigateTo<HomeViewModel>(false);
                }
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        private bool CanExecuteConfigCommand()
        {
            return !Singleton.GetRobotError();
        }

        private bool CanGoBack()
        {
            if (Singleton.GetRobotError() == true || canPrecedent == false)
                return false;
            else
                return true;
        }
        private void ChangeCanPrecedent(bool state)
        {
            canPrecedent = state;
        }
        //Permet de revenir sur le premier ViewModel
        private void GoBack()
        {
            try
            {
                Messenger.Default.Send<bool>(true, "InitAnalyseEval");
                if (_nav.GetTypeViewModelToBack() == typeof(EvaluationViewModel))
                {
                    _nav.GoBack("SetIsRetour", new object[] { true });
                }
                if (_nav.GetTypeViewModelToBack() == typeof(ChoixRessortMainViewModel))
                {
                    _nav.GoBack("SetIsRetour", new object[] { true });
                }
                //stop le robot et arret l'aquisition de données.
                Messenger.Default.Send("n", "StopRobot");
                //Clean l'interface patient.
                Messenger.Default.Send(true, "ClearGui");
                //stoper les timer des exercices
                Messenger.Default.Send(true, "StopTimers");
                //Enleve l'ecran de pause si il est activé.
                Messenger.Default.Send(true, "CleanDePause");
                this.ClearGraphiquesMoyen(true);
                this.ClearGraphiquesInstant(true);

                Messenger.Default.Send("", "ResetCanExecuteMainVM");//reset les can execute des bouttons de la visu
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        private void CCycle(int e)
        {
            CountCycle = e;
        }

        private void TraitementExercice(List<ExerciceGeneric> newExerciceList)
        {
            try
            {
                if(newExerciceList[0].TypeExercice== ExerciceTypes.Jeu)
                    LoadReed(newExerciceList);
                if (newExerciceList[0].TypeExercice == ExerciceTypes.Evaluation)
                    LoadEvaluation(newExerciceList);
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        private void TraitementForce(ForceDataModel newForce)
        {
            try
            {
                if (newForce != null)
                {
                    CurrentForceX = newForce.ForceX;
                    CurrentForceY = newForce.ForceY;
                }
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        private void TraitementForce2(Force2DataModel newForce)
        {
            try
            {
                if (newForce != null)
                {
                    CurrentForce2X = newForce.ForceX;
                    CurrentForce2Y = newForce.ForceY;
                }
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        public void TraitementPosition(PositionDataModel newPosition)   // TODO : à changer !
        {
            
        }

        public void TraitementPosition2(Position2DataModel newPosition)   // TODO : à changer !
        {
            
        }

        public void ConfigUcUpdateMessage(FrameConfigDataModel newConfigUc)   // TODO : à changer !
        {
            try
            {
                if (newConfigUc != null)
                {
                    switch (newConfigUc.Address)
                    {
                        case ConfigAddresses.KlatClong:
                            CurrentUcConfig.RaideurLat = (byte)newConfigUc.Data1_2;
                            CurrentUcConfig.RaideurLong = (byte)newConfigUc.Data3_4;
                            break;
                        case ConfigAddresses.VitesseNbrsrep:
                            CurrentUcConfig.Vitesse = (byte)newConfigUc.Data1_2;
                            break;
                        case ConfigAddresses.ActifPassif:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        private void Acquisition(FrameExerciceDataModel ack)
        {
            _ack_end = true;
        }
        
        #endregion

        #region RelayCommand
        public RelayCommand PreviousViewModelCommand { get; set; }
        public RelayCommand MainViewModelCommand { get; set; }

        public RelayCommand EnvoyerConfigCommand
        {
            get;
            private set;
        }
        public RelayCommand EnvoyerConfigInteractionCommand { get; set; }
        public RelayCommand EnvoyerConfigRegulateurCommand { get; set; }
        #endregion

        #region Actions

        // Specify what you want to happen when the Elapsed event is 
        // raised.
        private void Onacq100HzSequenceEvent(object source, ElapsedEventArgs e)
        //private void Onacq100HzSequenceEvent()
        {
            try
            {
                if (pause == false)
                {
                    //if (Math.Abs(Distance(tempsPositionXY.X, tempsPositionXY.Y, 35.45, 36.00)) < 0.3 && _ack_end == true && _ack_end_ok == false)
                    if (_ack_end == true)
                    {
                        _detectionBlockA = false;
                        _ack_end = false;
                        _calc_ack_end = false;
                        _ack_end_ok = true;
                        PositionXY.Add(new DataPosition(tempsPositionXY.X, tempsPositionXY.Y));
                        //PositionXY.Add(test2);
                        _initPosition = true;
                        if (CurrentConfig.Init != 0)
                        {
                            // _detectionStart.Add(new PositionDataModel((int)tempsPositionXY.X, (int)tempsPositionXY.Y));
                            _initdetectstart = true; // test
                        }
                    }
                    else
                    {
                        PositionXY.Add(new DataPosition(tempsPositionXY.X, tempsPositionXY.Y));
                        _tabVitesse.Add(new DataPosition(tempsPositionXY.X, tempsPositionXY.Y));
                        _detectionBlockA = true;
                    }
                }
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        /// <summary>
        /// Méthode qui décrémente le temps restant et arrête le timer
        /// </summary>
        private void timer_TickSave(object sender, EventArgs e)
        {
            try
            {
                if (pause == false)
                {
                    test = true;
                    File.AppendAllText(FileName, text + Environment.NewLine);
                    Debug.Print("Ecriture");
                    test = false;
                }
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

      

        private void EnvoyerConfigPort()
        {
            FrameExerciceDataModel newConfig = new FrameExerciceDataModel();

            newConfig.Address = ConfigAddresses.KlatClong;
            newConfig.Data1 = CurrentConfig.RaideurLat;
            newConfig.Data2 = CurrentConfig.RaideurLong;
            newConfig.Data3 = CurrentConfig.RaideurLat;
            newConfig.Data4 = CurrentConfig.RaideurLong;
            Messenger.Default.Send(newConfig, "VisualisationViewModelConfigExercice");

            newConfig.Address = ConfigAddresses.VitesseNbrsrep;
            newConfig.Data2 = CurrentConfig.Vitesse;
            newConfig.Data4 = CurrentConfig.NbrRep;
            Messenger.Default.Send(newConfig, "VisualisationViewModelConfigExercice");
        }

        private void EnvoyerConfigRegulateur()
        {
            FrameExerciceDataModel newConfig = new FrameExerciceDataModel();
            newConfig.Address = ConfigAddresses.Regulateur;
            newConfig.Data1 = CurrentRegulateurConfig.Kp;
            newConfig.Data2 = CurrentRegulateurConfig.Ki;
            newConfig.Data3 = CurrentRegulateurConfig.CGlob;
            newConfig.Data4 = 0;
            Messenger.Default.Send(newConfig, "VisualisationViewModelConfigExercice");
        }

        private void EnvoyerConfigInteraction()
        {
            FrameExerciceDataModel newConfig = new FrameExerciceDataModel();
            newConfig.Address = ConfigAddresses.VsJssParamInt;
            newConfig.Data1 = Convert.ToByte(CurrentInteractionConfig.Vs);
            newConfig.Data2 = Convert.ToByte(CurrentInteractionConfig.Jss);
            byte xd = (byte)((EchelleUtils.MiseEchelleEnvoyerX2(this.ExoPoulies.PoulieDroite.X))); 
            byte xg = (byte)((EchelleUtils.MiseEchelleEnvoyerX(this.ExoPoulies.PoulieGauche.X)));
            if (CurrentInteractionConfig.Vs)
                CurrentInteractionConfig.ParamInterac = (byte)(EchelleUtils.MiseEchelleEnvoyerX2(this.ExoPoulies.PoulieDroite.X) - EchelleUtils.MiseEchelleEnvoyerX(this.ExoPoulies.PoulieGauche.X));
            else
                CurrentInteractionConfig.ParamInterac = (byte)((xd + xg) / 2);
            newConfig.Data3 = CurrentInteractionConfig.ParamInterac;
            newConfig.Data4 = Convert.ToByte(this.Dephasage);
            Messenger.Default.Send(newConfig, "VisualisationViewModelConfigExercice");

            newConfig.Address = ConfigAddresses.KintCint;
            newConfig.Data1 = CurrentInteractionConfig.KInteracL;
            newConfig.Data2 = CurrentInteractionConfig.CInteractL;
            newConfig.Data3 = CurrentInteractionConfig.KInteractR;
            newConfig.Data4 = CurrentInteractionConfig.CInteractR;
            Messenger.Default.Send(newConfig, "VisualisationViewModelConfigExercice");
        }



        private void EnvoyerConfigEx()
        {
            FrameExerciceDataModel newConfig = new FrameExerciceDataModel();
            newConfig.Address = ConfigAddresses.ActifPassif;
            newConfig.Data1 = (byte)EchelleUtils.MiseEchelleEnvoyerX(this.ExoPoulies.PoulieGauche.X);
            newConfig.Data2 = (byte)EchelleUtils.MiseEchelleEnvoyerY(this.ExoPoulies.PoulieGauche.Y);
            newConfig.Data3 = (byte)EchelleUtils.MiseEchelleEnvoyer(this.ExoPoulies.PoulieGauche.Rayon);
            
            newConfig.Data4 = (byte)EchelleUtils.MiseEchelleEnvoyerX2(this.ExoPoulies.PoulieDroite.X);
            Messenger.Default.Send(newConfig, "VisualisationViewModelConfigExercice");

            Messenger.Default.Send(CommandCodes.mod_init_traj, "MessageCommand");
        }

        
        private void InitConfigAEnvoyer()
        {
            this.CurrentInteractionConfig.Jss = ExoPoulies.ConfigInteraction.Jss;
            this.CurrentInteractionConfig.Vs = ExoPoulies.ConfigInteraction.Vs;

            if (this.CurrentInteractionConfig.Jss)
                SymetrieJSS = true;
            else
                SymetrieVS = true;

            this.CurrentInteractionConfig.KInteracL = ExoPoulies.ConfigInteraction.KInteracL;
            this.CurrentInteractionConfig.CInteractL = ExoPoulies.ConfigInteraction.CInteractL;
            this.CurrentInteractionConfig.KInteractR = ExoPoulies.ConfigInteraction.KInteractR;
            this.CurrentInteractionConfig.CInteractR = ExoPoulies.ConfigInteraction.CInteractR;
        }

        private void VitessteInstantannePlateau(double[] vit)
        {
            // +/- toutes les secondes.
            //mettre la vitesse instant dans le graph.
            var lineSeries = this.PlotModelVitesseInstant.Series[0] as LineSeries;
            lineSeries.Points.Add(new DataPoint(vit[0], vit[1]));
            //Les graphs sont refresh une fois dans SatInstantannePlateau.
        }

        private void VitessteMoyennePlateau(double[] vit)
        {
            // A la fin d'un exercice.
            //mettre la vitesse moyenne dans le graph.
            var lineSeries = this.PlotModelVitesseMoyenne.Series[0] as LineSeries;
            lineSeries.Points.Add(new DataPoint(vit[0], vit[1]));
            //Les graphs sont refresh une fois dans SatMoyenPlateau.
        }

        private void AngleInstantannePlateau(double[] angle)
        {
            // +/- toutes les secondes.
            //mettre l'angle instant dans le graph.
            var lineSeries = this.PlotModelAngleInstant.Series[0] as LineSeries;
            lineSeries.Points.Add(new DataPoint(angle[0], angle[1]));
            //Les graphs sont refresh une fois dans SatInstantannePlateau.
        }

        private void AngleMoyenPlateau(double[] angle)
        {
            // A la fin d'un exercice.
            //mettre l'angle moyenne dans le graph.
            var lineSeries = this.PlotModelAngleMoyen.Series[0] as LineSeries;
            lineSeries.Points.Add(new DataPoint(angle[0], angle[1]));
            //Les graphs sont refresh une fois dans SatMoyenPlateau.
        }

        private void SatInstantannePlateau(double[] sat)
        {
            // +/- toutes les secondes.
            //mettre l'angle instant dans le graph.
            var lineSeries = this.PlotModelSatInstant.Series[0] as LineSeries;
            lineSeries.Points.Add(new DataPoint(sat[0], sat[1]));
            Messenger.Default.Send<bool>(true, "RafrechirGraphiques");
        }

        private void SatMoyenPlateau(double[] sat)
        {
            // A la fin d'un exercice.
            //mettre l'angle moyenne dans le graph.
            var lineSeries = this.PlotModelSatMoyen.Series[0] as LineSeries;
            lineSeries.Points.Add(new DataPoint(sat[0], sat[1]));
            Messenger.Default.Send<bool>(true, "RafrechirGraphiquesMoyens");
        }

        private void ClearGraphiquesInstant(bool state)
        {
            this.PlotModelVitesseInstant.Series[0] = new LineSeries() { MarkerType = MarkerType.Circle };
            this.PlotModelAngleInstant.Series[0] = new LineSeries() { MarkerType = MarkerType.Circle };
            this.PlotModelSatInstant.Series[0] = new LineSeries() { MarkerType = MarkerType.Circle };
            Messenger.Default.Send<bool>(true, "RafrechirGraphiques");
        }

        private void ClearGraphiquesMoyen(bool state)
        {
            this.PlotModelVitesseMoyenne.Series[0] = new LineSeries() { MarkerType = MarkerType.Circle };
            this.PlotModelAngleMoyen.Series[0] = new LineSeries() { MarkerType = MarkerType.Circle };
            this.PlotModelSatMoyen.Series[0] = new LineSeries() { MarkerType = MarkerType.Circle };
            Messenger.Default.Send<bool>(true, "RafrechirGraphiquesMoyens");
        }

        private void ChangementDeSymetrie()
        {
            foreach (var exo in _exoEvalList)
            {
                ExercicePoulies exoPoulies = exo as ExercicePoulies;
                exoPoulies.ConfigInteraction.Jss = this.CurrentInteractionConfig.Jss;
                exoPoulies.ConfigInteraction.Vs = this.CurrentInteractionConfig.Vs;
                exoPoulies.RaferchirSymetriePoulies();
            }
        }

        private void OnRobotError(RobotErrorMessage e)
        {
            //options grisé
            InitVisualisation();// reset les variable et timer
            Messenger.Default.Send("", "StopRobot");
        }
        #endregion

    }
}