using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Cw4.DTOs.Requests;
using Cw4.Models;
using Cw4.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cw4.Controllers
{
    [Route("api/students")]
    [ApiController]


    public class StudentsController : ControllerBase
    {
        //Utworzony własnie Interface (wewnetrzny?)
        public IConfiguration Configuration { get; set; }

        //Wstrzykiwanie kodu
        public StudentsController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetStudents([FromServices] IStudentsDAL dbService)
        {
            var list = dbService.GetStudents();
            return Ok(list);
        }

        [HttpGet("{IndexNumber}")]
        [Authorize]
        public IActionResult GetStudents(string IndexNumber, [FromServices] IStudentsDAL dbService)
        {
            var list = dbService.GetStudents(IndexNumber);
            return Ok(list);
        }

        [HttpPost]
        public IActionResult Login(LoginRequest request, [FromServices] IStudentsDAL dbService, [FromServices] IPasswordDAL dbService2, [FromServices] ITokenDAL dbService3)
        {
            //Loginem jest index
            //Sprawdz w DB, czy user i hasło poprawne
            //Console.WriteLine(dbService.LoginCredentialsCorrect(request.Login, request.Password));
            //Console.WriteLine(request.Password == "");

            // 0. Check if uset in DB
            if (!dbService.StudentExists(request.Login))
            {
                return BadRequest("Student nie istnieje w bazie");
            }

            

            // 1. Get user salt from db
            var saltDB = dbService2.GetSalt(request.Login);
            var saltedPassDB = dbService2.GetHashPass(request.Login); 

            // 2. get hash of the pass from http request
            var saltedPass = IPasswordDAL.CreateHash(request.Password, saltDB);

            //3. Check if salted pass from DB and now created hash are the same
            if (! (saltedPassDB == saltedPass))
                return BadRequest("Niepoprawne haslo. ");

            //if (!(dbService.LoginCredentialsCorrect(request.Login, request.Password)))
            //    return BadRequest("Incorrect login credentials. ");

            Student s = dbService.GetStudent(request.Login);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, request.Login),
                new Claim(ClaimTypes.Name, s.FirstName+" "+s.LastName),
                new Claim(ClaimTypes.Role, "student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            var refreshToken = Guid.NewGuid();
            //Dodaj refToken do DB
            dbService3.InsertToken(refreshToken.ToString());

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token), // Żyje 5-10 minut
                refreshToken
            });

            
        }
        
        [HttpPost("refresh-token{token}")]
        public IActionResult RefreshToken(string refToken, [FromServices] ITokenDAL dbService)
        {
            //spr w BD
            if(!dbService.TokenExistsInDB(refToken))
                return NotFound();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, ""),
                new Claim(ClaimTypes.Name, ""),
                new Claim(ClaimTypes.Role, "student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token), // Żyje 5-10 minut
                refreshToken = Guid.NewGuid()
            });
        }
    }
}