using DotVVM.Framework.Controls;
using DotVVM.Framework.Storage;
using DotvvmHangfireDemo.Services;
using Hangfire;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotvvmHangfireDemo.ViewModels.CRUD
{
    public class BulkCreateViewModel : MasterPageViewModel
    {
        private readonly StudentService studentService;
        private IUploadedFileStorage storage;
        public bool CanProcess { get; set; }
        public string Message { get; set; }
        public UploadedFilesCollection Files { get; set; }

        public BulkCreateViewModel(
            StudentService studentService,
            IUploadedFileStorage storage)
        {
            this.studentService = studentService;
            this.storage = storage;

            Files = new UploadedFilesCollection();
            CanProcess = false;
            Message = string.Empty;
        }

        public void ProcessFile()
        {
            // do what you have to do with the uploaded files
            CanProcess = true;
        }

        public void Process()
        {
            var uploadPath = GetUploadPath();

            // save all files to disk
            foreach (var file in Files.Files)
            {
                var targetPath = Path.Combine(uploadPath, file.FileId + ".csv");
                storage.SaveAs(file.FileId, targetPath);
                BackgroundJob.Enqueue(() => studentService.ProcessUploadedFile(file.FileId));
            }

            // clear the uploaded files collection so the user can continue with other files
            Files.Clear();
            CanProcess = false;
            Message = "Students are being inserted in the background";
        }

        private string GetUploadPath()
        {
            var uploadPath = Path.Combine(Context.Configuration.ApplicationPhysicalPath, "MyFiles");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            return uploadPath;
        }
    }
}
