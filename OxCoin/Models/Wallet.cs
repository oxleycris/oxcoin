using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxCoin.Models
{
    public class Wallet
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public virtual User User { get; set; }
    }
}
