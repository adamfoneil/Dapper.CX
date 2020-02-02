using AO.DbSchema.Attributes.Interfaces;
using AO.DbSchema.Attributes.Models;
using System.Data;
using System.Threading.Tasks;
using Tests.Models;

namespace Tests.SqlServer.Models
{
    public class EmployeeValid : Employee, IValidate<Employee>
    {
        public ValidationResult Validate()
        {
            if (TermDate < HireDate)
            {
                return new ValidationResult()
                {
                    Message = "TermDate cannot be before HireDate",
                    IsValid = false
                };
            }

            return new ValidationResult() { IsValid = true };
        }

        public Task<ValidationResult> ValidateAsync(IDbConnection connection)
        {
            return Task.FromResult(new ValidationResult() { IsValid = true });
        }
    }
}
