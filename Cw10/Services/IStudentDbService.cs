using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Cw10.Dto;
using Cw10.Model;

namespace Cw10.Services
{
    public interface IStudentDbService
    {
        public Task<IList<Student>> GetStudents();

        Task<bool> Exists(string indexNumber);
        
        Task Create(EnrollStudent model, SqlTransaction sqlTransaction, int idEnrollment);
        
        Task<StudentDto> GetByIndex(string index);
    }
}
