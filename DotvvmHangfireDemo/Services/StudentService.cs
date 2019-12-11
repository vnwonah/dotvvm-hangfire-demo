using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Storage;
using DotvvmHangfireDemo.DAL;
using DotvvmHangfireDemo.DAL.Entities;
using DotvvmHangfireDemo.Models;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace DotvvmHangfireDemo.Services
{
    public class StudentService
    {
        private readonly StudentDbContext studentDbContext;
        private readonly IUploadedFileStorage storage;

        public StudentService(
            StudentDbContext studentDbContext,
            IUploadedFileStorage storage)
        {
            this.studentDbContext = studentDbContext;
            this.storage = storage;
        }

        public async Task<List<StudentListModel>> GetAllStudentsAsync()
        {

            return await studentDbContext.Students.Select(
                s => new StudentListModel
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName
                }
            ).ToListAsync();
        }

        public async Task<StudentDetailModel> GetStudentByIdAsync(int studentId)
        {
            return await studentDbContext.Students.Select(
                    s => new StudentDetailModel
                    {
                        Id = s.Id,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        About = s.About,
                        EnrollmentDate = s.EnrollmentDate
                    })
                .FirstOrDefaultAsync(s => s.Id == studentId);

        }

        public async Task UpdateStudentAsync(StudentDetailModel student)
        {
            var entity = await studentDbContext.Students.FirstOrDefaultAsync(s => s.Id == student.Id);

            entity.FirstName = student.FirstName;
            entity.LastName = student.LastName;
            entity.About = student.About;
            entity.EnrollmentDate = student.EnrollmentDate;

            await studentDbContext.SaveChangesAsync();
        }

        public async Task InsertStudentAsync(StudentDetailModel student)
        {
            var entity = new Student()
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                About = student.About,
                EnrollmentDate = student.EnrollmentDate
            };

            studentDbContext.Students.Add(entity);
            await studentDbContext.SaveChangesAsync();
        }

        public async Task DeleteStudentAsync(int studentId)
        {
            var entity = new Student()
            {
                Id = studentId
            };
            studentDbContext.Students.Attach(entity);
            studentDbContext.Students.Remove(entity);
            await studentDbContext.SaveChangesAsync();
        }

        public void ProcessUploadedFile(Guid fileId)
        {
            StreamReader file = null;
            try
            {
                string line;
                file = new StreamReader(storage.GetFile(fileId));
                while ((line = file.ReadLine()) != null)
                {
                    //we are getting eac part of the todo into an array. 
                    //parts[0] will be the Todo Text and parts[1] will be the Due date
                    var parts = line.Split(",");
                    //first part of this if statement checks that the Todo Text is a valid non empty text
                    //second part checks that we are passing in a valid date time
                    if (!string.IsNullOrWhiteSpace(parts?[0]) 
                        && !string.IsNullOrWhiteSpace(parts?[1])
                        && !string.IsNullOrWhiteSpace(parts?[3])
                        && DateTime.TryParse(parts?[2], out DateTime enrollmentDate))
                    {
                        var model = new StudentDetailModel
                        {
                            FirstName = parts?[0],
                            LastName = parts?[1],
                            EnrollmentDate = enrollmentDate,
                            About = parts?[3]
                        };
                        //this line creates a background job for each Todo Item
                        BackgroundJob.Enqueue(() => InsertStudentAsync(model));
                    }
                }
                file.Close();
            }
            catch (Exception e)
            {
            }
            finally
            {
                if (file is object)
                    file.Close();

                storage.DeleteFile(fileId);
            
            }
        }

        public void DeleteDuplicateStudents()
        {
            var duplicateStudents = studentDbContext.Students.GroupBy(s => new { s.FirstName, s.LastName }).SelectMany(s => s.OrderBy(y => y.Id).Skip(1));
            studentDbContext.Students.RemoveRange(duplicateStudents);
            studentDbContext.SaveChanges();

        }
    }
}

