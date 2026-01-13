using System;
using System.Collections.Generic;
using System.Text;

namespace EmployeeApplication.Model.Models
{
    public class ApplicationError
    {
        public Guid ID { get; set; }

        public DateTime When { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
