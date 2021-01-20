using System.Collections.Generic;
using System.Threading.Tasks;
using Cw10.Dto;
using Cw10.Model;
using Microsoft.EntityFrameworkCore;

namespace Cw10.Services
{
    public class StudentDbService : IStudentDbService
    {
        private readonly APBDContext context;

        public StudentDbService(APBDContext context)
        {
            this.context = context;
        }

        public async Task<IList<Student>> GetStudents()
        {
            return await context.Students.ToListAsync();
        }

        public async Task<bool> Exists(string indexNumber)
        {
            return await context.Students.AnyAsync(n => n.IndexNumber == indexNumber);
        }

        public async Task Create(EnrollStudent model, int idEnrollment)
        {
            var student = new Student
            {
                BirthDate = model.BirthDate,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IdEnrollment = idEnrollment,
                IndexNumber = model.IndexNumber
            };
            await context.Students.AddAsync(student);
            await context.SaveChangesAsync();
        }

        public async Task Update(StudentDto studentDto, string indexNumber)
        {
            var student = await context.Students.FirstOrDefaultAsync(n => n.IndexNumber == indexNumber);
            
            student.LastName = studentDto.LastName;
            student.FirstName = studentDto.FirstName;
            student.BirthDate = studentDto.BirthDate;
            
            await context.SaveChangesAsync();
        }

        public async Task Delete(string id)
        {
            var student = await context.Students.FirstOrDefaultAsync(n => n.IndexNumber == id);
            context.Remove(student);
            await context.SaveChangesAsync();
        }
    }
}
