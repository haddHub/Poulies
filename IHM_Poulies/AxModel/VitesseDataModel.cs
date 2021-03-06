﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxModel
{
    public class VitesseDataModel : aXdataModel
    {
        #region Fields
        /// <summary>
        /// Force Reçu UART
        /// </summary>
        private int _vitesseX, _vitesseY;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the VitesseDataModel class.
        /// </summary>
        /// <param name="coX"></param>
        /// <param name="coY"></param>
        public VitesseDataModel(int viX, int viY)
        {
            this._vitesseX = viX;
            this._vitesseY = viY;
        }

        public VitesseDataModel()
        {
            this._vitesseX = 0;
            this._vitesseY = 0;
        }

        #endregion

        #region Properties

        public int VitesseX
        {
            get
            {
                return _vitesseX;
            }
            set
            {
                _vitesseX = value;
            }
        }

        public int VitesseY
        {
            get
            {
                return _vitesseY;
            }
            set
            {
                _vitesseY = value;
            }
        }

        #endregion

        #region Methods
        // TODO : ajouter methode convertion en couple
        #endregion
    }
}
