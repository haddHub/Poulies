using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxModel
{
    public class ACKConfigDataModel : aXdataModel
    {
        #region Fields
        /// <summary>
        /// Force Reçu UART
        /// </summary>
        //private int _forceX, _forceY;
        private byte[] data;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ForceDataModel class.
        /// </summary>
        /// <param name="coX"></param>
        /// <param name="coY"></param>
        public ACKConfigDataModel(byte[] d)
        {
            this.data = d;
        }

        public byte[] Donnee
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        #endregion

        #region Properties
        #endregion

        #region Methods
        // TODO : ajouter methode convertion en couple
        #endregion
    }
}
