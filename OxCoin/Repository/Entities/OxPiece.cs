using System;
using System.ComponentModel.DataAnnotations;

namespace OxCoin.Repository.Entities
{
    public class OxPiece
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
