using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASServerTCP
{
    class DisposeObject
    {
        private bool IsDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool Disposing)
        {
            if (!IsDisposed)
            {
                if (Disposing)
                {
                    // zwalniaj zasoby zarządzalne
                }
                // zwalniaj zasoby niezarządzalne
            }
            IsDisposed = true;
        }
        ~DisposeObject()
        {
            Dispose(false);
        }
    }
}
