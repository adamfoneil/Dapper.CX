using AO.Models;
using AO.Models.Enums;
using AO.Models.Interfaces;
using SampleApp.Data.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SampleApp.Models
{
    [Table("AspNetUsers")]
    [Identity(nameof(UserId))]
    public partial class UserProfile : IUserBase
    {
        public int UserId { get; set; }

        [MaxLength(256)]
        [SaveAction(SaveAction.None)]
        public string UserName { get; set; }
        
        [MaxLength(256)]        
        public string Email { get; set; }

        [MaxLength(100)]
        public string TimeZoneId { get; set; }

        [MaxLength(50)]
        public string DisplayName { get; set; }

        [References(typeof(Workspace))]
        public int? WorkspaceId { get; set; }

        public string Name => UserName;

        public DateTime LocalTime => Timestamp.Local(TimeZoneId);

        // AO pwd = Oopsie.Daisy!456
    }
}
