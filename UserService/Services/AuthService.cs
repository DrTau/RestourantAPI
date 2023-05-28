using UserService.Database;
using UserService.Models;

namespace UserService.Services{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ISessionService _sessionService;

        public AuthService(AppDbContext context, ISessionService sessionService)
        {
            _context = context;
            _sessionService = sessionService;
        }

        public async Task<Session> LoginAsync(string email, string password)
        {
            // Find the user with the given email
            var user = await _context.Users.FindAsync(email);
            if (user == null)
            {
                throw new ArgumentException("User with this email does not exist");
            }

            // Check if the password is correct
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new ArgumentException("Password is incorrect");
            }

            return await _sessionService.CreateSession(user);
        }

        public async Task<User> RegisterAsync(UserRegistrationDto user)
        {
            // Check if user with this username or email already exists
            var existingUser = await _context.Users.FindAsync(user.Email, user.Name);
            if (existingUser != null)
            {
                throw new ArgumentException("User with this email or username already exists");
            }

            // Hash the password using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

            // Create a new user
            var newUser = new User
            {
                Name = user.Name,
                Email = user.Email,
                PasswordHash = passwordHash
            };

            // Add the user to the database
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }
    }
}