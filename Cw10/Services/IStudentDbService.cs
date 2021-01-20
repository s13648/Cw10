using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Cw10.Dto;

namespace Cw10.Services
{
    public interface IStudentDbService
    {
        public Task<IEnumerable<Student>> GetStudents();

        Task<bool> Exists(string indexNumber);
        
        Task Create(EnrollStudent model, SqlTransaction sqlTransaction, int idEnrollment);
        
        Task<Student> GetByIndex(string index);
    }
}
