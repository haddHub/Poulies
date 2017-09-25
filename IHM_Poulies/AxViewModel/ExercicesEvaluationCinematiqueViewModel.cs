using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using AxExerciceGenerator;
using AxModelExercice;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using AxError;
using AxModel;
using AxTheme;
using GalaSoft.MvvmLight.Ioc;
using Navegar;

namespace AxViewModel
{


    public class ExercicesEvaluationCinematiqueViewModel : ViewModelBase
    {
        private List<ExerciceEvaluation> reaPlanExercices;
        private List<ExerciceGeneric> listExGen;
        public ExercicesEvaluationCinematiqueViewModel()
        {
            try
            {
                Messenger.Default.Register<bool>(this, "ConfigEvalCinematique", Config);
                reaPlanExercices = new List<ExerciceEvaluation>();
                CanUseNext = true;
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }

        #region properties
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
        private bool _canUseNext;

        public bool CanUseNext
        {
            get { return _canUseNext; }
            set 
            {   _canUseNext = value;
                RaisePropertyChanged("CanUseNext");
            }
        }

        private bool _typeCarre = true;
        public bool TypeCarre
        {
            get
            {
                return _typeCarre;
            }
            set
            {
                _typeCarre = value;
                RaisePropertyChanged("TypeCarre");
                CanExecuteNext();
            }
        }

        private bool _typeCible = true;
        public bool TypeCible
        {
            get
            {
                return _typeCible;
            }
            set
            {
                _typeCible = value;
                RaisePropertyChanged("TypeCible");
                CanExecuteNext();
            }
        }
        private bool _typeCercle = true;
        public bool TypeCercle
        {
            get
            {
                return _typeCercle;
            }
            set
            {
                _typeCercle = value;
                RaisePropertyChanged("TypeCercle");
                CanExecuteNext();
            }
        }
        private bool _typeDroite = true;
        public bool TypeDroite
        {
            get
            {
                return _typeDroite;
            }
            set
            {
                _typeDroite = value;
                RaisePropertyChanged("TypeDroite");
                CanExecuteNext();
            }
        }
        #endregion


        private void Config(bool y)
        {
            try
            {
                init_exo();
                listExGen = new List<ExerciceGeneric>(reaPlanExercices);
                Messenger.Default.Send(listExGen, "EvaluationViewModel");  // Message envoyé à MainViewModel pour traitement et envoi au µc
            }
            catch (Exception ex)
            {
                GestionErreur.GerrerErreur(ex);
            }
        }
        private void init_exo()
        {
            reaPlanExercices.Clear();
            string nomFond = SelectedTheme.Remove(0, 28);
            
            List<ThemeEvaluationModel> listeThemeEval = GestionThemes.LoadAllEvalTheme(nomFond);
            if (TypeCarre == true)
            {
                //reaPlanExercices.Add(ExerciceGenerator.GetExerciceEvaluation("carre", listeThemeEval.Find(t => t.Name.Equals("Carre"))));
            }
            if (TypeCercle == true)
            {
                //reaPlanExercices.Add(ExerciceGenerator.GetExerciceEvaluation("cercle", listeThemeEval.Find(t => t.Name.Equals("Cercle"))));
            }
            if (TypeDroite == true)
            {
                //reaPlanExercices.Add(ExerciceGenerator.GetExerciceEvaluation("freeAmpl", listeThemeEval.Find(t => t.Name.Equals("Droite"))));
            }
            if (TypeCible == true)
            {
                //reaPlanExercices.Add(ExerciceGenerator.GetExerciceEvaluation("target", listeThemeEval.Find(t => t.Name.Equals("Cible"))));
            }
        }
        private void CanExecuteNext()
        {
            if (TypeCarre == true || TypeCible == true || TypeCercle == true || TypeDroite == true)
                CanUseNext = true;
            else
                CanUseNext = false;
        }
    }
}
