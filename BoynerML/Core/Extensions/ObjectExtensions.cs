using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoynerML.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static void RunSafely(this object obj, Action action, Action<Exception> errorAction = null)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException;
                while (innerException != null && innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }

                if (errorAction != null)
                {
                    errorAction(ex);
                }
            }
        }
    }
}