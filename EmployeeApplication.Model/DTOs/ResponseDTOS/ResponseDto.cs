using System;
using System.Collections.Generic;
using System.Text;

namespace EmployeeApplication.Model.DTOs.ResponseDTOS
{
    public class ResponseDto
    {
        public object? Result { get; set; }

        public bool IsSuccess { get; set; } = false;

        public string Message { get; set; } = string.Empty;
    }
}
