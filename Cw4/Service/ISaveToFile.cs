using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw4.Service
{
    interface ISaveToFile
    {
        public void saveToFile(string fileName, string content); 
    }
}
