using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Cw10.Dto;
using Cw10.Model;
using Microsoft.EntityFrameworkCore;

namespace Cw10.Services
{
    public class EnrollmentDbService : IEnrollmentDbService
    {
        private readonly IStudyDbService studyDbService;
        private readonly IStudentDbService studentDbService;
        private readonly APBDContext context;

        public EnrollmentDbService(
            IStudyDbService studyDbService,
            IStudentDbService studentDbService,
            APBDContext context)
        {
            this.studyDbService = studyDbService;
            this.studentDbService = studentDbService;
            this.context = context;
        }

        public async Task<EnrollmentDto> EnrollStudent(EnrollStudent model, StudyDto studyDto)
        {
            var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                var actualEnrollmentId = await ActualEnrollmentGetByName(model.Studies) ?? await EnrollmentCreate(studyDto.IdStudy);
                await studentDbService.Create(model,  actualEnrollmentId);;

                await transaction.CommitAsync();
                return await GetById(actualEnrollmentId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task Promotions(Promotions promotions)
        {
            var newSemester = promotions.Semester + 1;
            var enrollmentDto = await GetBy(promotions.Studies, newSemester);
            int enrollmentId;
            if (enrollmentDto == null)
            {
                var studyDto = await studyDbService.GetByName(promotions.Studies);
                enrollmentId = await EnrollmentCreate(studyDto.IdStudy, newSemester);
            }
            else
                enrollmentId = enrollmentDto.IdEnrollment;

            var students = await GetStudentsBy(promotions.Studies, promotions.Semester);
            foreach (var student in students)
            {
                student.IdEnrollment = enrollmentId;
            }

            await context.SaveChangesAsync();
        }

        public async Task<EnrollmentDto> GetBy(string studies,int semester)
        {
            return await context.Enrollments
                    .Where(n => n.IdStudyNavigation.Name == studies && n.Semester == semester)
                    .Select(n => new EnrollmentDto
                    {
                        Semester = n.Semester.ToString(),
                        IdEnrollment = n.IdEnrollment,
                        IdStudy = n.IdStudy,
                        StartDate = n.StartDate
                    }).FirstOrDefaultAsync();
        }
        
        private async Task<IList<Student>> GetStudentsBy(string studies,int semester)
        {
            return await context.Enrollments
                .Where(n => n.IdStudyNavigation.Name == studies && n.Semester == semester)
                .Include(n => n.Students)
                .SelectMany(n => n.Students)
                .ToListAsync();
        }

        private async Task<EnrollmentDto> GetById(int id)
        {
            return await context.Enrollments.Where(n => n.IdEnrollment == id)
                .Select(n => new EnrollmentDto
                {
                    Semester = n.Semester.ToString(),
                    IdEnrollment = n.IdEnrollment,
                    IdStudy = n.IdStudy,
                    StartDate = n.StartDate
                })
                .FirstOrDefaultAsync();
        }
        
        private async Task<int> EnrollmentCreate(int idStudy,int semester = 1)
        {
            var maxId = context.Enrollments.Max(n => n.IdEnrollment);
            
            var newEnrollment = new Enrollment
            {
                IdEnrollment = maxId + 1,
                Semester = semester,
                StartDate = DateTime.Now,
                IdStudy = idStudy
            };
            await context.Enrollments.AddAsync(newEnrollment);
            await context.SaveChangesAsync();
            return newEnrollment.IdEnrollment;
        }
        
        private async Task<int?> ActualEnrollmentGetByName(string studyName)
        {
            return await context
                .Enrollments
                .Where(e => e.IdStudyNavigation.Name == studyName && e.Semester == 1)
                .Select(e => (int?)e.IdEnrollment)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> Exists(string studies,int semester)
        {
            return await context.Enrollments.AnyAsync(n => n.IdStudyNavigation.Name == studies && n.Semester == semester);
        }
    }
}
