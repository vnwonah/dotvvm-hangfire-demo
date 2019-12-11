using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Filters;
using DotvvmHangfireDemo.Models;
using DotvvmHangfireDemo.Services;

namespace DotvvmHangfireDemo.ViewModels.CRUD
{
    public class EditViewModel : MasterPageViewModel
    {
        private readonly StudentService studentService;

        public StudentDetailModel Student { get; set; }

        public EditViewModel(StudentService studentService)
        {
            this.studentService = studentService;
        }


        public override async Task PreRender()
        {
			int id = 0;
            if (int.TryParse(Context.Parameters["Id"].ToString(), out id))
            {
                Student = await studentService.GetStudentByIdAsync(id);
            }
            await base.PreRender();
        }


        public async Task EditStudent()
        {
            await studentService.UpdateStudentAsync(Student);
            Context.RedirectToRoute("CRUD_Detail", new {id = Student.Id});
        }
    }
}
