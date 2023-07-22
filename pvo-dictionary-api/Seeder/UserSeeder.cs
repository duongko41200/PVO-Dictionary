using Microsoft.EntityFrameworkCore;
using pvo_dictionary_api.Models;
using pvo_dictionary_api.Common;

namespace pvo_dictionary_api.Seeder
{
    class UserSeeder
    {
        private readonly ModelBuilder _modelBuilder;
        public UserSeeder(ModelBuilder modelBuilder)
        {
            _modelBuilder = modelBuilder;
        }

        /// <summary>
        /// Excute data
        /// </summary>
        public void SeedData()
        {
            _modelBuilder.Entity<User>().HasData(
                new User
                {
                    user_id = 1,
                    user_name = "Quyen",
                    password = "Quyen",
                    email = "Quyen@gmail.com",
                    display_name = "Quyen",
                    full_name = "Quyen",
                    birthday = new DateTime(2000, 06, 06),
                    position = "Quyen",
                    avatar = "",
                    status = 1,
                }
                );
        }
    }
}
