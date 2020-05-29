using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cw4.Service
{
    public interface ITokenDAL
    {
        public bool TokenExistsInDB(string token);

        public void InsertToken(string token);
        
    }
}
