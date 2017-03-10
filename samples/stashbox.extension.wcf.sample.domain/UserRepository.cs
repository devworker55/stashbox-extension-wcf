using System.Collections.Generic;

namespace stashbox.extension.wcf.sample.domain
{
    public interface IUserRepository
    {
        User GetById(int userId);
    }

    public class UserRepository : IUserRepository
    {
        private readonly List<User> _users;

        public UserRepository()
        {
            _users = new List<User>
            {
                new User { Age = 25, FirstName = "John", Gender = 'M', Id = 1, LastName = "Leesack"},
                new User { Age = 31, FirstName = "Emily", Gender = 'F', Id = 2, LastName = "Bosner"},
                new User { Age = 42, FirstName = "Hue", Gender = 'M', Id = 3, LastName = "Cartner"}
            };
        }

        public User GetById(int userId)
        {
            return _users.Find(u => u.Id == userId);
        }
    }
}
