using AO.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Dapper.CX.Models
{
    [Identity(nameof(Id))]    
    [Schema("changes")]
    public class ColumnHistory
    {
        public long Id { get; set; }

        [MaxLength(50)]
        [Required]
        public string UserName { get; set; }

        public DateTime Timestamp { get; set; }

        [Required]
        [MaxLength(100)]
        [Key]
        public string TableName { get; set; }

        [Key]
        public long RowId { get; set; }

        [Key]
        public int Version { get; set; }

        [MaxLength(100)]
        [Required]
        [Key]
        public string ColumnName { get; set; }        

        [Required]
        public string OldValue { get; set; }

        [Required]
        public string NewValue { get; set; }
    }
}
