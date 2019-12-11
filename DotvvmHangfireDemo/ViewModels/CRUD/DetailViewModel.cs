﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Runtime.Filters;
using DotvvmHangfireDemo.Models;
using DotvvmHangfireDemo.Services;


namespace DotvvmHangfireDemo.ViewModels.CRUD
{
    public class DetailViewModel : MasterPageViewModel
    {
        private readonly StudentService studentService;

        public DetailViewModel(StudentService studentService)
        {
            this.studentService = studentService;
        }

        public StudentDetailModel Student { get; set; }

        public override async Task PreRender()
        {
            int id = Convert.ToInt32(Context.Parameters["Id"]);
            Student = await studentService.GetStudentByIdAsync(id);
            await base.PreRender();
        }
        public async Task DeleteStudent()
        {
            await studentService.DeleteStudentAsync(Student.Id);
            Context.RedirectToRoutePermanent("Default", replaceInHistory: true);
        }
    }
}
