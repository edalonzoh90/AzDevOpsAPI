using Microsoft.Extensions.Configuration;
using RS.api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RS.api.Services
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
        Task<IEnumerable<User>> GetAll();
    }

    public class UserService : IUserService
    {
        private readonly IConfiguration configuration;

        public UserService(IConfiguration configuration)
        {
            string user = configuration.GetSection("AppSettings").GetSection("User").Value;
            string pass = configuration.GetSection("AppSettings").GetSection("Identity").Value;
            this._users = new List<User>
            {

                new User { Id = 1, FirstName = "Test", LastName = "User", Username = user, Password = pass }
            };
        }

        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users { get; set; }

        public async Task<User> Authenticate(string username, string password)
        {
            // wrapped in "await Task.Run" to mimic fetching user from a db
            var user = await Task.Run(() => _users.SingleOrDefault(x => x.Username == username && x.Password == password));

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so return user details
            return user;
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            // wrapped in "await Task.Run" to mimic fetching users from a db
            return await Task.Run(() => _users);
        }
    }
}
