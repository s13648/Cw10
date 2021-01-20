using System.Linq;
using System.Threading.Tasks;
using Cw10.Dto;
using Cw10.Model;
using Microsoft.EntityFrameworkCore;

namespace Cw10.Services
{
    public class StudyDbService : IStudyDbService
    {
        private readonly APBDContext context;

        public StudyDbService(APBDContext context)
        {
            this.context = context;
        }

        public async Task<StudyDto> GetByName(string name)
        {
            return await context.Studies
                .Where(n => n.Name == name)
                .Select(n => new StudyDto
                {
                    Name = n.Name,
                    IdStudy = n.IdStudy
                })
                .FirstOrDefaultAsync();
        }
    }
}
