using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxModel
{

    public enum ModeType
    {
        Passif,
        Activo_passif,
        Actif
    }

    public class ExerciceBaseConfig
    {
        #region Fields

        private ModeType _mode;
        private byte _masse;        // masse_vir
        private byte _viscosite;    // visq_vir
        private byte _raideurLat;   // k_lat
        private byte _raideurLong;  // c_long
        private byte _vitesse;      // temps_desiree
        private byte _nbrRep;       // nbrs_parc
        private byte _init;         // initialisation
        private bool _auto;         // assist auto

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ExerciceBaseConfig class.
        /// </summary>
        public ExerciceBaseConfig()
        {
            this._mode = ModeType.Activo_passif;
            this._masse = 10;
            this._viscosite = 0;
            this._raideurLat = 180;
            this._raideurLong = 0;
            this._vitesse = 0;
            this._nbrRep = 100;
            this._init = 0;
            this._auto = false;
        }

        /// <summary>
        /// Initializes a new instance of the ExerciceBaseConfig class.
        /// </summary>
        /// <param name="mas1"></param>
        /// <param name="mas2"></param>
        /// <param name="vis1"></param>
        /// <param name="vis2"></param>
        /// <param name="rLatX"></param>
        /// <param name="rLatY"></param>
        /// <param name="rLonX"></param>
        /// <param name="rLonY"></param>
        /// <param name="vit"></param>
        /// <param name="nbr"></param>
        /// <param name="pmAX"></param>
        /// <param name="pmAY"></param>
        /// <param name="pmBX"></param>
        /// <param name="pmBY"></param>
        /// <param name="ini"></param>
        /// <param name="aut"></param>
        public ExerciceBaseConfig(byte mas, byte vis, byte rLat, byte rLon, byte vit, byte nbr, byte ini, bool aut)
        {
            this._mode = ModeType.Activo_passif;
            this._masse = mas;
            this._viscosite = vis;
            this._raideurLat = rLat;
            this._raideurLong = rLon;
            this._vitesse = vit;
            this._nbrRep = nbr;
            this._init = ini;
            this._auto = aut;
        }

        public ExerciceBaseConfig(ExerciceBaseConfig ebc)
        {
            this._mode = ebc.ModeType;
            this._masse = ebc.Masse;
            this._viscosite = ebc.Viscosite;
            this._raideurLat = ebc.RaideurLat;
            this._raideurLong = ebc.RaideurLong;
            this._vitesse = ebc.Vitesse;
            this._nbrRep = ebc.NbrRep;
            this._init = ebc.Init;
            this._auto = ebc.Auto;
        }

        #endregion

        #region Properties

        public ModeType ModeType
        {
            get
            {
                return _mode;
            }
            set
            {
                _mode = value;
            }
        }

        public byte Masse
        {
            get
            {
                return this._masse;
            }
            set
            {
                this._masse = value;
            }
        }

        public byte Viscosite
        {
            get
            {
                return this._viscosite;
            }
            set
            {
                this._viscosite = value;
            }
        }

        public byte RaideurLat
        {
            get
            {
                return this._raideurLat;
            }
            set
            {
                this._raideurLat = value;
            }
        }

        public byte RaideurLong
        {
            get
            {
                return this._raideurLong;
            }
            set
            {
                this._raideurLong = value;
            }
        }

        public byte Vitesse
        {
            get
            {
                return this._vitesse;
            }
            set
            {
                this._vitesse = value;
            }
        }

        public byte NbrRep
        {
            get
            {
                return this._nbrRep;
            }
            set
            {
                this._nbrRep = value;
            }
        }

        public byte Init
        {
            get
            {
                return this._init;
            }
            set
            {
                this._init = value;
            }
        }

        public bool Auto
        {
            get
            {
                return this._auto;
            }
            set
            {
                this._auto = value;
            }
        }

        #endregion

        #region Methods

        #endregion

        #region RelayCommand

        #endregion

        #region Actions

        #endregion
    }
}
