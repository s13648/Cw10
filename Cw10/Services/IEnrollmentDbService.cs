using System.Threading.Tasks;
using Cw10.Dto;

namespace Cw10.Services
{
    public interface IEnrollmentDbService
    {
        Task<Enrollment> GetBy(string studies, int semester);

        Task<bool> Exists(string studies, int semester);

        Task<Enrollment> EnrollStudent(EnrollStudent model, Study study);
        Task Promotions(Promotions promotions);
    }
}
