using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OxCoin.Repository.Entities
{
    public class Transaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public decimal TransferedAmount { get; set; }

        [Required]
        public decimal TransferFee { get; set; } = Math.Round(new Random().Next(1, 99) * 0.000001m, 4);

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public int Size { get; set; } = new Random().Next(200, 700);

        [Required]
        public Guid SourceWalletId { get; set; }

        [ForeignKey("SourceWalletId")]
        public virtual Wallet SourceWallet { get; set; }

        [Required]
        public Guid DestinationWalletId { get; set; }

        [ForeignKey("DestinationWalletId")]
        public virtual Wallet DestinationWallet { get; set; }
    }
}
