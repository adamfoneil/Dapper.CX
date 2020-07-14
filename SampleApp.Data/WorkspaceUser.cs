using AO.Models;
using Models.Conventions;
using System.ComponentModel.DataAnnotations;

namespace SampleApp.Data
{
    public class WorkspaceUser : BaseTable
    {
        [Key]
        [References(typeof(Workspace))]
        public int WorkspaceId { get; set; }

        [Key]
        [References(typeof(UserProfile))]
        public int UserId { get; set; }
    }
}
