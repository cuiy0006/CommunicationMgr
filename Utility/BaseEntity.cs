using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utility
{
    [Serializable]
    public abstract class BaseEntity : ICloneable, IDisposable
    {
        public BaseEntity()
        {


        }
        public virtual void Dispose()
        {
        }


        #region ICloneable

        /// <summary>
        /// Clone object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
