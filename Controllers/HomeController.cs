using ChatApp.Models;
using ChatApp.Services;
using ChatApp.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Policy;
using System.Security.Cryptography;
//using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MongoDbService _mongoDbService;
        //private readonly IHubContext<ChatHub> _hubContext;

        public HomeController(ILogger<HomeController> logger, MongoDbService mongoDbService)
        {
            _logger = logger;
            _mongoDbService = mongoDbService;
			//_hubContext = hubContext;
		}

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> TestMongoDb()
        {
            var messages = await _mongoDbService.GetMessagesAsync();
            return View(messages);
        }

        // Register a user.
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            // Check if username already exists
            var existingUser = await _mongoDbService.GetUserByUsernameAsync(username);
            if (existingUser != null)
            {
                ViewData["ErrorMessage"] = "Username already exists.";
                return View();
            }

            // Generate salt and hash for password
            byte[] salt = GenerateSalt();
            byte[] passwordHash = Convert.FromBase64String(HashPassword(password, salt));

            // Create new user
            var newUser = new User
            {
                Username = username,
                Salt = Convert.ToBase64String(salt),
                PasswordHash = Convert.ToBase64String(passwordHash)
            };

            // Save user to database
            await _mongoDbService.CreateUserAsync(newUser);

            return RedirectToAction("Login", "Home");
        }

        // GET: Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _mongoDbService.GetUserByUsernameAsync(username);
            if (user != null && VerifyPassword(password, Convert.FromBase64String(user.Salt), user.PasswordHash))
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("UserID", user.Id.ToString())
        };
                var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");

                await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity));

                // Store user information in session
                HttpContext.Session.SetString("UserID", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username);

                return RedirectToAction("Index", "Home");
            }

            ViewData["ErrorMessage"] = "Invalid username or password.";
            return View();
        }


        // GET: Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            HttpContext.Session.Clear(); // Clear session data
            return RedirectToAction("Login", "Home");
        }

        // Fetch Users
        public async Task<IActionResult> Chat()
        {
            var currentUserId = HttpContext.Session.GetString("UserID");
            if (currentUserId == null) return RedirectToAction("Login");

            var users = await _mongoDbService.GetUsersAsync();
            var otherUsers = users.Where(u => u.Id != currentUserId).ToList();
            return View(otherUsers);
        }

        public async Task<IActionResult> ChatWithUser(string receiverId)
        {
            var senderId = HttpContext.Session.GetString("UserID");
            if (senderId == null) return RedirectToAction("Login");

            // Fetch chat messages between the current user and selected user
            var messages = await _mongoDbService.GetMessagesBetweenUsersAsync(senderId, receiverId);
            var receiverUser = await _mongoDbService.GetUserByIDAsync(receiverId);

            var viewModel = new ChatViewModel
            {
                ReceiverUsername = receiverUser.Username,
                Messages = messages,
                ReceiverId = receiverId,
                SenderId = senderId
            };
            return View(viewModel);
        }



        // Messages..

        [HttpPost]
        public async Task<IActionResult> SendMessage(string receiverId, string messageContent)
        {
            var senderId = HttpContext.Session.GetString("UserID");
            if (senderId == null) return RedirectToAction("Login");

            var newMessage = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                MessageContent = messageContent,
                Timestamp = DateTime.Now
            };

            await _mongoDbService.CreateMessageAsync(newMessage);

            // Redirect back to the chat with updated messages
            return RedirectToAction("ChatWithUser", new { receiverId });
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Helper method to generate Salt
        private byte[] GenerateSalt(int size = 16)
        {
            var salt = new byte[size];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        //Helper method to create PasswordHash
        private string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }


        // Helper method to verify password hash
        private bool VerifyPassword(string password, byte[] salt, string hash)
        {
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));

            return hashedPassword == hash;
        }
    }
}
