using System.Threading.Tasks;
using Cw10.Dto;

namespace Cw10.Services
{
    public interface IEnrollmentDbService
    {
        Task<EnrollmentDto> GetBy(string studies, int semester);
        Task<bool> Exists(string studies, int semester);
        Task<EnrollmentDto> EnrollStudent(EnrollStudent model, StudyDto studyDto);
        Task Promotions(Promotions promotions);
    }
}
