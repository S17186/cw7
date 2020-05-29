using Cw4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cw4.Service
{
    public interface IStudentsDAL
    {
        public IEnumerable<Student> GetStudents();
        public IEnumerable<Student> GetStudents(string indexNum);
        public Student GetStudent(string indexNum);
        public bool StudentExists(string indexNum);
        public bool LoginCredentialsCorrect(string indexNum, string passHash, string salt);
    }
}
