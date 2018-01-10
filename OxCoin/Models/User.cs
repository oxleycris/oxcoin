using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxCoin.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string GivenName { get; set; }

        public string FamilyName { get; set; }
    }
}
