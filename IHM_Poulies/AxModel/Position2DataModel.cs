using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AxModel
{
    public class Position2DataModel : aXdataModel
    {
        #region Fields
        /// <summary>
        /// Position reçue UART
        /// </summary>
        private int _positionX, _positionY;

        #endregion

        #region Constructors

        public Position2DataModel(int poX, int poY)
        {
            this._positionX = poX;
            this._positionY = poY;
        }

        public Position2DataModel()
        {
            this._positionX = 0;
            this._positionY = 0;
        }

        public int PositionX
        {
            get
            {
                return _positionX;
            }
            set
            {
                _positionX = value;
            }
        }

        public int PositionY
        {
            get
            {
                return _positionY;
            }
            set
            {
                _positionY = value;
            }
        }

        #endregion

        #region Properties
        #endregion

        #region Methods
        #endregion
    }
}
