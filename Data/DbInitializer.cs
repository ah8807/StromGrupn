using web.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(StromGrupnContext context)
        {
            context.Database.EnsureCreated();

            if(context.Roles.Any()){
                return; //DB was seeded
            }

            var roles = new IdentityRole[] {
                new IdentityRole{Id="1", Name="Administrator"},
                new IdentityRole{Id="2", Name="Trener"},
                new IdentityRole{Id="3", Name="Plavalec"}
            };

            foreach (IdentityRole r in roles)
            {
                context.Roles.Add(r);
            }

            context.SaveChanges();
           
            var hasher = new PasswordHasher<ApplicationUser>();
            var admin = new ApplicationUser{
                    FirstName = "Admin",
                    LastName = "Admin",
                    UserName = "admin@example.com",
                    NormalizedUserName = "ADMIN@EXAMPLE.COM",
                    Email = "admin@example.com",
                    NormalizedEmail = "ADMIN@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false,
                    TwoFactorEnabled = false,
                    };

            admin.PasswordHash = hasher.HashPassword(admin,"Testni123!");
            context.Users.Add(admin);
            context.SaveChanges();

            var trenerjiUsers = new ApplicationUser[]{
                new ApplicationUser{
                    FirstName = "Marko",
                    LastName = "Varko",
                    UserName = "marko.varko@example.com",
                    NormalizedUserName = "MARKO.VARKO@EXAMPLE.COM",
                    Email = "marko.varko@example.com",
                    NormalizedEmail = "MARKO.VARKO@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false,
                    TwoFactorEnabled = false,
                    },
                new ApplicationUser{
                    FirstName = "Branka",
                    LastName = "Danka",
                    UserName = "branka.danka@example.com",
                    NormalizedUserName = "BRANKA.DANKA@EXAMPLE.COM",
                    Email = "branka.danka@example.com",
                    NormalizedEmail = "BRANKA.DANKA@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false,
                    TwoFactorEnabled = false,
                    },
                new ApplicationUser{
                    FirstName = "Miki",
                    LastName = "Milan",
                    UserName = "miki.milan@example.com",
                    NormalizedUserName = "MIKI.MILAN@EXAMPLE.COM",
                    Email = "miki.milan@example.com",
                    NormalizedEmail = "MIKI.MILAN@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false,
                    TwoFactorEnabled = false,
                    }
            };

            trenerjiUsers[0].PasswordHash = hasher.HashPassword(trenerjiUsers[0],"Geslo123");
            trenerjiUsers[1].PasswordHash = hasher.HashPassword(trenerjiUsers[1],"Geslo123");
            trenerjiUsers[2].PasswordHash = hasher.HashPassword(trenerjiUsers[2],"Geslo123");
            
            foreach(ApplicationUser r in trenerjiUsers)
            {
                context.Users.Add(r);
            }

            context.SaveChanges();

            var plavalciUsers = new ApplicationUser[]{
                new ApplicationUser{
                    FirstName = "Marko",
                    LastName = "Zerjal",
                    UserName = "marko.zerjal@example.com",
                    NormalizedUserName = "MARKO.ZERJAL@EXAMPLE.COM",
                    Email = "marko.zerjal@example.com",
                    NormalizedEmail = "MARKO.ZERJAL@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false,
                    TwoFactorEnabled = false,
                    },
                new ApplicationUser{
                    FirstName = "Mateja",
                    LastName = "Novak",
                    UserName = "mateja.novak@example.com",
                    NormalizedUserName = "MATEJA.NOVAK@EXAMPLE.COM",
                    Email = "mateja.novak@example.com",
                    NormalizedEmail = "MATEJA.NOVAK@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false,
                    TwoFactorEnabled = false,
                    },
                new ApplicationUser{
                    FirstName = "Boris",
                    LastName = "Jereb",
                    UserName = "boris.jereb@example.com",
                    NormalizedUserName = "BORIS.JEREB@EXAMPLE.COM",
                    Email = "boris.jereb@example.com",
                    NormalizedEmail = "BORIS.JEREB@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false, 
                    TwoFactorEnabled = false,
                    },
                new ApplicationUser{
                    FirstName = "Katarina",
                    LastName = "Zelnik",
                    UserName = "katarina.zelnik@example.com",
                    NormalizedUserName = "KATARINA.ZELNIK@EXAMPLE.COM",
                    Email = "katarina.zelnik@example.com",
                    NormalizedEmail = "KATARINA.ZELNIK@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false,
                    TwoFactorEnabled = false,
                    },
                new ApplicationUser{
                    FirstName = "Darina",
                    LastName = "Kastelic",
                    UserName = "darina.kastelic@example.com",
                    NormalizedUserName = "DARINA.KASTELIC@EXAMPLE.COM",
                    Email = "darina.kastelic@example.com",
                    NormalizedEmail = "DARINA.KASTELIC@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false,
                    TwoFactorEnabled = false,
                    },
                new ApplicationUser{
                    FirstName = "Marjan",
                    LastName = "Boris",
                    UserName = "marjan.boris@example.com",
                    NormalizedUserName = "MARJAN.BORIS@EXAMPLE.COM",
                    Email = "marjan.boris@example.com",
                    NormalizedEmail = "MARJAN.BORIS@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false,
                    TwoFactorEnabled = false,
                    },
                new ApplicationUser{
                    FirstName = "Branko",
                    LastName = "Batelj",
                    UserName = "branko.batelj@example.com",
                    NormalizedUserName = "BRANKO.BATELJ@EXAMPLE.COM",
                    Email = "branko.batelj@example.com",
                    NormalizedEmail = "BRANKO.BATELJ@EXAMPLE.COM",
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("D"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("D"),
                    PhoneNumber = "+111111111111",
                    PhoneNumberConfirmed = true,
                    LockoutEnabled = false,
                    TwoFactorEnabled = false,
                    }
            };

            plavalciUsers[0].PasswordHash = hasher.HashPassword(plavalciUsers[0],"Geslo123");
            plavalciUsers[1].PasswordHash = hasher.HashPassword(plavalciUsers[1],"Geslo123");
            plavalciUsers[2].PasswordHash = hasher.HashPassword(plavalciUsers[2],"Geslo123");
            plavalciUsers[3].PasswordHash = hasher.HashPassword(plavalciUsers[3],"Geslo123");
            plavalciUsers[4].PasswordHash = hasher.HashPassword(plavalciUsers[4],"Geslo123");
            plavalciUsers[5].PasswordHash = hasher.HashPassword(plavalciUsers[5],"Geslo123");
            plavalciUsers[6].PasswordHash = hasher.HashPassword(plavalciUsers[6],"Geslo123");
            

            foreach(ApplicationUser r in plavalciUsers)
            {
                context.Users.Add(r);
            }

            context.SaveChanges();


            var UserRoles = new IdentityUserRole<string>[]
            {
                new IdentityUserRole<string>{RoleId = roles[0].Id, UserId=admin.Id},
                new IdentityUserRole<string>{RoleId = roles[1].Id, UserId=trenerjiUsers[0].Id},
                new IdentityUserRole<string>{RoleId = roles[1].Id, UserId=trenerjiUsers[1].Id},
                new IdentityUserRole<string>{RoleId = roles[1].Id, UserId=trenerjiUsers[2].Id},
                new IdentityUserRole<string>{RoleId = roles[2].Id, UserId=plavalciUsers[0].Id},
                new IdentityUserRole<string>{RoleId = roles[2].Id, UserId=plavalciUsers[1].Id},
                new IdentityUserRole<string>{RoleId = roles[2].Id, UserId=plavalciUsers[2].Id},
                new IdentityUserRole<string>{RoleId = roles[2].Id, UserId=plavalciUsers[3].Id},
                new IdentityUserRole<string>{RoleId = roles[2].Id, UserId=plavalciUsers[4].Id},
                new IdentityUserRole<string>{RoleId = roles[2].Id, UserId=plavalciUsers[5].Id},
                new IdentityUserRole<string>{RoleId = roles[2].Id, UserId=plavalciUsers[6].Id}
            };

            foreach (IdentityUserRole<string> r in UserRoles)
            {
                context.UserRoles.Add(r);
            }
            context.SaveChanges();

        }
    }
}