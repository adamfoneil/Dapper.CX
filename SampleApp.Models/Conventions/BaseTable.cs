using AO.Models;
using AO.Models.Attributes;
using AO.Models.Enums;
using AO.Models.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace Models.Conventions
{
    [Identity(nameof(Id))]
    public abstract class BaseTable : IAudit
    {
        public int Id { get; set; }

        [MaxLength(50)]
        [Required]
        [SaveAction(SaveAction.Insert)]
        [NoChangeTracking]
        public string CreatedBy { get; set; }

        [SaveAction(SaveAction.Insert)]
        [NoChangeTracking]
        public DateTime DateCreated { get; set; }

        [SaveAction(SaveAction.Update)]
        [MaxLength(50)]
        [NoChangeTracking]
        public string ModifiedBy { get; set; }

        [SaveAction(SaveAction.Update)]
        [NoChangeTracking]
        public DateTime? DateModified { get; set; }

        public void Stamp(SaveAction saveAction, IUserBase user)
        {
            switch (saveAction)
            {
                case SaveAction.Insert:
                    CreatedBy = user.Name;
                    DateCreated = user.LocalTime;
                    break;

                case SaveAction.Update:
                    ModifiedBy = user.Name;
                    DateModified = user.LocalTime;
                    break;
            }
        }

        public override bool Equals(object obj)
        {
            var test = obj as BaseTable;
            return (test != null) ? test.Id == Id && Id != 0 : false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
