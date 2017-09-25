using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using AxModelExercice;
using AxModel;
using AxTheme;
using AxExerciceGenerator;
using AxData;
using GalaSoft.MvvmLight.Command;

namespace AxViewModel
{
    /// <summary>
    /// ViewModel qui creer, configure et envoye l'exercice des poulies.
    /// </summary>
    public class ExercicePouliesChoixViewModel : ViewModelBase
    {
        private bool _canUseNext = true;
        public bool CanUseNext
        {
            get { return _canUseNext; }
            set 
            {
                _canUseNext = value;
                RaisePropertyChanged("CanUseNext");
            }
        }

        private string _selectedTheme;
        public string SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {
                _selectedTheme = value;
                RaisePropertyChanged("SelectedTheme");
            }
        }

        private double assistanceValue;
        public double AssistanceValue
        {
            get { return assistanceValue; }
            set 
            {
                assistanceValue = value;
                RaisePropertyChanged("AssistanceValue");
            }
        }


        private int modeScenario = 6; // pour le jour 1 qui est défaut

        private TypeSymetriePoulies currentSymetrie = TypeSymetriePoulies.JssHorloge;
        private int decalage = 90;

        private List<ExerciceEvaluation> reaPlanExercices;
        private List<ExerciceGeneric> listExGen;

        public RelayCommand<int> ChoixScenarioCommand { get; set; }
        public RelayCommand<int> ChoixSymetrieCommand { get; set; }
        public RelayCommand<int> ChoixDecalageCommand { get; set; }

        public ExercicePouliesChoixViewModel()
        {
            reaPlanExercices = new List<ExerciceEvaluation>();
            ChoixScenarioCommand = new RelayCommand<int>(CreationScenario);
            ChoixSymetrieCommand = new RelayCommand<int>(ChoixSymetrie);
            ChoixDecalageCommand = new RelayCommand<int>(ChoixDecalage);
            Messenger.Default.Register<bool>(this, "ConfigEvalCinematique", Config);
        }

        private void CreationScenario(int mode)
        {
            modeScenario = mode;
        }

        private void ChoixSymetrie(int sym)
        {
            switch (sym)
            {
                case 0: currentSymetrie = TypeSymetriePoulies.JssHorloge;
                    break;
                case 1: currentSymetrie = TypeSymetriePoulies.VsHorloge;
                    break;
                default:
                    break;
            }
        }

        private void ChoixDecalage(int dec)
        {
            if (dec == 0)
            {
                decalage = 90;
            }
            else if (dec == 1)
            {
                decalage = 0;
            }
        }

        private void Config(bool y)
        {
            init_exo(y);
            listExGen = new List<ExerciceGeneric>(reaPlanExercices);

            if (!y)
                Messenger.Default.Send(listExGen, "EvaluationViewModel");
            else
                Messenger.Default.Send(listExGen, "ChoixRessortMainViewModel");
        }

        private void init_exo(bool y)
        {
            reaPlanExercices.Clear();

            string nomFond = SelectedTheme.Remove(0, 28);
            List<ThemeEvaluationModel> listeThemeEval = GestionThemes.LoadAllEvalTheme(nomFond);

            CreationExercicesPoulies(listeThemeEval, y);
            modeScenario = 6;
            currentSymetrie = TypeSymetriePoulies.JssHorloge;
            decalage = 90;
        }
        //Creation de tout la série d'exercice.
        //5 TestVitesseLibre (20sec, 5sec)
        //15 TestVitesseContrainte (20sec,5sec) + vitesse
        //15 Entrainement (60sec,30sec)
        //5 TestVitesseLibre (20sec, 5sec)
        //15 TestVitesseContrainte (20sec,5sec) + vitesse
        private void CreationExercicesPoulies(List<ThemeEvaluationModel> listeTheme, bool ressort)
        {
            if (modeScenario == 0 || modeScenario == 6)
            {
                #region Defaut
                ExercicePoulies exoPoulies;
                //Creation des Exercice a vitesse libre.
                int nbrExoLibre = (modeScenario == 0) ? nbrExoLibre = 5 : nbrExoLibre = 8;
                for (int i = 0; i < nbrExoLibre; i++)
                {
                    exoPoulies = ExerciceGenerator.GetExerciceEvaluation("poulies", listeTheme.Find(t => t.Name.Equals("Cercle")), TypeExercicePoulies.TestVitesseLibre, currentSymetrie, decalage) as ExercicePoulies;
                    exoPoulies.NumeroExercice = i + 1;
                    this.reaPlanExercices.Add(exoPoulies);
                }
                exoPoulies = this.reaPlanExercices.Last() as ExercicePoulies;
                //Le temps de pause = 0 permet un start manuel et pas automatique.
                exoPoulies.TempsDePause = 0;

                //Creation des Exercice a vitesse contrainte en cm/s.
                int numExercice = 1;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        exoPoulies = ExerciceGenerator.GetExerciceEvaluation("poulies", listeTheme.Find(t => t.Name.Equals("Cercle")), TypeExercicePoulies.TestVitesseContrainte, currentSymetrie, decalage) as ExercicePoulies;
                        if (i == 0)
                            exoPoulies.VitesseContrainte = 4;
                        else if (i == 1)
                            exoPoulies.VitesseContrainte = 8;
                        else if (i == 2)
                            exoPoulies.VitesseContrainte = 12;
                        exoPoulies.NumeroExercice = numExercice;
                        numExercice++;
                        this.reaPlanExercices.Add(exoPoulies);
                    }
                }
                exoPoulies = this.reaPlanExercices.Last() as ExercicePoulies;
                exoPoulies.TempsDePause = 0;

                //Creation des Exercice entrainement.
                numExercice = 1;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        exoPoulies = ExerciceGenerator.GetExerciceEvaluation("poulies", listeTheme.Find(t => t.Name.Equals("Cercle")), TypeExercicePoulies.Entrainement, currentSymetrie, decalage) as ExercicePoulies;
                        exoPoulies.NumeroExercice = numExercice;
                        numExercice++;
                        this.reaPlanExercices.Add(exoPoulies);
                    }
                }
                exoPoulies = this.reaPlanExercices.Last() as ExercicePoulies;
                exoPoulies.TempsDePause = 0;

                //Creation des Exercice a vitesse libre 2.
                for (int i = 0; i < 5; i++)
                {
                    exoPoulies = ExerciceGenerator.GetExerciceEvaluation("poulies", listeTheme.Find(t => t.Name.Equals("Cercle")), TypeExercicePoulies.TestVitesseLibre, currentSymetrie, decalage) as ExercicePoulies;
                    exoPoulies.NumeroExercice = i + 1;
                    this.reaPlanExercices.Add(exoPoulies);
                }
                exoPoulies = this.reaPlanExercices.Last() as ExercicePoulies;
                //Le temps de pause = 0 permet un start manuel et pas automatique.
                exoPoulies.TempsDePause = 0;

                //Creation des Exercice a vitesse contrainte 2.
                numExercice = 1;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        exoPoulies = ExerciceGenerator.GetExerciceEvaluation("poulies", listeTheme.Find(t => t.Name.Equals("Cercle")), TypeExercicePoulies.TestVitesseContrainte, currentSymetrie, decalage) as ExercicePoulies;
                        if (i == 0)
                            exoPoulies.VitesseContrainte = 4;
                        else if (i == 1)
                            exoPoulies.VitesseContrainte = 8;
                        else if (i == 2)
                            exoPoulies.VitesseContrainte = 12;
                        exoPoulies.NumeroExercice = numExercice;
                        numExercice++;
                        this.reaPlanExercices.Add(exoPoulies);
                    }
                }
                exoPoulies = this.reaPlanExercices.Last() as ExercicePoulies;
                exoPoulies.TempsDePause = 0;
                #endregion
            }
            else if (modeScenario == 1)
            {
                ExercicePoulies exoPoulies;
                //Creation des Exercice a vitesse libre.
                for (int i = 0; i < 5; i++)
                {
                    exoPoulies = ExerciceGenerator.GetExerciceEvaluation("poulies", listeTheme.Find(t => t.Name.Equals("Cercle")), TypeExercicePoulies.TestVitesseLibre, currentSymetrie, decalage) as ExercicePoulies;
                    exoPoulies.NumeroExercice = i + 1;
                    this.reaPlanExercices.Add(exoPoulies);
                }
                exoPoulies = this.reaPlanExercices.Last() as ExercicePoulies;
                //Le temps de pause = 0 permet un start manuel et pas automatique.
                exoPoulies.TempsDePause = 0;
            }
            else if (modeScenario == 2)
            {
                ExercicePoulies exoPoulies;
                //Creation des Exercice a vitesse contrainte en cm/s.
                int numExercice = 1;
                for (int j = 0; j < 5; j++)
                {
                    exoPoulies = ExerciceGenerator.GetExerciceEvaluation("poulies", listeTheme.Find(t => t.Name.Equals("Cercle")), TypeExercicePoulies.TestVitesseContrainte, currentSymetrie, decalage) as ExercicePoulies;
                    exoPoulies.VitesseContrainte = 4;
                    exoPoulies.NumeroExercice = numExercice;
                    numExercice++;
                    this.reaPlanExercices.Add(exoPoulies);
                }
                exoPoulies = this.reaPlanExercices.Last() as ExercicePoulies;
                exoPoulies.TempsDePause = 0;
            }
            else if (modeScenario == 3)
            {
                ExercicePoulies exoPoulies;
                //Creation des Exercice a vitesse contrainte en cm/s.
                int numExercice = 6;
                for (int j = 0; j < 5; j++)
                {
                    exoPoulies = ExerciceGenerator.GetExerciceEvaluation("poulies", listeTheme.Find(t => t.Name.Equals("Cercle")), TypeExercicePoulies.TestVitesseContrainte, currentSymetrie, decalage) as ExercicePoulies;
                    exoPoulies.VitesseContrainte = 8;
                    exoPoulies.NumeroExercice = numExercice;
                    numExercice++;
                    this.reaPlanExercices.Add(exoPoulies);
                }
                exoPoulies = this.reaPlanExercices.Last() as ExercicePoulies;
                exoPoulies.TempsDePause = 0;
            }
            else if (modeScenario == 4)
            {
                ExercicePoulies exoPoulies;
                //Creation des Exercice a vitesse contrainte en cm/s.
                int numExercice = 11;
                for (int j = 0; j < 5; j++)
                {
                    exoPoulies = ExerciceGenerator.GetExerciceEvaluation("poulies", listeTheme.Find(t => t.Name.Equals("Cercle")), TypeExercicePoulies.TestVitesseContrainte, currentSymetrie, decalage) as ExercicePoulies;
                    exoPoulies.VitesseContrainte = 12;
                    exoPoulies.NumeroExercice = numExercice;
                    numExercice++;
                    this.reaPlanExercices.Add(exoPoulies);
                }
                exoPoulies = this.reaPlanExercices.Last() as ExercicePoulies;
                exoPoulies.TempsDePause = 0;
            }
            else if (modeScenario == 5)
            {
                ExercicePoulies exoPoulies;
                //Creation des Exercice entrainement.
                int numExercice = 1;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        exoPoulies = ExerciceGenerator.GetExerciceEvaluation("poulies", listeTheme.Find(t => t.Name.Equals("Cercle")), TypeExercicePoulies.Entrainement, currentSymetrie, decalage) as ExercicePoulies;
                        exoPoulies.NumeroExercice = numExercice;
                        exoPoulies.ValeurSliderAssist = this.AssistanceValue;
                        numExercice++;
                        this.reaPlanExercices.Add(exoPoulies);
                    }
                }
                exoPoulies = this.reaPlanExercices.Last() as ExercicePoulies;
                exoPoulies.TempsDePause = 0;
            }

            //si ressort = false, c'est que le choix à déjà été fait, regarde si le patient doit utiliser ou non le ressort et l'ajout dans la liste d'exercices.
            if (ressort == false)
            {
                bool besoinRessort = PatientData.GetRessort();
                foreach (var ex in reaPlanExercices)
                {
                    ExercicePoulies exoRessort = ex as ExercicePoulies;
                    if (besoinRessort)
                        exoRessort.Ressort = TypeRessort.AvecRessort;
                    else
                        exoRessort.Ressort = TypeRessort.SansRessort;
                }
            }
        }
    }
}
