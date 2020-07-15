using AO.Models;
using Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Models
{
    public enum UserStatus
    {
        Disabled = 0,
        Requested = 1,
        Enabled = 2,
        Denied = -1
    }

    public class WorkspaceUser : BaseTable
    {
        [Key]
        [References(typeof(Workspace))]
        public int WorkspaceId { get; set; }

        [Key]
        [References(typeof(UserProfile))]
        public int UserId { get; set; }

        public UserStatus Status { get; set; }
    }
}
