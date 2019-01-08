using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoynerML.Services
{
    public interface IPredictor
    {
        int[] Predict(double[] input);
    }
}