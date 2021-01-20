using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Cw10.Dto;
using Cw10.Model;
using Microsoft.EntityFrameworkCore;

namespace Cw10.Services
{
    public class StudentDbService : IStudentDbService
    {
        private const string GetStudentsSql = @"SELECT 
	                                        S.FirstName,
	                                        S.LastName,
	                                        S.BirthDate,
	                                        ST.Name AS StudyName,
	                                        E.Semester
                                        FROM 
	                                        [Student] AS S JOIN 
	                                        [Enrollment] AS E ON S.IdEnrollment = E.IdEnrollment JOIN
	                                        [Studies] AS ST ON E.IdStudy = ST.IdStudy
                                        ";

        private const string ExistsQuery = @"SELECT TOP 1 1
                                                FROM 
	                                                [Student] AS ST 
                                                WHERE
	                                                ST.[IndexNumber] = @IndexNumber";


        private const string GetByIndexQuery = @"SELECT 
	                                        S.FirstName,
	                                        S.LastName,
	                                        S.BirthDate,
	                                        ST.Name AS StudyName,
	                                        E.Semester
                                        FROM 
	                                        [Student] AS S JOIN 
	                                        [Enrollment] AS E ON S.IdEnrollment = E.IdEnrollment JOIN
	                                        [Studies] AS ST ON E.IdStudy = ST.IdStudy
                                        WHERE
                                            S.[IndexNumber] = @IndexNumber";

        private const string InsertStudentQuery = @"INSERT INTO [dbo].[Student]
                           ([IndexNumber]
                           ,[FirstName]
                           ,[LastName]
                           ,[BirthDate]
                           ,[IdEnrollment])
                     VALUES
                           (@IndexNumber
                           ,@FirstName
                           ,@LastName
                           ,@BirthDate
                           ,@IdEnrollment)";


        private readonly IConfig config;
        private readonly APBDContext context;

        public StudentDbService(IConfig config,APBDContext context)
        {
            this.config = config;
            this.context = context;
        }

        public async Task<IList<Student>> GetStudents()
        {
            return await context.Students.ToListAsync();
        }

        public async Task<bool> Exists(string indexNumber)
        {
            await using var sqlConnection = new SqlConnection(config.ConnectionString);

            await using var command = new SqlCommand(ExistsQuery, sqlConnection) { CommandType = CommandType.Text };
            command.Parameters.AddWithValue("IndexNumber", indexNumber);

            await sqlConnection.OpenAsync();

            await using var sqlDataReader = await command.ExecuteReaderAsync();
            return await sqlDataReader.ReadAsync();
        }

        public async Task Create(EnrollStudent model, SqlTransaction sqlTransaction, int idEnrollment)
        {
            await using var command = new SqlCommand(InsertStudentQuery, sqlTransaction.Connection)
            {
                CommandType = CommandType.Text,
                Transaction = sqlTransaction
            };

            command.Parameters.AddWithValue("@IndexNumber", model.IndexNumber);
            command.Parameters.AddWithValue("@FirstName", model.FirstName);
            command.Parameters.AddWithValue("@LastName", model.LastName);
            command.Parameters.AddWithValue("@BirthDate", model.BirthDate);
            command.Parameters.AddWithValue("@IdEnrollment", idEnrollment);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<StudentDto> GetByIndex(string index)
        {
            await using var sqlConnection = new SqlConnection(config.ConnectionString);
            await using var command = new SqlCommand(GetByIndexQuery, sqlConnection) { CommandType = CommandType.Text };
            command.Parameters.AddWithValue("IndexNumber", index);

            await sqlConnection.OpenAsync();

            await using var sqlDataReader = await command.ExecuteReaderAsync();
            while (await sqlDataReader.ReadAsync())
            {
                return new StudentDto
                {
                    BirthDate = DateTime.Parse(sqlDataReader[nameof(StudentDto.BirthDate)]?.ToString()),
                    FirstName = sqlDataReader[nameof(StudentDto.FirstName)].ToString(),
                    LastName = sqlDataReader[nameof(StudentDto.LastName)].ToString(),
                    // Semester = int.Parse(sqlDataReader[nameof(StudentDto.Semester)].ToString()),
                    // StudyName = sqlDataReader[nameof(StudentDto.StudyName)].ToString()
                };
            }

            return null;
        }

        public async Task Update(StudentDto studentDto, string indexNumber)
        {
            var student = await context.Students.FirstOrDefaultAsync(n => n.IndexNumber == indexNumber);
            
            student.LastName = studentDto.LastName;
            student.FirstName = studentDto.FirstName;
            student.BirthDate = studentDto.BirthDate;
            
            await context.SaveChangesAsync();
        }
    }
}
