using Dapper.TinyCrud.Base.Attributes;
using System;

namespace Tests.Models
{
    [Identity(nameof(Id))]
    public class Greeting
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public DateTime CurrentTime { get; set; }
    }
}
