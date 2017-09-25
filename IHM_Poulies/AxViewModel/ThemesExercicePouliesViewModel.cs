using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using AxTheme;

namespace AxViewModel
{
    public class ThemesExercicePouliesViewModel : ViewModelBase 
    {
        private ObservableCollection<string> _listeThemes;

        public ObservableCollection<string> ListeThemes
        {
            get { return _listeThemes; }
            set { _listeThemes = value; }
        }
        

        public ThemesExercicePouliesViewModel()
        {
            _listeThemes = new ObservableCollection<string>();
            _listeThemes = GestionThemes.LoadDefaultFondEvalTheme();
        }
    }
}
