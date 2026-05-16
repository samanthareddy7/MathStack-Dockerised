using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Firebase.Auth;
using MathApi.Models;
using MathApi.Utils; // Ensure your AuthLogger is in this namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace MathApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase // Use ControllerBase for APIs
    {
        private readonly FirebaseAuthProvider _auth;
        private readonly byte[] _key;

        public AuthController(IConfiguration configuration)
        {
            // We pull these from appsettings.json or Environment Variables
            // This is safer for Docker deployment
            var firebaseKey = configuration["Firebase:ApiKey"] ?? Environment.GetEnvironmentVariable("FirebaseMathApp");
            var jwtSecret = configuration["Jwt:Key"] ?? Environment.GetEnvironmentVariable("MathAppJwtKey");

            _auth = new FirebaseAuthProvider(new FirebaseConfig(firebaseKey));
            _key = Encoding.ASCII.GetBytes(jwtSecret);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(LoginModel login)
        {
            try
            {
                // 1. Create user in Firebase
                await _auth.CreateUserWithEmailAndPasswordAsync(login.Email, login.Password);

                // 2. Sign in to get the UserID (LocalId)
                var fbAuthLink = await _auth.SignInWithEmailAndPasswordAsync(login.Email, login.Password);
                
                // 3. Generate the JWT so they don't have to login again immediately
                return Ok(GenerateJwt(fbAuthLink.User.LocalId, fbAuthLink.User.Email));
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(ex.ResponseData);
                return Unauthorized(new { error = firebaseEx.error.message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginModel login)
        {
            try
            {
                // 1. Verify credentials with Firebase
                var fbAuthLink = await _auth.SignInWithEmailAndPasswordAsync(login.Email, login.Password);
                
                // 2. Generate and return the JWT badge
                return Ok(GenerateJwt(fbAuthLink.User.LocalId, fbAuthLink.User.Email));
            }
            catch (FirebaseAuthException ex)
            {
                var firebaseEx = JsonConvert.DeserializeObject<FirebaseErrorModel>(ex.ResponseData);
                
                // Optional: Log the error using your AuthLogger
                // AuthLogger.Instance.LogError(firebaseEx.error.message + " - User: " + login.Email);
                
                return Unauthorized(new { error = firebaseEx.error.message });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
        }

        // Helper Method to create the JWT badge
        private AuthResponse GenerateJwt(string userId, string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // These claims are the "information" written on the badge
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim("UserId", userId) // Used by our MathController [Authorize] attribute
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1), // Badge lasts for 24 hours
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(_key), 
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AuthResponse(tokenHandler.WriteToken(token), userId);
        }

        [HttpPost("Logout")]
        public IActionResult LogOut()
        {
            // JWTs are stateless, so we just tell the client "OK"
            // The client will remove the token from its session
            return Ok();
        }
    }
}